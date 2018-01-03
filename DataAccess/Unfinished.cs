using System;
using System.Collections.Generic;

namespace EvilComic.DataAccess
{
    public partial class Unfinished
    {
        public long UnfinishedId { get; set; }
        public long InformationId { get; set; }

        public Xeinformation Information { get; set; }
    }
}
