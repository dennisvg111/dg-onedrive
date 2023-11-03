using DG.OneDrive.Serialized.Resources;

namespace DG.OneDrive.Serialized.DriveItems
{
    public class DriveItem : BaseItem
    {
        public string cTag { get; set; }

        public FileSystemInfo fileSystemInfo { get; set; }
        public Folder folder { get; set; }
        public long size { get; set; }
        public SpecialFolder specialFolder { get; set; }

        public bool IsFolder()
        {
            return folder != null;
        }
        public bool IsSpecialFolder()
        {
            return specialFolder != null;
        }
    }
}
