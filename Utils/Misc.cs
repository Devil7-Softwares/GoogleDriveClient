﻿using Google.Apis.Drive.v3.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Devil7.Utils.GDriveCLI.Utils
{
    public class Misc
    {
        #region Public Methods
        public static Models.FileListItem FileToItem(File file)
        {
            return new Models.FileListItem(file.Id, file.Name, GetOwnersName(file), file.ModifiedTime.GetValueOrDefault(), file.Size.GetValueOrDefault(), file.MimeType);
        }

        public static String BytesToString(long byteCount)
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

        #region Private Methods
        private static string GetOwnersName(File file)
        {
            string owner = "me";
            if (file != null && !file.OwnedByMe.GetValueOrDefault() && file.Owners != null)
            {
                owner = string.Join(", ", file.Owners.Select((item) => item.DisplayName));
            }
            return owner;
        }
        #endregion
    }
}
