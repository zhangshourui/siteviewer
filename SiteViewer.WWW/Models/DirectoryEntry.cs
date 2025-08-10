
namespace SiteViewer.WWW.Models
{
    public class DirectoryEntry
    {
        public string DirectoryName { get; set; }
        public string ParentDirectory { get; set; }
        public DateTime LastTime { get; internal set; }
        public string Link
        {
            get
            {
                if (string.IsNullOrEmpty(ParentDirectory))
                {
                    return $"/{DirectoryName}";
                }
                return Path.Combine(ParentDirectory, DirectoryName).Replace(Path.DirectorySeparatorChar, '/');
            }
        }
        public DirectoryEntry(string fileName, string parentDirectory)
        {
            DirectoryName = fileName;
            ParentDirectory = parentDirectory;
        }

    }
}
