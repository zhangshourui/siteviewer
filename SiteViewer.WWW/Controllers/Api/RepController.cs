using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using SiteViewer.WWW.AppService;
using SiteViewer.WWW.Models.Api;
using SiteViewer.WWW.Service;
using SiteViewer.WWW.Utils;
using Utils;

namespace SiteViewer.WWW.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    [EnableCors("ApiCorsPolicy")]
    public class RepController : ControllerBase
    {

        /// <summary>
        /// Get files and directories in the repository
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        [HttpPost("files")]
        public IActionResult GetFiles(GetRepMsg msg)
        {

            // The parent of `msg.ParentDir`
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


        /// <summary>
        /// Get file info, tell the client how to handle the file (view, download, play, etc.)
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        [HttpPost("View")]
        public IActionResult View(ViewFileMsg msg)
        {
            if (string.IsNullOrEmpty(msg.File))
            {
                return MsgBase.BuildErrorMsg("File path cannot be null or empty.", ResultCode.Error);
            }


            // if the file is a video file, return the video player info
            if (FileRep.IsVideoFile(msg.File))
            {
                return new DefaultMsg<object>
                {
                    Result = ResultCode.Success,
                    Data = new FileViewInfo("video", "play", msg.File, FileRep.ResolveRepFileToPubUrl(msg.File))
                };
            }

            // If the file is text, image, etc., directly send the file content to be displayed in the browser.
            if (FileRep.IsBrowerSupportFile(msg.File))
            {
                return new DefaultMsg<object>
                {
                    Result = ResultCode.Success,
                    Data = new FileViewInfo("", "view", msg.File, FileRep.ResolveRepFileToPubUrl(msg.File))
                };

            }
            // for other file types, return the download link
            else
            {
                return new DefaultMsg<object>
                {
                    Result = ResultCode.Success,
                    Data = new FileViewInfo("", "download", msg.File, FileRep.ResolveRepFileToPubUrl(msg.File))
                };
            }
        }


        /// <summary>
        /// Play a video file, if not supported, convert it to m3u8 format
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        [HttpPost("Play")]
        public IActionResult Play(PlayFileMsg msg)
        {
            var requestFile = msg.File;
            if (string.IsNullOrEmpty(requestFile))
            {
                return MsgBase.BuildErrorMsg("File path cannot be null or empty.", ResultCode.NotFound);
            }
            //文件仓库路径
            var repFile = FileRep.ResolveRepPath(requestFile);
            if (!System.IO.File.Exists(repFile))
            {
                return MsgBase.BuildErrorMsg("File path cannot be null or empty. File: " + requestFile, ResultCode.NotFound);
            }
            // 缓存路径
            var cacheFile = FileRep.ResolveCachePath(requestFile, true);
            var ext = Path.GetExtension(repFile).ToLowerInvariant();
            var videoUrl = requestFile;
            var fileName = Path.GetFileName(repFile);

            if (ext.Equals(".ts", StringComparison.OrdinalIgnoreCase) || ext.Equals(".mp4", StringComparison.OrdinalIgnoreCase))
            {
                #region convert the video into m3u8 & ts files, save them to the cache directory

                var outPath = cacheFile + "-stream";// System.IO.Path.Combine(repFile+"-stream", requestFile);
                var m3u8File = outPath + Path.DirectorySeparatorChar + "index.m3u8";

                if (!System.IO.File.Exists(m3u8File))
                {
                    FfmpegHelper.ExportTs(repFile, outPath);
                }

                videoUrl = $"{FileRep.ResolveRepFileToPubUrl(requestFile + "-stream/index.m3u8")}";
                #endregion
            }
            else
            {
                // ViewBag.VideoUrl = Url.Action("DownloadFile", "Home", new { p = realPhyicPath, detectContentType = true });
                videoUrl = $"{FileRep.ResolveRepFileToPubUrl(requestFile)}";
            }

            return new DefaultMsg<object>
            {
                Result = ResultCode.Success,
                Data = new { VideoUrl = videoUrl, FileName = fileName }
            };
        }


        /// <summary>
        /// Renames a file in the repository
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        [HttpPost("Rename")]
        public IActionResult Rename(RenameMsg msg)
        {
            var requestFile = msg.File;
            if (string.IsNullOrEmpty(requestFile))
            {
                return MsgBase.BuildErrorMsg("File path cannot be null or empty.", ResultCode.NotFound);
            }

            var newFilePath = Path.Combine(Path.GetDirectoryName(requestFile) ?? string.Empty, msg.NewFileName ?? "");

            if (!FileRep.RenameFile(requestFile, msg.NewFileName ?? ""))
            {
                return MsgBase.BuildErrorMsg(ThreadHelper.GetLastError() ?? "Failed to rename file. Please check if the new file name is valid.", ResultCode.Error);
            }


            return new DefaultMsg<object>
            {
                Result = ResultCode.Success,
                Data = new { File = FileRep.GetFile(newFilePath) }
            };
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
    }


}
