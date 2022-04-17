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
        #region CompilerMessage Fields
        [DataMember(Name = "reason")]
        string Reason { get; set; }

        [DataMember(Name = "package_id")]
        string PackageId { get; set; }

        [DataMember(Name = "manifest_path")]
        string ManifestPath { get; set; }

        [DataMember(Name = "target")]
        Target TargetData { get; set; }

        [DataMember(Name ="message")]
        RustcMessageWithChildren MessageData { get; set; }

        #endregion

        #region InnerData
        [Serializable]
        public class Target
        {
            #region CompilerMessage.Target Fields
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
            List<string> Kinds { get; set; }

            /** Array of crate types.
               - lib and example libraries list the `crate-type` values
                 from the manifest such as "lib", "rlib", "dylib",
                 "proc-macro", etc. (default ["lib"])
               - all other target kinds are ["bin"]
            **/
            [DataMember(Name = "crate_types")]
            List<string> CrateTypes { get; set; }

            [DataMember(Name = "name")]
            string Name { get; set; }

            [DataMember(Name = "src_path")]
            string SourcePath { get; set; }

            [DataMember(Name = "edition")]
            string Edition { get; set; }

            [DataMember(Name = "required-features")]
            List<string> RequiredFeatures { get; set; }

            [DataMember(Name = "doctest")]
            string Doctest { get; set; }
            #endregion
        }
        #endregion
    }

}
