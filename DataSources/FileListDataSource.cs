using NStack;
using System;
using System.Collections.Generic;
using System.Linq;
using Terminal.Gui;

namespace Devil7.Utils.GDriveCLI.DataSources
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
                text += (item.IsDirectory ? "      -      " : BytesToString(item.FileSize).PadLeft(sizePad));
            }

            RenderUstr(driver, text, width);
        }

        public void UpdateIsstems(List<Models.FileListItem> items, string previousParentId)
        {
            this.Items.Clear();
            this.Items.AddRange(items);


            if (previousParentId != "root")
            {
                this.Items.Insert(0, new Models.FileListItem(previousParentId, "..", "", DateTime.Now, 0, Utils.Constants.MIME_GDRIVE_DIRECTORY));
            }
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

        private String BytesToString(long byteCount)
        {
            string[] suf = { "B", "KB", "MB", "GB", "TB", "PB", "EB" }; //Longs run out around EB
            if (byteCount == 0)
                return "0" + suf[0];
            long bytes = Math.Abs(byteCount);
            int place = Convert.ToInt32(Math.Floor(Math.Log(bytes, 1024)));
            double num = Math.Round(bytes / Math.Pow(1024, place), 1);
            return string.Format("{0}{1}", (Math.Sign(byteCount) * num).ToString(), suf[place]);
        }
        #endregion

        #region Sorting Methods
        public void DirectoriesFirst()
        {
            this.Items = this.Items.OrderBy((item) => item.Name == ".." ? 0 : item.IsDirectory ? 0 : 1).ToList();
        }
        #endregion
    }
}
