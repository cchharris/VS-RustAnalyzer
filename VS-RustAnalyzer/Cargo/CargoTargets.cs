using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VS_RustAnalyzer.Cargo.Toml;

namespace VS_RustAnalyzer.Cargo
{
    internal class CargoTargets
    {
        private CargoManifest _manifest;
        public CargoTargets(CargoManifest manifest)
        {
            _manifest = manifest;
        }

        private CargoToml Toml => _manifest.Toml;

        // Only one name allowed per Type
        private Dictionary<TargetType, Dictionary<string, CargoTarget>> _targetsByTypeByName = new Dictionary<TargetType, Dictionary<string, CargoTarget>>();
        // Only to be used to stop loading inferred paths if there is a defined one to the same path
        private Dictionary<string, CargoTarget> _targetsByPath = new Dictionary<string, CargoTarget>();
        private Dictionary<TargetType, List<CargoTarget>> _targetsByType = new Dictionary<TargetType, List<CargoTarget>>();
        bool _loaded = false;

        public void ClearCache()
        {
            _targetsByTypeByName.Clear();
            _targetsByPath.Clear();
            _targetsByType.Clear();
            _loaded = false;
        }

        public static string MapProfileToTargetFolderName(string profile)
        {
            switch (profile)
            {
                case CargoManifest.ProfileDev:
                case CargoManifest.ProfileTest:
                    return "debug";
                case CargoManifest.ProfileRelease:
                case CargoManifest.ProfileBench:
                    return "release";
                default: // Custom profile
                    return profile;

            }
        }

        private static string SuffixForCrateType(CrateType type)
        {
            switch (type)
            {
                case CrateType.Bench:
                case CrateType.Test:
                case CrateType.Binary: return ".exe";
                case CrateType.RustLibrary:
                case CrateType.Library: return ".rlib";
                case CrateType.StaticLibrary: return ".lib";
                case CrateType.CDynamicLibrary:
                case CrateType.DynamicLibrary: return ".dll";
                default:
                    throw new ArgumentException("Unknown crate type");
            }
        }

        private static IEnumerable<string> TargetPathsForCrate(string profile, string name, IEnumerable<CrateType> crate, string extra = "")
        {
            foreach(var c in crate)
            {
                if (string.IsNullOrEmpty(extra))
                    yield return Path.Combine("target", MapProfileToTargetFolderName(profile), $"{name}{SuffixForCrateType(c)}");
                else
                    yield return Path.Combine("target", MapProfileToTargetFolderName(profile), extra, $"{name}{SuffixForCrateType(c)}");
            }
        }

