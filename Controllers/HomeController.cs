using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EvilComic.Common;
using EvilComic.DataAccess;
using EvilComic.Models;
using EvilComic.WebComicData;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;

namespace EvilComic.Controllers
{
    public class HomeController : Controller
    {
        public HomeController(XEDbContext dbContext) : base()
        {
            _dbContext = dbContext;
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            // Optional: Work only for GET request
            //if (filterContext.HttpContext.Request.Method != "GET")
                //return;

            // Optional: Do not work with AjaxRequests
            //if (filterContext.HttpContext.Request.IsAjaxRequest())
                //return;

            (filterContext.Controller as HomeController).ViewData["TaskRunning"] = TaskDownloadComic != null;
        }

        private XEDbContext _dbContext { get; }

        private static Task TaskDownloadComic;
        private static CancellationTokenSource CancellationToken;
        private static string Message;
        private static int? ComicCount;
        private static int? ComicFinished;
        private static float? PrecentComplete;
        private static bool? IsProcessingUnfinished;

        public IActionResult Index(int? category, int? PageNum)
        {
            int Num = PageNum ?? 1;
            ViewData["PageNum"] = Num;

            int Cat = 0;
            if (category != null)
            {
                _dbContext.Xecategory.Any(m => m.CategoryId == category);
                Cat = category.Value;
            }
            else
            {
                Cat = (int)_dbContext.Xecategory.First().CategoryId;
            }
            ViewData["Cat"] = Cat;

            int count;
            ViewBag.PicList = _dbContext.Xeinformation.Where(m => m.CategoryId == Cat)
                .Pagegation(m => m.Order, Num, 20, out count).ToArray();

            ViewData["PageCount"] = (_dbContext.Xeinformation.Count() / 20) + (_dbContext.Xeinformation.Count() % 20 != 0 ? 1 : 0);

            return View();
        }

        public Task<IActionResult> CoverImage(int ComicId)
        {
            return Task<IActionResult>.Factory.StartNew(() =>
            {
                var info = _dbContext.Xeinformation.FirstOrDefault(m => m.InformationId == ComicId);
                if (info == null)
                    return new NotFoundResult();
                return new FileContentResult(info.CoverImage, "image/jpeg");
            });
        }

        public IActionResult ViewComic(int ComicId, int? PageNum)
        {
            var info = _dbContext.Xeinformation.FirstOrDefault(m => m.InformationId == ComicId);
            if (info == null)
                return new NotFoundResult();
            if (PageNum != null && PageNum > info.ImageCount)
                return new NotFoundResult();
            ViewData["PageNum"] = PageNum ?? 1;
            return View(info);
        }

        public Task<IActionResult> ComicImage(int ComicId, int Order)
        {
            return Task<IActionResult>.Factory.StartNew(() =>
            {
                var info = _dbContext.Xeinformation.FirstOrDefault(m => m.InformationId == ComicId);
                if (info == null)
                    return new NotFoundResult();

                using (var dbc = info.GetXEImageContext(_dbContext))
                {
                    var img = dbc.Xeimages.FirstOrDefault(m => m.Order == Order && m.InformationId == ComicId);
                    if (img == null)
                        return new NotFoundResult();
                    return new FileContentResult(img.Xedata, "image/jpeg");
                }
            });
        }

