using Microsoft.AspNetCore.StaticFiles;
using SiteViewer.WWW.AppService;
using SiteViewer.WWW.Models;
using SiteViewer.WWW.Utils;

namespace SiteViewer.WWW.Service
{
    public static class FileRep
    {
        /// <summary>
        ///  Resolve a relative path (file or directory) to a physical file path in the repository.
        /// </summary>
        /// <param name="root">Repository root</param>
        /// <param name="filePath">Relative path</param>
        /// <remarks>
        /// Supports formats, e.g. "images/photo.jpg", "/docs/report.pdf", "\docs\report.pdf". Don't use qualified path.
        /// </remarks>
        /// <returns></returns>
        private static string _ResolveRep(string root, string? filePath)
        {
            if (string.IsNullOrEmpty(root))
            {
                throw new Exception("ResourceRoot is not set in AppConfig. Please check your configuration.");
            }

            if (string.IsNullOrEmpty(filePath))
            {
                filePath = "";
            }
            if (filePath.StartsWith(Path.DirectorySeparatorChar) || filePath.StartsWith(Path.AltDirectorySeparatorChar))
            {
                filePath = filePath.Substring(1);
            }
            var fullPath = Path.Combine(root, filePath.Replace('/', Path.DirectorySeparatorChar));
            return fullPath;
        }

        /// <summary>
        ///  Resolve a relative path (file or directory) to a physical file path in the repository.
        /// </summary>
        /// <param name="filePath">Relative path</param>
        /// <remarks>
        /// Supports formats, e.g. "images/photo.jpg", "/docs/report.pdf", "\docs\report.pdf". Don't use qualified path.
        /// </remarks>
        /// <returns></returns>

        public static string ResolveRepPath(string? filePath)
        {
            return _ResolveRep(AppSettings.ResourceRoot, filePath);
        }

        /// <summary>
        ///  Resolve a relative path (file or directory) to a physical file path of cached directory.
        /// </summary>
        /// <param name="filePath"></param>
        /// <remarks>
        /// Supports formats, e.g. "images/photo.jpg", "/docs/report.pdf", "\docs\report.pdf".
        /// Don't use qualified path.
        /// </remarks>
        /// <returns></returns>
        public static string ResolveCachePath(string? filePath, bool createDirIfNotExists = false)
        {
            return _ResolveRep(AppSettings.ResourceCache, filePath);
        }


        /// <summary>
        /// Resolve the content type based on the file extension.
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static string GetContentType(string fileName)
        {
            var ext = Path.GetExtension(fileName);
            if (ExtendMimeType.TryGetValue(ext, out string? value))
            {
                return value;
            }

            var provider = new FileExtensionContentTypeProvider();
            if (!provider.TryGetContentType(fileName, out var contentType))
            {
                contentType = "application/octet-stream";
            }
            return contentType;
        }

        /// <summary>
        /// Check if this file can be opened directly in the browser
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static bool IsBrowerSupportFile(string fileName)
        {
            var ext = Path.GetExtension(fileName);
            if (string.IsNullOrEmpty(ext))
            {
                return false;
            }
            // 目前浏览器支持的文件类型
            var supportedExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                ".html", ".htm", ".css", ".js", ".json", ".xml",
                ".jpg", ".jpeg", ".png", ".gif", ".svg",
           //     ".mp4", ".webm", ".ogg",
                ".txt",".md", ".log",
                ".pdf"
            };
            return supportedExtensions.Contains(ext);
        }


