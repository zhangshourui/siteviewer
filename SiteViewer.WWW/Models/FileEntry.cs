
namespace SiteViewer.WWW.Models
{
    public class FileEntry
    {
        private string? fileId;

        public string FileName { get; set; }
        public string Directory { get; set; }
        public string FileId
        {
            get
            {
                if (!string.IsNullOrEmpty(fileId))
                {
                    return fileId;
                }
                return Utils.Crypto.SHA256Hash($"{Directory}${FileName}");
            }
        }
        public string Link
        {
            get
            {
                if (string.IsNullOrEmpty(Directory))
                {
                    return $"/{FileName}";
                }
                return Path.Combine(Directory, FileName).Replace(Path.DirectorySeparatorChar, '/');
            }
        }
        public DateTime LastTime { get; set; }
        public long FileSize { get;  set; }
        public string FileSizeReadable
        {
            get
            {
                if (FileSize < 1024)
                {
                    return $"{FileSize} B";
                }
                else if (FileSize < 1024 * 1024)
                {
                    return $"{FileSize / 1024.0:F2} KB";
                }
                else if (FileSize < 1024 * 1024 * 1024)
                {
                    return $"{FileSize / (1024.0 * 1024):F2} MB";
                }
                else
                {
                    return $"{FileSize / (1024.0 * 1024 * 1024):F2} GB";
                }

            }
        }

        public FileEntry(string fileName, string directory)
        {
            fileId = null;
            FileName = fileName;
            Directory = directory;

        }

    }
}