        /**
         * Example 
         *      src/
         *          bin/
         *             multifile/
         *                 main.rs
         *             singlefile.rs
         *
         * Toml
         * [[bin]]
         *  name = multifile
         *  path = src/bin/singlefile.rs
         * 
         * A target with name singlefile doesn't exist
         * A target with multifile exists, consisting of singlefile.rs
         * src/bin/multifile/main.rs does not get compiled into a target
         *
         * If path isnt' set, multifile has a failing test, and test=false, the test won't run when we run `cargo test`,
         * so we can obviously also map to 
         *
         * Our algorithm:
         *  Load all toml targets
         *  Load all inferred targets, whose names aren't already taken by a toml target
         *  Map them to Toml based on Path
         *   If Path isn't defined in Toml, map based on name
         *
         */
        public void LoadTargets()
        {
            if (_loaded) return;

            void AddTargetOfType(TargetType type, CargoTarget target)
            {
                if (!_targetsByType.TryGetValue(type, out List<CargoTarget> targets))
                {
                    targets = new List<CargoTarget>();
                    _targetsByType[type] = targets;
                }
                targets.Add(target);
            }

            void TrackTargetNameOfType(TargetType type, string name, CargoTarget target)
            {
                if (!_targetsByTypeByName.TryGetValue(type, out Dictionary<string, CargoTarget> targets))
                {
                    targets = new Dictionary<string, CargoTarget>();
                    _targetsByTypeByName[type] = targets;
                }
                targets[name] = target;
            }

            // Load from TOML
            var definedTargets = Toml.AllTargets;
            foreach (CargoTargetToml tt in definedTargets)
            {
                string name = tt.Name;
                string path = tt.PathDefined ? tt.Path : InferPathFromName(name, tt.Type);

                var target = new CargoTarget(_manifest.Path,
                    name,
                    path,
                    tt.Type,
                    (profile) => TargetPathsForCrate(profile, name, tt.CrateTypes, tt.Type == TargetType.Example ? "examples" : string.Empty),
                    tt);

                TrackTargetNameOfType(tt.Type, name, target);
                _targetsByPath[NormalizePath(path)] = target;
                AddTargetOfType(tt.Type, target);
            }

            // Filter out targets whose name is already being squatted by a Toml target,
            // Or an inferred target which has a target already targeting its path
            var inferredTargets = EnumerateAllFileSystemInferredTargets()
                .Where(t => !(_targetsByTypeByName.TryGetValue(t.TargetType, out Dictionary<string, CargoTarget> trackedTargets) && trackedTargets.ContainsKey(t.Name)) && !_targetsByPath.ContainsKey(NormalizePath(t.SrcPath)));
            foreach (var it in inferredTargets)
            {
                var target = new CargoTarget(_manifest.Path,
                    it.Name,
                    it.SrcPath,
                    it.TargetType,
                    (profile) => TargetPathsForCrate(profile, it.Name, new CrateType[] { it.CrateType }, it.TargetType == TargetType.Example ? "example" : string.Empty));
                TrackTargetNameOfType(it.TargetType, it.Name, target);
                _targetsByPath[NormalizePath(it.SrcPath)] = target;
                AddTargetOfType(it.TargetType, target);
            }

            _loaded = true;
        }

        public IEnumerable<ICargoTarget> EnumerateTargetsByType(TargetType type)
        {
            if (_targetsByType.TryGetValue(type, out List<CargoTarget> targets))
                return targets;
            return Enumerable.Empty<ICargoTarget>();
        }

        private IEnumerable<CargoTargetInferred> EnumerateAllFileSystemInferredTargets()
        {
            var lib = FileSystemInferredLib();
            if (lib != null)
                yield return lib;

            if (Toml.Package.AutoBins)
            {
                foreach (var target in EnumerateFileSystemInferredBins())
                {
                    yield return target;
                }
            }

            if (Toml.Package.AutoExamples)
            {
                foreach (var target in EnumerateFileSystemInferredExamples())
                {
                    yield return target;
                }
            }
            if (Toml.Package.AutoTests)
            {
                foreach (var target in EnumerateFileSystemInferredTests())
                {
                    yield return target;
                }
            }
            if (Toml.Package.AutoBenches)
            {
                foreach (var target in EnumerateFileSystemInferredBenches())
                {
                    yield return target;
                }
            }
        }

        private IEnumerable<string> EnumerateDirectoriesContainingMain(string path)
        {
            return Directory.EnumerateDirectories(path).Where(DirectoryContainsMain);
        }

        private bool DirectoryContainsMain(string dir)
        {
            return File.Exists(Path.Combine(dir, "main.rs"));
        }

        private IEnumerable<CargoTargetInferred> EnumerateFileSystemInferredBins()
        {
            var directoryContainingCargo = Path.GetDirectoryName(_manifest.Path);
            var srcPath = Path.Combine(directoryContainingCargo, "src");
            var defaultMain = Path.Combine(srcPath, "main.rs");

            if (File.Exists(defaultMain))
            {
                yield return InferredCargoTargetFor(PackageNameAsTarget(), defaultMain, string.Empty, TargetType.Binary, CrateType.Binary);
            }
            var binPath = Path.Combine(srcPath, "bin");
            foreach (var target in EnumerateFileSystemForInferredTargets(binPath, string.Empty, TargetType.Binary, CrateType.Binary))
            {
                yield return target;
            }
        }

