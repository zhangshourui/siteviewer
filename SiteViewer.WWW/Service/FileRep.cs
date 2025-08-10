using Microsoft.AspNetCore.StaticFiles;
using SiteViewer.WWW.AppService;
using SiteViewer.WWW.Models;

namespace SiteViewer.WWW.Service
{
    public static class FileRep
    {
        public static string ResolveRepFile(string? filePath)
        {
            if (string.IsNullOrEmpty(AppSettings.ResourceRoot))
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
            var fullPath = Path.Combine(AppSettings.ResourceRoot, filePath.Replace('\\', Path.DirectorySeparatorChar));
            return fullPath;
        }

        public static string ResolveCacheFile(string? filePath, bool createDirIfNotExists = false)
        {
            if (string.IsNullOrEmpty(AppSettings.ResourceCache))
            {
                throw new Exception("ResourceRoot is not set in AppConfig. Please check your configuration.");
            }

            if (string.IsNullOrEmpty(filePath))
            {
                filePath = "";
            }
            var cacheFile = Path.Combine(AppSettings.ResourceCache, filePath.Replace('/', Path.DirectorySeparatorChar).TrimStart(Path.DirectorySeparatorChar));

            if (createDirIfNotExists)
            {
                var cacheDir = Path.GetDirectoryName(cacheFile);
                if (cacheDir != null && !Directory.Exists(cacheDir))
                {
                    Directory.CreateDirectory(cacheDir);
                }
            }
            return cacheFile;
        }

        public static string GetContentType(string fileName)
        {
            var ext = Path.GetExtension(fileName);
            if (ExtendMimeType.ContainsKey(ext))
            {
                return ExtendMimeType[ext];
            }

            var provider = new FileExtensionContentTypeProvider();
            if (!provider.TryGetContentType(fileName, out var contentType))
            {
                contentType = "application/octet-stream";
            }
            return contentType;
        }

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
            var full = ResolveRepFile(directory);
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
            var full = ResolveRepFile(directory);

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

        public static FileEntry GetFile(string file)
        {
            var repFile = ResolveRepFile(file);

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

        public static bool RenameRepFile(string? filePath, string newFileName)
        {
            if (string.IsNullOrEmpty(AppSettings.ResourceRoot))
            {
                throw new Exception("ResourceRoot is not set in AppConfig. Please check your configuration.");
            }

            // newFileName 是否合法
            if (string.IsNullOrEmpty(newFileName) || newFileName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
            {
                throw new Exception("New file name is invalid.");
            }

            var srcFilePath = ResolveRepFile(filePath); // 原文件物理路径

            // 扩展名不允许修改
            var ext = Path.GetExtension(srcFilePath);
            if (!Path.GetExtension(newFileName).Equals(ext, StringComparison.OrdinalIgnoreCase))
            {
                throw new Exception("File extension cannot be changed.");
            }

            // 只修改文件名
            var newFilePath = Path.Combine(Path.GetDirectoryName(srcFilePath) ?? string.Empty, newFileName);
            if (File.Exists(newFilePath))
            {
                throw new Exception("同名文件已存在！");
            }
            File.Move(srcFilePath, newFilePath);

            // 移除缓存文件
            var srcCachedFilePath = ResolveCacheFile(filePath, false);
            if (File.Exists(srcCachedFilePath))
            {
                File.Delete(srcCachedFilePath);
            }


            return true;

        }


        private static Dictionary<string, string> ExtendMimeType = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { ".ts", "video/mp2t"},
            {".js", "text/plain"},
            {".html", "text/plain"},
            {".log", "text/plain"},
            {".md", "text/plain"},

        };
    }
}