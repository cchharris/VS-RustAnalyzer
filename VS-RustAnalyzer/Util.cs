using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace VS_RustAnalyzer
{
    internal class Util
    {
        public const string RustFileExtension = ".rs";
        public const string CargoFileExtension = ".toml";
        public static bool IsRustFile(string path)
        {
            return path.EndsWith(RustFileExtension);
        }

        public static Regex cargoMatcher = new Regex(@"(?i)\bcargo\.toml\b", RegexOptions.Compiled);
        public static bool IsCargoFile(string path)
        {
            return cargoMatcher.IsMatch(path);
        }

        public static string GetCargoFileForPath(string filePath, string projectRoot)
        {
            var currentPath = Path.GetDirectoryName(filePath);

            while (true)
            {
                var candidateCargoPath = Path.Combine(currentPath, "cargo.toml");
                if (currentPath.Equals(projectRoot, StringComparison.OrdinalIgnoreCase))
                {
                    return candidateCargoPath;
                }

                if (File.Exists(candidateCargoPath))
                {
                    return candidateCargoPath;
                }

                currentPath = Path.GetDirectoryName(currentPath);
            }
        }
    }
}
