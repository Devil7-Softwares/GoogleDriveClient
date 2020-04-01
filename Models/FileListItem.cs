using System;

namespace Devil7.Utils.GDriveCLI.Models
{
    public class FileListItem
    {
        #region Properties
        public string Id { get; }
        public string Name { get; }
        public string Owner { get; }
        public DateTime LastModified { get; }
        public long FileSize { get; }

        public bool IsDirectory { get; }
        public bool IsSelected { get; internal set; }
        #endregion

        #region Constructor
        public FileListItem(string Id, string Name, string Owner, DateTime LastModified, long FileSize, string MimeType)
        {
            this.Id = Id;
            this.Name = Name;
            this.Owner = Owner;
            this.LastModified = LastModified;
            this.FileSize = FileSize;

            this.IsDirectory = (MimeType == Utils.Constants.MIME_GDRIVE_DIRECTORY);
        } 
        #endregion

        public override string ToString()
        {
            return this.Name;
        }
    }
}
