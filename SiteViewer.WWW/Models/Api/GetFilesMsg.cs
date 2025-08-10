using System.Runtime.Serialization;

namespace SiteViewer.WWW.Models.Api
{
    public class GetRepMsg
    {
        public string? ParentDir { get; set; } = null;
    }
    public class GetRepMsgResp : MsgBase
    {
        public IEnumerable<FileEntry> Files { get; set; } = new List<FileEntry>();
        public IEnumerable<DirectoryEntry> Directories { get; set; } = new List<DirectoryEntry>();
        public string ParentDirectory { get; set; } = "/";
        public string? ParentDirectoryLink { get; set; }
    };

    public class ViewFileMsg
    {
        public string? File { get; set; } = null;
    }
    public class PlayFileMsg
    {
        public string? File { get; set; } = null;
    }
    public class RenameMsg
    {
        public string? File { get; set; } = null;
        public string? NewFileName { get; set; }
    }
}
