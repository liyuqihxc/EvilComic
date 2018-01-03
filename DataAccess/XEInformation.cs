using System;
using System.Collections.Generic;

namespace EvilComic.DataAccess
{
    public partial class Xeinformation
    {
        public Xeinformation()
        {
            Unfinished = new HashSet<Unfinished>();
        }

        public long InformationId { get; set; }
        public string Title { get; set; }
        public byte[] CoverImage { get; set; }
        public long ImageCount { get; set; }
        public string ImageUrlTemplate { get; set; }
        public string OrginUrl { get; set; }
        public long CategoryId { get; set; }
        public long Order { get; set; }
        public long ConnectionId { get; set; }

        public Xecategory Category { get; set; }
        public Xeconnection Connection { get; set; }
        public ICollection<Unfinished> Unfinished { get; set; }
    }
}
