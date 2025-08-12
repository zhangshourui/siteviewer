using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using SiteViewer.WWW.AppService;
using SiteViewer.WWW.Models;
using SiteViewer.WWW.Service;
using Utils;

namespace SiteViewer.WWW.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index([ModelBinder(Name = "p")] string? parentDir)
        {
            ViewBag.ResourceRoot = AppSettings.ResourceRoot;

            ViewBag.ParentDirectory = parentDir;

            if (string.IsNullOrEmpty(AppSettings.ResourceRoot))
            {
                throw new Exception("ResourceRoot is not set in AppConfig. Please check your configuration.");
            }

            ViewBag.SubDirectories = FileRep.GetSubDirectories(parentDir); ;
            ViewBag.Files = FileRep.GetSubFiles(parentDir);

            // 上级目录链接
            if (!string.IsNullOrEmpty(parentDir))
            {
                var parentDirectory = Path.GetDirectoryName(parentDir);
                if (parentDirectory != null)
                {
                    ViewBag.ParentDirectoryLink = parentDirectory;
                }
            }
            else
            {
                ViewBag.ParentDirectoryLink = string.Empty;
            }

            return View();
        }

        public IActionResult ViewFile([ModelBinder(Name = "p")] string filePath)
        {
            var ext = Path.GetExtension(filePath);
            // action 重定向处理

            // 如果是视频文件，直接跳转到播放页面           
            if (FileRep.IsVideoFile(filePath))
            {
                return RedirectToAction("Play", "Home", new { p = filePath });
            }

            // 如果是文本、图片等，则直接推送文件内容，在浏览器中显示
            if (FileRep.IsBrowerSupportFile(filePath))
            {
                return PushDefaultFile( filePath); // 直接推送文件内容

            }


            // 其他文件类型直接下载
            return DownloadFile(filePath);
        }

        private IActionResult PushDefaultFile(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                return NotFound();
            }
            var fullPath = FileRep.ResolveRepPath(filePath);
            if (!System.IO.File.Exists(fullPath))
            {
                return NotFound();
            }
            var contentType = FileRep.GetContentType(filePath);

            var stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read);

            return File(fileStream: stream,
                        contentType: contentType,
                        enableRangeProcessing: true);
        }

        public IActionResult DownloadFile([ModelBinder(Name = "p")] string filePath, bool detectContentType = false)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                return NotFound();
            }
            var fullPath = FileRep.ResolveRepPath(filePath);
            if (!System.IO.File.Exists(fullPath))
            {
                return NotFound();
            }

            var downloadFileName = Path.GetFileName(fullPath);

            var contentType = "application/octet-stream"; // 默认的下载类型

            var stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read);

            return File(fileStream: stream,
                        contentType: contentType,
                        fileDownloadName: downloadFileName,
                        enableRangeProcessing: true);
        }

        public IActionResult Play([ModelBinder(Name = "p")] string requestFile)
        {
            if (string.IsNullOrEmpty(requestFile))
            {
                return NotFound();
            }
            //文件仓库路径
            var repFile = FileRep.ResolveRepPath(requestFile);
            if (!System.IO.File.Exists(repFile))
            {
                return NotFound();
            }
            // 缓存路径
            var cacheFile = FileRep.ResolveCachePath(requestFile, true);
            var ext = Path.GetExtension(repFile).ToLowerInvariant();

            if (ext.Equals(".ts", StringComparison.OrdinalIgnoreCase) || ext.Equals(".mp4", StringComparison.OrdinalIgnoreCase))
            {
#if false // 把ts转成mp4
                // 如果是.ts文件，转换为.mp4
                //var mp4FilePath = Path.ChangeExtension(repFile, ".mp4");
                //if (!System.IO.File.Exists(mp4FilePath))
                //{
                //    try
                //    {
                //        FfmpegHelper.ConvertTs2Mp4(repFile, mp4FilePath);
                //    }
                //    catch (Exception ex)
                //    {
                //        _logger.LogError(ex, "转换TS文件失败");
                //        return Content("转换TS文件失败，请检查FFmpeg配置。");
                //    }
                //}
                //realPhyicPath = mp4FilePath;
#endif
                #region 把.ts文件转换为.m3u8

                var outPath = cacheFile + "-stream";// System.IO.Path.Combine(repFile+"-stream", requestFile);
                var m3u8File = outPath + Path.DirectorySeparatorChar + "index.m3u8";

                if (!System.IO.File.Exists(m3u8File))
                {
                    FfmpegHelper.ExportTs(repFile, outPath);
                }


                ViewBag.VideoUrl = $"/{requestFile}-stream/index.m3u8";
                #endregion
            }
            else
            {
                // ViewBag.VideoUrl = Url.Action("DownloadFile", "Home", new { p = realPhyicPath, detectContentType = true });
                ViewBag.VideoUrl = $"/{requestFile}";
            }

            ViewBag.File = Path.GetFileName(repFile);

            return View();
        }

        /// <summary>
        /// 获取目录下的所有子目录
        /// </summary>
        /// <returns></returns>
        private List<string> GetSubDirectories(string path)
        {
            List<string> subDirectories = new List<string>();
            if (Directory.Exists(path))
            {
                var directories = Directory.GetDirectories(path);
                foreach (var dir in directories)
                {
                    subDirectories.Add(Path.GetFileName(dir));
                }
            }
            return subDirectories;
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}