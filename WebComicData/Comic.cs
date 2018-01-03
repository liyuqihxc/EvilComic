using EvilComic.Common;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace EvilComic.WebComicData
{
    class Comic
    {
        public Comic(ComicCoverModel model)
        {
            Title = model.Title;
            OrginUrl = model.OrginUrl;

            FrontPageDocumentString = Utility.DefaultEncoding.GetString(Utility.LoadResource(new Uri(model.OrginUrl), 3));
            FrontPageDom = new HtmlDocument();
            FrontPageDom.LoadHtml(FrontPageDocumentString);

            var fanye1 = FrontPageDom.DocumentNode.SelectSingleNode("//div[@class=\"fanye1\"]");
            Regex reg = new Regex(@"\d+(?=页)");
            nImages = int.Parse(reg.Match(fanye1.InnerText).Value);
            //nImages = 10;

            var img = FrontPageDom.DocumentNode.SelectSingleNode("//img");
            string img_src = img.GetAttributeValue("src", "");
            string ParentParth = img_src.Substring(0, img_src.LastIndexOf('/') + 1);
            string ext = Path.GetExtension(new Uri(img_src).LocalPath);
            string FormatPattern = null;
            /*if (nImages >= 1 && nImages < 10)
                FormatPattern = "{0}";
            else if (nImages >= 10 && nImages < 100)
                FormatPattern = "{0:00}";
            else if (nImages >= 100 && nImages < 1000)*/
                FormatPattern = "{0:000}";
            /*else if (nImages >= 1000 && nImages < 10000)
                FormatPattern = "{0:0000}";*/
            ImageUrlTemplate = ParentParth + FormatPattern + ext;
        }

        public int nImages { get; }
        public string Title { get; }
        public string OrginUrl { get; }
        public string ImageUrlTemplate { get; }
        private string FrontPageDocumentString;
        private HtmlDocument FrontPageDom;

        public IEnumerable<ComicImageModel> Images
        {
            get
            {
                for (int i = 0; i < nImages; i++)
                {
                    byte[] img = null;
                    try
                    {
                        img = Utility.LoadResource(new Uri(string.Format(ImageUrlTemplate, i)), 3);
                    }
                    catch
                    {
                        
                    }
                    if (img == null)
                    {
                        yield return null;
                    }
                    else
                    {
                        var m = new ComicImageModel
                        {
                            Order = i,
                            ImageData = img
                        };
                        yield return m;
                    }
                }
            }
        }
    }

    class ComicImageModel
    {
        public int Order { get; set; }

        public byte[] ImageData { get; set; }
    }
}
