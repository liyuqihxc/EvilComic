using System;
using System.Collections.Generic;

namespace EvilComic.DataAccess
{
    public partial class XedbPath
    {
        public XedbPath()
        {
            Xeconnection = new HashSet<Xeconnection>();
        }

        public long DbPathId { get; set; }
        public string Path { get; set; }

        public ICollection<Xeconnection> Xeconnection { get; set; }
    }
}
