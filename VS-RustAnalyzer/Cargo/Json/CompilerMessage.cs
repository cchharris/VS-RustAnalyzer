using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace VS_RustAnalyzer.Cargo.Json
{
    /**
     *  See https://doc.rust-lang.org/cargo/reference/external-tools.html#json-messages
     */
    [Serializable]
    public class CompilerMessage
    {
        #region CompilerMessage Properties
        /** compiler-message */
        [DataMember(Name = "reason")]
        public string Reason { get; set; }

        /** The Package ID, a unique identifier for referring to the package.
          ex. "package_id": "my-package 0.1.0 (path+file:///path/to/my-package)"
        */
        [DataMember(Name = "package_id")]
        public string PackageId { get; set; }

        [DataMember(Name = "manifest_path")]
        public string ManifestPath { get; set; }

        [DataMember(Name = "target")]
        public Target TargetData { get; set; }

        [DataMember(Name ="message")]
        public RustcMessageWithChildren MessageData { get; set; }

        #endregion

        #region InnerData
        [Serializable]
        public class Target
        {
            #region CompilerMessage.Target Properties
            /** Array of target kinds.
                 - lib targets list the `crate-type` values from the
                   manifest such as "lib", "rlib", "dylib",
                   "proc-macro", etc. (default ["lib"])
                 - binary is ["bin"]
                 - example is ["example"]
                 - integration test is ["test"]
                 - benchmark is ["bench"]
                 - build script is ["custom-build"]
            **/
            [DataMember(Name = "kind")]
            public List<string> Kinds { get; set; }

            /** Array of crate types.
               - lib and example libraries list the `crate-type` values
                 from the manifest such as "lib", "rlib", "dylib",
                 "proc-macro", etc. (default ["lib"])
               - all other target kinds are ["bin"]
            **/
            [DataMember(Name = "crate_types")]
            public List<string> CrateTypes { get; set; }

            [DataMember(Name = "name")]
            public string Name { get; set; }

            [DataMember(Name = "src_path")]
            public string SourcePath { get; set; }

            [DataMember(Name = "edition")]
            public string Edition { get; set; }

            [DataMember(Name = "required-features")]
            public List<string> RequiredFeatures { get; set; }

            [DataMember(Name = "doctest")]
            public string Doctest { get; set; }
            #endregion
        }
        #endregion
    }

}
