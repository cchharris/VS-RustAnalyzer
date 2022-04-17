using System;
using System.Collections.Generic;
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
    }
}
