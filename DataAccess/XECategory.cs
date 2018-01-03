using System;
using System.Collections.Generic;

namespace EvilComic.DataAccess
{
    public partial class Xecategory
    {
        public Xecategory()
        {
            Xeinformation = new HashSet<Xeinformation>();
        }

        public long CategoryId { get; set; }
        public string Name { get; set; }
        public string FrontPage { get; set; }

        public ICollection<Xeinformation> Xeinformation { get; set; }
    }
}
