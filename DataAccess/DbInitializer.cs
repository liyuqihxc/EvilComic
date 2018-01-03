using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EvilComic.DataAccess
{
    internal static class DbInitializer
    {
        public static void Initialize(XEDbContext context)
        {
            context.Database.EnsureCreated();

            // Look for any students.
            if (context.Xecategory.Any())
            {
                return;// DB has been seeded
            }

            Xecategory[] categories = new Xecategory[]
            {
                new Xecategory{ CategoryId = 1, Name = "漫画", FrontPage = "http://m.62fan.com/shaonv/" },
                new Xecategory{ CategoryId = 2, Name = "同人", FrontPage = "http://m.62fan.com/manhua/" },
                new Xecategory{ CategoryId = 3, Name = "GIF", FrontPage = "http://m.62fan.com/dongtai/" },
                new Xecategory{ CategoryId = 4, Name = "内涵", FrontPage = "http://m.62fan.com/neihan/" },
            };

            context.Xecategory.AddRange(categories);
            context.SaveChanges();
        }
    }
}