        /// <summary>
        /// Check if this file is a video file that can be played in the browser.
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static bool IsVideoFile(string fileName)
        {
            var ext = Path.GetExtension(fileName);
            if (string.IsNullOrEmpty(ext))
            {
                return false;
            }
            // 目前浏览器支持的视频文件类型
            var videoExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                ".mp4", ".webm", ".ogg", ".ts", ".m3u8"
            };
            return videoExtensions.Contains(ext);
        }

        /// <summary>
        /// Get all subdirectories in the specified directory, and order by last modified time descending.
        /// </summary>
        /// <param name="directory"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static List<DirectoryEntry> GetSubDirectories(string? directory)
        {
            if (string.IsNullOrEmpty(directory))
            {
                directory = "";
            }
            if (Path.IsPathFullyQualified(directory))
            {
                throw new Exception(nameof(directory) + " should be a relative path, not a fully qualified path.");
            }
            var full = ResolveRepPath(directory);
            var directories = new List<DirectoryEntry>();
            if (Directory.Exists(full))
            {
                var dirEntries = Directory.GetDirectories(full);
                foreach (var dir in dirEntries)
                {
                    var name = Path.GetFileName(dir);
                    if (name == "_cache" || name.StartsWith("."))
                    {
                        continue;
                    }

                    directories.Add(new DirectoryEntry(Path.GetFileName(dir), directory)
                    {
                        LastTime = Directory.GetLastWriteTime(dir)
                    });
                }
            }
            return directories.OrderByDescending(m => m.LastTime).ToList();
        }

        /// <summary>
        /// Get all files in the specified directory, and order by last modified time descending.
        /// </summary>
        /// <param name="directory"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static List<FileEntry> GetSubFiles(string? directory)
        {
            if (string.IsNullOrEmpty(directory))
            {
                directory = "";
            }
            if (Path.IsPathFullyQualified(directory))
            {
                throw new Exception(nameof(directory) + " should be a relative path, not a fully qualified path.");
            }
            var full = ResolveRepPath(directory);

            var files = new List<FileEntry>();
            //  var fileMd5s = new List<string>();
            if (Directory.Exists(full))
            {
                var fileEntries = Directory.GetFiles(full, "*.*");
                foreach (var file in fileEntries)
                {
                    var fi = new FileInfo(file);
                    files.Add(new FileEntry(Path.GetFileName(file), directory)
                    {
                        LastTime = fi.LastWriteTime,
                        FileSize = fi.Length
                    });
                }
            }
            return files.OrderByDescending(m => m.LastTime).ToList();
        }

        /// <summary>
        /// Get a file entry for the specified file.
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        /// <exception cref="FileNotFoundException"></exception>

        public static FileEntry GetFile(string file)
        {
            var repFile = ResolveRepPath(file);

            var dir = Path.GetDirectoryName(file);
            if (!File.Exists(repFile))
            {
                throw new FileNotFoundException("File not found: " + file);
            }
            var fi = new FileInfo(repFile);
            return new FileEntry(Path.GetFileName(file), dir ?? "")
            {
                LastTime = fi.LastWriteTime,
                FileSize = fi.Length
            };
        }

        /// <summary>
        /// Renames a file in the repository.
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="newFileName"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>

        public static bool RenameFile(string? filePath, string newFileName)
        {
            // some checks
            if (string.IsNullOrEmpty(AppSettings.ResourceRoot))
            {
                throw new Exception("ResourceRoot is not set in AppConfig. Please check your configuration.");
            }

            // Is newFileName a valid file name?
            if (string.IsNullOrEmpty(newFileName) || newFileName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
            {
                ThreadHelper.SetLastError("New file name is invalid.");
                return false;
            }
            // end checks

            // source file physical path
            var srcFilePath = ResolveRepPath(filePath);

            // File extension is not allowed to change
            var ext = Path.GetExtension(srcFilePath);
            if (!Path.GetExtension(newFileName).Equals(ext, StringComparison.OrdinalIgnoreCase))
            {
                ThreadHelper.SetLastError("File extension cannot be changed.");
                return false;
            }

            // Replace file name without extension
            var newFilePath = Path.Combine(Path.GetDirectoryName(srcFilePath) ?? string.Empty, newFileName);
            if (File.Exists(newFilePath))
            {
                ThreadHelper.SetLastError("File extension cannot be changed.");
                return false;
            }

            // Here change the file name(through `move` like linux)
            File.Move(srcFilePath, newFilePath);

            // remove cached file if exists
            var srcCachedFilePath = ResolveCachePath(filePath, false);
            if (File.Exists(srcCachedFilePath))
            {
                File.Delete(srcCachedFilePath);
            }

            // video cache directory.
            var cacheDir = srcCachedFilePath + "-stream";
            if (Directory.Exists(cacheDir))
            {
                Directory.Delete(cacheDir, true);
            }

            return true;
        }

        /// <summary>
        /// Converts the relative path of the file repository to the public URL.
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static string ResolveRepFileToPubUrl(string? filePath)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                return string.Empty;
            }
            filePath = filePath.TrimStart('/').TrimStart('\\').Replace('\\', '/'); // 去除开头的斜杠或反斜杠

            string[] parts = filePath.Split('/');

            for (int i = 0; i < parts.Length; i++)
            {
                parts[i] = Uri.EscapeDataString(parts[i]);
            }

            // 重新组合路径
            return AppSettings.Host.TrimEnd('/') + '/' + string.Join("/", parts);

        }

        /// <summary>
        /// Extended MIME types for specific file extensions that are not covered by the default MIME type provider.
        /// Also it overrides some default MIME types for better compatibility with browsers.
        /// </summary>
        private static readonly Dictionary<string, string> ExtendMimeType = new(StringComparer.OrdinalIgnoreCase)
        {
            { ".ts", "video/mp2t"},
            {".js", "text/plain"},
            {".html", "text/plain"},
            {".log", "text/plain"},
            {".md", "text/plain"},
        };
    }
}