using System;
using System.Collections.Generic;

namespace EvilComic.DataAccess
{
    public partial class XEImages
    {
        public long ImageId { get; set; }
        public long Order { get; set; }
        public byte[] Xedata { get; set; }
        public long InformationId { get; set; }
    }
}
