using EvilComic.Common;
using HtmlAgilityPack;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace EvilComic.WebComicData
{
    class ComicCovers
    {
        private ComicCovers()
        {
            throw new NotImplementedException();
        }

        public ComicCovers(Uri FrontPage)
        {
            this.FrontPage = FrontPage ?? throw new ArgumentNullException();

            FrontPageDocumentString = Utility.DefaultEncoding.GetString(Utility.LoadResource(FrontPage, 3));
            FrontPageDom = new HtmlDocument();
            FrontPageDom.LoadHtml(FrontPageDocumentString);

            var span = FrontPageDom.DocumentNode.SelectSingleNode("//span[@class=\"pageinfo\"]");
            Regex reg = new Regex(@"\d+(?=页)");
            nPages = int.Parse(reg.Match(span.InnerText).Value);
            reg = new Regex(@"\d+(?=条)");
            nComics = int.Parse(reg.Match(span.InnerText).Value);
            //nPages = 1;

            if (nPages > 1)
            {
                var li = FrontPageDom.DocumentNode.SelectSingleNode("//div[@class=\"showpage\"]/li[3]");
                string href = li.FirstChild.GetAttributeValue("href", "");
                if (string.IsNullOrEmpty(href))
                    throw new InvalidOperationException("DOM结构已改变。");
                var array = href.Split('.');
                array[0] = array[0].Substring(0, array[0].LastIndexOf('_') + 1);
                PageUrlTemplate = new Uri(FrontPage, array[0] + "{0}." + array[1]).ToString();
            }
        }

        private Uri FrontPage;
        private string FrontPageDocumentString;
        private HtmlDocument FrontPageDom;
        private int nPages;
        public int nComics { get; }
        private string PageUrlTemplate;

        public IEnumerable<ComicCoverModel> Covers
        {
            get
            {
                int PageNum = 1;
                HtmlDocument doc = FrontPageDom;
                int ComicOrder = 1;
                do
                {
                    var lis = doc.DocumentNode.SelectNodes("//ul[@class=\"pic pic1\"]/li");
                    foreach (var li in lis)
                    {
                        var AS = li.SelectNodes("a");

                        string OrginUrl = AS[0].GetAttributeValue("href", "");

                        byte[] CoverImage = null;
                        try
                        {
                            CoverImage = Utility.LoadResource(new Uri(AS[0].FirstChild.GetAttributeValue("src", "")), 3);
                        }
                        catch
                        {
                            CoverImage = Properties.Resources.HPic;
                        }

                        if (string.IsNullOrEmpty(OrginUrl) || CoverImage == null)
                            continue;

                        yield return new ComicCoverModel
                        {
                            OrginUrl = new Uri(FrontPage, OrginUrl).ToString(),
                            Title = li.InnerText,
                            CoverImage = CoverImage,
                            Order = ComicOrder++
                        };
                        //yield break;
                    }

                    PageNum++;
                    if (PageNum > nPages)
                        yield break;
                    doc = new HtmlDocument();
                    byte[] buff = null;
                    try
                    {
                        var t = Utility.LoadResource(new Uri(string.Format(PageUrlTemplate, PageNum)));
                        t.Wait();
                        buff = t.Result;
                        doc.LoadHtml(Utility.DefaultEncoding.GetString(buff));
                    }
                    catch
                    {
                        yield break;
                    }
                } while (true);
            }
        }
    }

    class ComicCoverModel
    {
        public string OrginUrl { get; set; }

        public string Title { get; set; }

        public int Order { get; set; }

        public byte[] CoverImage { get; set; }
    }
}
