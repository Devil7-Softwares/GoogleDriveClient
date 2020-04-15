using NStack;
using System;
using System.Collections.Generic;
using System.Linq;
using Terminal.Gui;

namespace Devil7.Utils.GoogleDriveClient.DataSources
{
    public class FileListDataSource : IListDataSource
    {
        #region Properties
        public int Count { get => Items.Count; }

        public List<Models.FileListItem> Items { get; private set; }
        #endregion

        #region Constructor
        public FileListDataSource()
        {
            this.Items = new List<Models.FileListItem>();
        }

        public FileListDataSource(List<Models.FileListItem> Items)
        {
            this.Items = Items;
        }
        #endregion

        #region Public Methods
        public bool IsMarked(int index)
        {
            return this.Items[index].IsSelected;
        }

        public void SetMark(int index, bool value)
        {
            Models.FileListItem item = this.Items[index];
            if (item.Name != "..")
                item.IsSelected = value;
        }

        public void Render(ListView container, ConsoleDriver driver, bool selected, int index, int col, int line, int width)
        {
            var text = "";
            Models.FileListItem item = Items[index];

            if (item.Name == "..")
            {
                text = "..";
            }
            else
            {
                string dateFormat = "dd/MM/yyyy hh:mm:ss tt";

                int datePad = dateFormat.Length + 2;
                int ownerPad = 20;
                int sizePad = 13; // "  1023.01MB"
                int namePad = width - (ownerPad + datePad + sizePad + 4);

                string name = item.Name.Length > (namePad - 1) ? item.Name.Substring(0, namePad - 4) + "..." : item.Name;
                string owner = item.Owner.Length > (ownerPad - 2) ? item.Owner.Substring(0, ownerPad - 5) + "..." : item.Owner;

                text += name.PadRight(namePad - 1);
                text += "|";
                text += " " + owner.PadRight(ownerPad - 1);
                text += "|";
                text += (" " + item.LastModified.ToString(dateFormat) + " ");
                text += "|";
                text += (item.IsDirectory ? "      -      " : Utils.Misc.BytesToString(item.FileSize).PadLeft(sizePad));
            }

            RenderUstr(driver, text, width);
        }
        #endregion

        #region Private Methods
        private void RenderUstr(ConsoleDriver driver, ustring ustr, int width)
        {
            var byteLen = ustr.Length;
            var used = 0;
            for (var i = 0; i < byteLen;)
            {
                var (rune, size) = Utf8.DecodeRune(ustr, i, i - byteLen);
                var count = System.Rune.ColumnWidth(rune);
                if (used + count >= width)
                    break;
                driver.AddRune(rune);
                used += count;
                i += size;
            }

            for (; used < width; used++) driver.AddRune(' ');
        }
        #endregion

        #region Sorting Methods
        public void Sort()
        {
            // Sort by User Preference
            this.Items.Sort(new Utils.FileListItemComparer());

            // Put Directories First
            this.Items = this.Items.OrderBy((item) => item.Name == ".." ? 0 : item.IsDirectory ? 1 : 2).ToList();
        }
        #endregion
    }
}
