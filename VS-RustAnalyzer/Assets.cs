using Microsoft.VisualStudio.Imaging.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VS_RustAnalyzer
{
    internal class Assets
    {
        public static Guid Guid = new Guid("{F43B625C-EFA4-4346-9E7C-D792611D3465}");
        public static int RSFileNodeId = 3;
        public static int RSConsoleId = 4;
        public static int RSClassLibraryId = 5;

        public static ImageMoniker RSConsoleMoniker => new ImageMoniker { Guid = Guid, Id = RSConsoleId };
        public static ImageMoniker RSFileNodeMoniker => new ImageMoniker { Guid = Guid, Id = RSFileNodeId };
        public static ImageMoniker RSClassLibrarymoniker => new ImageMoniker { Guid = Guid, Id = RSClassLibraryId };
    }
}
