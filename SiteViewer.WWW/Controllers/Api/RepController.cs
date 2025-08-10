using System.Web;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SiteViewer.WWW.AppService;
using SiteViewer.WWW.Models;
using SiteViewer.WWW.Models.Api;
using SiteViewer.WWW.Service;
using Utils;

namespace SiteViewer.WWW.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    [EnableCors("ApiCorsPolicy")]
    public class RepController : ControllerBase
    {

        //get file list
        [HttpPost("files")]
        [HttpGet("files")]
        public IActionResult GetFiles(GetRepMsg msg)
        {
            try
            {
                var parentDirectoryLink = string.Empty;
                if (!string.IsNullOrEmpty(msg.ParentDir))
                {
                    var parentDirectory = Path.GetDirectoryName(msg.ParentDir);
                    if (parentDirectory != null)
                    {
                        parentDirectoryLink = parentDirectory.Replace(Path.DirectorySeparatorChar, '/');
                    }
                }


                var resp = new GetRepMsgResp()
                {
                    Files = FileRep.GetSubFiles(msg.ParentDir),
                    Directories = FileRep.GetSubDirectories(msg.ParentDir),
                    ParentDirectory = msg.ParentDir?.TrimEnd(Path.DirectorySeparatorChar) ?? "/",
                    ParentDirectoryLink = parentDirectoryLink
                };
                return resp;

            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        //get file list
        [HttpPost("View")]
        public IActionResult View(ViewFileMsg msg)
        {
            if (string.IsNullOrEmpty(msg.File))
            {
                return MsgBase.BuildErrorMsg("File path cannot be null or empty.", ResultCode.Error);
            }
            // action 重定向处理

            // 如果是视频文件，直接跳转到播放页面           
            if (FileRep.IsVideoFile(msg.File))
            {
                return new DefaultMsg<object>
                {
                    Result = ResultCode.Success,
                    Data = new FileViewInfo("video", "play", msg.File, AppSettings.Host + ConvertRepRelavtiveFileToUrl(msg.File))
                };
            }

            // 如果是文本、图片等，则直接推送文件内容，在浏览器中显示
            if (FileRep.IsBrowerSupportFile(msg.File))
            {
                return new DefaultMsg<object>
                {
                    Result = ResultCode.Success,
                    Data = new FileViewInfo("", "view", msg.File, AppSettings.Host + ConvertRepRelavtiveFileToUrl(msg.File))
                };

            }
            // 其他文件类型直接下载
            else
            {
                return new DefaultMsg<object>
                {
                    Result = ResultCode.Success,
                    Data = new FileViewInfo("", "download", msg.File, AppSettings.Host + ConvertRepRelavtiveFileToUrl(msg.File))
                };
            }
        }

        [HttpPost("Play")]
        public IActionResult Play(PlayFileMsg msg)
        {
            var requestFile = msg.File;
            if (string.IsNullOrEmpty(requestFile))
            {
                return MsgBase.BuildErrorMsg("File path cannot be null or empty.", ResultCode.NotFound);
            }
            //文件仓库路径
            var repFile = FileRep.ResolveRepFile(requestFile);
            if (!System.IO.File.Exists(repFile))
            {
                return MsgBase.BuildErrorMsg("File path cannot be null or empty. File: " + requestFile, ResultCode.NotFound);
            }
            // 缓存路径
            var cacheFile = FileRep.ResolveCacheFile(requestFile, true);
            var ext = Path.GetExtension(repFile).ToLowerInvariant();
            var videoUrl = requestFile;
            var fileName = Path.GetFileName(repFile);

            if (ext.Equals(".ts", StringComparison.OrdinalIgnoreCase) || ext.Equals(".mp4", StringComparison.OrdinalIgnoreCase))
            {
                #region 把.ts文件转换为.m3u8

                var outPath = cacheFile + "-stream";// System.IO.Path.Combine(repFile+"-stream", requestFile);
                var m3u8File = outPath + Path.DirectorySeparatorChar + "index.m3u8";

                if (!System.IO.File.Exists(m3u8File))
                {
                    FfmpegHelper.ExportTs(repFile, outPath);
                }

                videoUrl = $"{AppSettings.Host.TrimEnd('/')}{ConvertRepRelavtiveFileToUrl(requestFile)}-stream/index.m3u8";
                #endregion
            }
            else
            {
                // ViewBag.VideoUrl = Url.Action("DownloadFile", "Home", new { p = realPhyicPath, detectContentType = true });
                videoUrl = $"{AppSettings.Host.TrimEnd('/')}{ConvertRepRelavtiveFileToUrl(requestFile)}";
            }

            return new DefaultMsg<object>
            {
                Result = ResultCode.Success,
                Data = new { VideoUrl = videoUrl, FileName = fileName }
            };
        }


        [HttpPost("Rename")]
        public IActionResult Rename(RenameMsg msg)
        {
            var requestFile = msg.File;
            if (string.IsNullOrEmpty(requestFile))
            {
                return MsgBase.BuildErrorMsg("File path cannot be null or empty.", ResultCode.NotFound);
            }
            //文件仓库路径
           //  var repFile = FileRep.ResolveRepFile(requestFile);
            var newFilePath = Path.Combine(Path.GetDirectoryName(requestFile) ?? string.Empty, msg.NewFileName??"");
       
            FileRep.RenameRepFile(requestFile, msg.NewFileName ?? "");


            return new DefaultMsg<object>
            {
                Result = ResultCode.Success,
                Data = new { File = FileRep.GetFile(newFilePath) }
            };
        }

        /// <summary>
        /// 把文件库的相对路径转换为URL格式
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        private string ConvertRepRelavtiveFileToUrl(string? filePath)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                return string.Empty;
            }
            // 确保路径以斜杠开头
            if (!filePath.StartsWith('/'))
            {
                filePath = '/' + filePath;
            }
            // 替换反斜杠为正斜杠
            filePath = filePath.Replace('\\', '/');
            string[] parts = filePath.Split('/');

            for (int i = 0; i < parts.Length; i++)
            {
                parts[i] = Uri.EscapeDataString(parts[i]);
            }

            // 重新组合路径
            return string.Join("/", parts);

        }
    }

    class FileViewInfo
    {
        public string FileType { get; }
        public string Action { get; }
        public string Path { get; }
        public string FileUrl { get; }

        public FileViewInfo(string fileType, string action, string path, string fileUrl)
        {
            FileType = fileType;
            Action = action;
            Path = path;
            FileUrl = fileUrl;
        }

        public override bool Equals(object? obj)
        {
            return obj is FileViewInfo other &&
                   FileType == other.FileType &&
                   Action == other.Action &&
                   Path == other.Path &&
                   FileUrl == other.FileUrl;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(FileType, Action, Path, FileUrl);
        }
    }
}
