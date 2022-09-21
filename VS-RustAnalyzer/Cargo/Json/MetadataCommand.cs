using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VS_RustAnalyzer.Cargo.Json
{
    /*
     * See https://doc.rust-lang.org/cargo/commands/cargo-metadata.html
     */
    [Serializable]
    public class MetadataCommand
    {
        #region MetadataCommand Properties
        [JsonProperty("packages")]
        public Package[] Packages { get; set; }
        [JsonProperty("workspace_members")]
        public string[] WorkspaceMembers { get; set; }
        /**
            Null if `--no-deps` specified
         */
        [JsonProperty("resolve")]
        public ResolvedDependencies Resolve { get; set; }
        [JsonProperty("target_directory")]
        public string TargetDirectory { get; set; }
        [JsonProperty("version")]
        public string Version { get; set; }
        [JsonProperty("workspace_root")]
        public string WorkspaceRoot { get; set; }
        [JsonProperty("metadata")]
        public Dictionary<string, object> Metadata { get; set; }
        #endregion

        #region InnerData
        [Serializable]
        public class Package
        {
            [JsonProperty("name")]
            public string Name { get; set; }
            [JsonProperty("version")]
            public string Version { get; set; }
            [JsonProperty("id")]
            public string Id { get; set; }
            [JsonProperty("license")]
            public string License { get; set; }
            [JsonProperty("license_file")]
            public string LicenseFile { get; set; }
            [JsonProperty("description")]
            public string Description { get; set; }
            [JsonProperty("source")]
            public string Source { get; set; }
            [JsonProperty("dependencies")]
            public Dependency[] Dependencies { get; set; }
            [JsonProperty("targets")]
            public Target[] Targets { get; set; }
            [JsonProperty("features")]
            public Dictionary<string, string[]> Features { get; set; }
            [JsonProperty("manifest_path")]
            public string ManifestPath { get; set; }
            [JsonProperty("metadata")]
            public Dictionary<string, object> Metadata { get; set; }
            [JsonProperty("publish")]
            public string[] Publish { get; set; }
            [JsonProperty("authors")]
            public string[] Authors { get; set; }
            [JsonProperty("categories")]
            public string[] Categories { get; set; }
            [JsonProperty("default_run")]
            public string DefaultRun { get; set; }
            [JsonProperty("rust_version")]
            public string RustVersion { get; set; }
            [JsonProperty("keywords")]
            public string[] Keywords { get; set; }
            [JsonProperty("readme")]
            public string Readme { get; set; }
            [JsonProperty("repository")]
            public string Repository { get; set; }
            [JsonProperty("homepage")]
            public string HomePage { get; set; }
            [JsonProperty("documentation")]
            public string Documentation { get; set; }
            [JsonProperty("edition")]
            public string Edition { get; set; }
            [JsonProperty("links")]
            public string Links { get; set; }

            #region Package InnerData
            [Serializable]
            public class Dependency
            {
                [JsonProperty("name")]
                public string Name { get; set; }
                [JsonProperty("source")]
                public string Source { get; set; }
                [JsonProperty("req")]
                public string Req { get; set; }
                /**
                 * "dev", "build", or null
                 */
                [JsonProperty("kind")]
                public string Kind { get; set; }
                /**
                  * Name for the dependency if renamed, null if not
                 */
                [JsonProperty("rename")]
                public string Rename { get; set; }
                [JsonProperty("optional")]
                public bool Optional { get; set; }
                [JsonProperty("uses_default_features")]
                public bool UsesDefaultFeatures { get; set; }
                [JsonProperty("features")]
                public string[] Features { get; set; }
                [JsonProperty("target")]
                public string Target { get; set; }
                [JsonProperty("path")]
                public string Path { get; set; }
                [JsonProperty("registry")]
                public string Registry { get; set; }
            }

            [Serializable]
            public class Target
            {
                [JsonProperty("kind")]
                public string[] Kind { get; set; }
                [JsonProperty("crate_types")]
                public string[] CrateTypes { get; set; }
                [JsonProperty("name")]
                public string Name { get; set; }
                [JsonProperty("src_path")]
                public string SourcePath { get; set; }
                [JsonProperty("edition")]
                public string Edition { get; set; }
                [JsonProperty("required-features")]
                public string[] RequiredFeatures { get; set; }
                [JsonProperty("doc")]
                public bool Doc { get; set; }
                [JsonProperty("doctest")]
                public bool DocTest { get; set; }
                [JsonProperty("test")]
                public bool Test { get; set; }

            }
            #endregion
        }

        [Serializable]
        public class ResolvedDependencies
        {
            [JsonProperty("nodes")]
            public Node[] Nodes { get; set; }
            [JsonProperty("root")]
            public string Root { get; set; }

            [Serializable]
            public class Node
            {
                [JsonProperty("id")]
                public string Id { get; set; }
                [JsonProperty("dependencies")]
                public string[] Dependencies { get; set; }
                [JsonProperty("deps")]
                public Dep[] Deps { get; set; }
                [JsonProperty("features")]
                public string[] Features { get; set; }

                [Serializable]
                public class Dep
                {
                    [JsonProperty("name")]
                    public string Name { get; set; }
                    [JsonProperty("pkg")]
                    public string Pkg { get; set; }
                    [JsonProperty("dep_kinds")]
                    public DepKind[] DepKinds { get; set; }

                    [Serializable]
                    public class DepKind
                    {
                        [JsonProperty("kind")]
                        public string Kind { get; set; }
                        [JsonProperty("target")]
                        public string Target { get; set; }
                    }
                }
            }
        }
        #endregion
    }
}