        private CargoTargetInferred FileSystemInferredLib()
        {
            var directoryContainingCargo = Path.GetDirectoryName(_manifest.Path);
            var srcPath = Path.Combine(directoryContainingCargo, "src");
            var defaultLib = Path.Combine(srcPath, "lib.rs");
            if (File.Exists(defaultLib))
            {
                return InferredCargoTargetFor(PackageNameAsTarget(), defaultLib, string.Empty, TargetType.Library, CrateType.RustLibrary);
            }
            return null;
        }

        private IEnumerable<CargoTargetInferred> EnumerateFileSystemInferredBenches()
        {
            var directoryContainingCargo = Path.GetDirectoryName(_manifest.Path);
            var benchesPath = Path.Combine(directoryContainingCargo, "benches");
            foreach (var target in EnumerateFileSystemForInferredTargets(benchesPath, string.Empty, TargetType.Bench, CrateType.Bench))
            {
                yield return target;
            }
        }

        private IEnumerable<CargoTargetInferred> EnumerateFileSystemInferredExamples()
        {
            var directoryContainingCargo = Path.GetDirectoryName(_manifest.Path);
            var examplesPath = Path.Combine(directoryContainingCargo, "examples");
            foreach (var target in EnumerateFileSystemForInferredTargets(examplesPath, "examples", TargetType.Example, CrateType.Binary))
            {
                yield return target;
            }
        }

        private IEnumerable<CargoTargetInferred> EnumerateFileSystemInferredTests()
        {
            var directoryContainingCargo = Path.GetDirectoryName(_manifest.Path);
            var testsPath = Path.Combine(directoryContainingCargo, "tests");
            foreach (var target in EnumerateFileSystemForInferredTargets(testsPath, string.Empty, TargetType.Test, CrateType.Test))
            {
                yield return target;
            }
        }


        private IEnumerable<CargoTargetInferred> EnumerateFileSystemForInferredTargets(string dir, string extra, TargetType type, CrateType crate)
        {
            if (Directory.Exists(dir))
            {
                var files = Directory.EnumerateFiles(dir, "*.rs");
                foreach (var file in files)
                {
                    var fileName = Path.GetFileNameWithoutExtension(file);
                    yield return InferredCargoTargetFor(fileName, file, extra, type, crate);
                }

                foreach (var d in EnumerateDirectoriesContainingMain(dir))
                {
                    var exeName = new DirectoryInfo(d).Name;
                    yield return InferredCargoTargetFor(exeName, Path.Combine(d, "main.rs"), extra, type, crate);
                }
            }
        }

        private CargoTargetInferred InferredCargoTargetFor(string name, string path, string extra, TargetType type, CrateType crate)
        {
            return new CargoTargetInferred(name, new Uri(_manifest.Path).MakeRelativeUri(new Uri(path)).OriginalString, _manifest.Path, type, crate);
        }

        private string PackageNameAsTarget()
        {
            return _manifest.PackageName.Replace('-', '_');
        }

        public string InferPathFromName(string name, TargetType type)
        {
            switch (type)
            {
                case TargetType.Library:
                    return Path.Combine("src", "lib.rs");
                case TargetType.Test:
                    return InferFilePathFromName(name, Path.Combine(_manifest.Path, "tests"), "test");
                case TargetType.Example:
                    return InferFilePathFromName(name, Path.Combine(_manifest.Path, "examples"), "examples");
                case TargetType.Bench:
                    return InferFilePathFromName(name, Path.Combine(_manifest.Path, "benches"), "benches");
                case TargetType.Binary:
                    return InferFilePathFromName(name, Path.Combine(_manifest.Path, "src", "bin"), Path.Combine("src", "bin"));
                default: throw new InvalidOperationException("Inferring path from an unknown TargetType");
            }
        }

        private string InferFilePathFromName(string name, string fullParentPath, string relativeFromCargoPath)
        {
            if (File.Exists(Path.Combine(fullParentPath, $"{name}.rs")))
                return Path.Combine(relativeFromCargoPath, $"{name}.rs");
            if (DirectoryContainsMain(Path.Combine(fullParentPath, name)))
                return Path.Combine(relativeFromCargoPath, name, "main.rs");
            return null;
        }

        private string NormalizePath(string path)
        {
            return path.Replace('\\', '/');
        }
    }
}