        public Task<IActionResult> DownloadComics()
        {
            return Task<IActionResult>.Factory.StartNew(() =>
            {
                if (TaskDownloadComic != null)
                {
                    ViewData["PrecentComplete"] = PrecentComplete ?? 0;
                    ViewData["IsProcessingUnfinished"] = IsProcessingUnfinished;
                    ViewData["ComicCount"] = ComicCount;
                    ViewData["ComicFinished"] = ComicFinished;
                    ViewData["Message"] = Message;
                }
                return View(_dbContext.Xecategory.ToList());
            });
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        public IActionResult StartDownload(int category)
        {
            if (TaskDownloadComic == null)
            {
                CancellationToken = new CancellationTokenSource();
                TaskDownloadComic = DownloadComicAsync(_dbContext.Xecategory.First(m => m.CategoryId == category), CancellationToken.Token);
                return Json(new ResultModel()
                {
                    Succeed = true,
                    Message = "成功启动下载任务。"
                });
            }
            else
            {
                return Json(new ResultModel()
                {
                    Succeed = false,
                    Message = "下载任务正在运行。"
                });
            }
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        public IActionResult StopDownload()
        {
            if (TaskDownloadComic != null)
            {
                try
                {
                    CancellationToken.Cancel();
                    TaskDownloadComic.Wait();
                    CancellationToken = null;
                }
                catch
                {

                }
                return Json(new ResultModel()
                {
                    Succeed = true,
                    Message = "下载任务已退出。"
                });
            }
            return Json(new ResultModel()
            {
                Succeed = false,
                Message = "下载任务不存在。"
            });
        }

        public IActionResult Error()
        {
            return View();
        }

        private static Task DownloadComicAsync(Xecategory category, CancellationToken cancellationToken)
        {
            return Task.Factory.StartNew(() =>
            {
                string FrontPage = category.FrontPage;
                string CatName = category.Name;

                ProcessUnfinished(cancellationToken);
                if (cancellationToken.IsCancellationRequested)
                {
                    TaskDownloadComic = null;
                    IsProcessingUnfinished = null;
                    ComicCount = null;
                    ComicFinished = null;
                    PrecentComplete = null;
                    Message = null;
                    return;
                }

                ComicCovers ccs = null;
                try
                {
                    ccs = new ComicCovers(new Uri(FrontPage));
                }
                catch
                {
                    TaskDownloadComic = null;
                    IsProcessingUnfinished = null;
                    ComicCount = null;
                    ComicFinished = null;
                    PrecentComplete = null;
                    Message = null;
                    return;
                }

                //Invoke(new Action(() =>
                //{
                //    frmProgress.ProgressBar.Minimum = 0;
                //    //设置一个最大值
                //    frmProgress.ProgressBar.Maximum = ccs.nComics;
                //    //设置步长，即每次增加的数
                //    frmProgress.ProgressBar.Step = 1;
                //}));

                IsProcessingUnfinished = false;
                ComicCount = ccs.nComics;
                int CountComic = 1;
                foreach (var cover in ccs.Covers)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        TaskDownloadComic = null;
                        IsProcessingUnfinished = null;
                        ComicCount = null;
                        ComicFinished = null;
                        PrecentComplete = null;
                        Message = null;
                        return;
                    }

                    //Invoke(new Action(() =>
                    //{
                    //    frmProgress.labCount.Text = string.Format("共{0}个项目，正在下载第{1}个。", ccs.nComics, CountComic);
                    //}));

                    Xeinformation info = null;
                    // 先按标题判断数据库当中是否存在封面记录

                    // 封面记录不存在，开始下载
                    using (var xedbc = XEDbContext.CreateContext())
                    {
                        info = xedbc.Xeinformation.FirstOrDefault(m => m.Title == cover.Title);
                        if (info != null)
                        {
                            info.Order = cover.Order;
                            xedbc.SaveChanges();
                        }
                    }

                    if (info == null)
                        SaveComicImages(cover, category, cancellationToken);

                    if (cancellationToken.IsCancellationRequested)
                    {
                        TaskDownloadComic = null;
                        IsProcessingUnfinished = null;
                        ComicCount = null;
                        ComicFinished = null;
                        PrecentComplete = null;
                        Message = null;
                        return;
                    }

                    ComicFinished = CountComic;
                    PrecentComplete = ((float)ComicFinished / (float)ComicCount) * 100f;
                    CountComic++;
                    //Invoke(new Action(() =>
                    //{
                    //    frmProgress.ProgressBar.PerformStep();
                    //}));
                }
                TaskDownloadComic = null;
                IsProcessingUnfinished = null;
                ComicCount = null;
                ComicFinished = null;
                PrecentComplete = null;
                Message = null;
            });
        }

        private static void ProcessUnfinished(CancellationToken token)
        {
            var random = new Random();

            Xeinformation info = null;
            using (var db = XEDbContext.CreateContext())
            {
                var unfinished = db.Unfinished.Include(m => m.Information).FirstOrDefault();
                if (unfinished == null)
                    return;
                info = unfinished.Information;
            }

            IsProcessingUnfinished = true;
            ComicCount = (int)info.ImageCount;

            for (int i = 0; i < info.ImageCount; i++)
            {
                if (token.IsCancellationRequested)
                    return;

                bool needDelay = false;
                using (var xedbc = XEDbContext.CreateContext())
                {
                    using (var imgdbc = info.GetXEImageContext(xedbc))
                    {
                        // 判断图片是否存在，不存在则开始下载
                        if (imgdbc.Xeimages.Count(m => m.InformationId == info.InformationId && m.Order == i) == 0)
                        {
                            try
                            {
                                imgdbc.Xeimages.Add(new XEImages
                                {
                                    Order = i,
                                    Xedata = Utility.LoadResource(new Uri(string.Format(info.ImageUrlTemplate, i)), 3),
                                    InformationId = info.InformationId
                                });
                                needDelay = true;
                                imgdbc.SaveChanges();
                            }
                            catch
                            {

                            }
                        }

                        ComicFinished = i;
                        PrecentComplete = ((float)ComicFinished / (float)ComicCount) * 100f;
                        Message = $"{info.Title} - ({info.ImageCount}/{ComicFinished})";
                    }
                }

                //Invoke(new Action(() =>
                //{
                //    frmProgress.labTitle.Text = string.Format("({0}/{1}){2}", i + 1, info.nImageCount, info.Title);
                //}));
                if (needDelay) Thread.Sleep(random.Next(5000, 7000));
            }

            using (var db = XEDbContext.CreateContext())
                db.Database.ExecuteSqlCommand("delete from Unfinished;update sqlite_sequence set seq = 0 where name = 'Unfinished';");
        }

