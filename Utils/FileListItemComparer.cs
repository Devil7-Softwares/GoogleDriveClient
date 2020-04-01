using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Devil7.Utils.GDriveCLI.Utils
{
    class FileListItemComparer : IComparer<Models.FileListItem>
    {
        public int Compare([AllowNull] Models.FileListItem x, [AllowNull] Models.FileListItem y)
        {
            if (Settings.SortBy == SortBy.Name)
            {
                return Settings.SortOrder == SortOrder.Ascending ? x.Name.CompareTo(y.Name) : y.Name.CompareTo(x.Name);
            }
            else
            {
                return Settings.SortOrder == SortOrder.Ascending ? x.LastModified.CompareTo(y.LastModified) : y.LastModified.CompareTo(x.LastModified);
            }
        }
    }
}
