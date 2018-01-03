using System;
using System.Collections.Generic;

namespace EvilComic.DataAccess
{
    public partial class Xeconnection
    {
        public Xeconnection()
        {
            Xeinformation = new HashSet<Xeinformation>();
        }

        public long ConnectionId { get; set; }
        public string DbGuid { get; set; }
        public long DbPathId { get; set; }

        public XedbPath DbPath { get; set; }
        public ICollection<Xeinformation> Xeinformation { get; set; }
    }
}