        private static void SaveComicImages(ComicCoverModel model, Xecategory category, CancellationToken token)
        {
            Xeinformation info = null;
            Comic c = null;
            try
            {
                c = new Comic(model);
                info = new Xeinformation
                {
                    Title = model.Title,
                    CoverImage = model.CoverImage,
                    ImageCount = c.nImages,
                    ImageUrlTemplate = c.ImageUrlTemplate,
                    OrginUrl = model.OrginUrl,
                    Order = model.Order,
                    CategoryId = category.CategoryId
                };
                using (var xedbc = XEDbContext.CreateContext())
                {
                    //xedbc.Xecategory.Attach(category);

                    Xeconnection dbConn = null;
                    foreach (var path in xedbc.XedbPath)
                    {
                        var di = new DriveInfo(Path.GetPathRoot(path.Path));
                        if (di.AvailableFreeSpace < XEImageContext.DiskMinFreeSpace)// 跳过剩余空间小于规定的磁盘查找可用的数据文件
                            continue;
                        
                        foreach (var conn in xedbc.Xeconnection.Where(m => m.DbPathId == path.DbPathId))
                        {
                            var fi = new FileInfo(Path.Combine(path.Path, conn.DbGuid + ".sqlite"));
                            // 跳过大小超过规定的数据文件
                            if (fi.Length >= XEImageContext.DataFileMaxSize)
                                continue;
                            // 跳过剩余空间不足的数据文件
                            // 新增记录数 * 记录平均大小 + 当前数据文件大小 必须 小于等于 最大数据文件大小
                            if (c.nImages * XEImageContext.RecordAvgSize + fi.Length > XEImageContext.DataFileMaxSize)
                                continue;
                            dbConn = conn;
                            break;
                        }
                        if (dbConn == null)
                        {
                            // 如果不存在合适的数据文件，创建一个
                            var guid = Guid.NewGuid();
                            using (var db = System.IO.File.Create(Path.Combine(path.Path, guid.ToString() + ".sqlite")))
                                db.Write(Properties.Resources.XEImages, 0, Properties.Resources.XEImages.Length);
                            dbConn = new Xeconnection
                            {
                                DbGuid = guid.ToString(),
                                DbPathId = path.DbPathId
                            };
                            xedbc.Xeconnection.Add(dbConn);
                            xedbc.SaveChanges();
                        }
                        break;
                    }
                    if (dbConn == null)
                        return;// 数据库当中可用路径所在的磁盘都没有空间了

                    info.ConnectionId = dbConn.ConnectionId;
                    xedbc.Xeinformation.Add(info);
                    xedbc.Unfinished.Add(new Unfinished { Information = info });
                    xedbc.SaveChanges();
                }
            }
            catch
            {
                return;
            }

            int ImageCount = 0;
            var random = new Random();
            foreach (var img in c.Images)
            {
                if (token.IsCancellationRequested)
                    return;

                if (img != null)
                {
                    using (var xedbc = XEDbContext.CreateContext())
                    {
                        using (var imgdbc = info.GetXEImageContext(xedbc))
                        {
                            imgdbc.Xeimages.Add(new XEImages
                            {
                                Xedata = img.ImageData,
                                Order = img.Order,
                                InformationId = info.InformationId
                            });
                            imgdbc.SaveChanges();
                        }
                    }
                }

                Message = $"{info.Title} - ({info.ImageCount}/{ImageCount})";
                ImageCount++;
                //Invoke(new Action(() =>
                //{
                //    frmProgress.labTitle.Text = string.Format("({0}/{1}){2}", ImageCount, c.nImages, c.Title);
                //}));
                Thread.Sleep(random.Next(5000, 7000));
            }

            using (var xedbc = XEDbContext.CreateContext())
                xedbc.Database.ExecuteSqlCommand("delete from Unfinished;update sqlite_sequence set seq = 0 where name = 'Unfinished';");
        }

    }
}
