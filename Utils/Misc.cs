using Google.Apis.Drive.v3.Data;
using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;

namespace Devil7.Utils.GoogleDriveClient.Utils
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

        // Opening URLs using Process.Start is broken
        // Read more: https://brockallen.com/2016/09/24/process-start-for-urls-on-net-core/
        public static void LaunchURL(string url)
        {
            try
            {
                Process.Start(url);
            }
            catch
            {
                // hack because of this: https://github.com/dotnet/corefx/issues/10361
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    url = url.Replace("&", "^&");
                    Process.Start(new ProcessStartInfo("cmd", $"/c start {url}") { CreateNoWindow = true });
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    Process.Start("xdg-open", url);
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    Process.Start("open", url);
                }
                else
                {
                    throw;
                }
            }
        }

        public static bool IsRunningInsideSSH()
        {
            return Environment.GetEnvironmentVariable("SSH_CLIENT") != null || Environment.GetEnvironmentVariable("SSH_TTY") != null;
        }

        public static int GetRandomUnusedPort()
        {
            // Use UDP to find an unused port since TCP is causing blue screens on Windows
            var udp = new UdpClient(0, AddressFamily.InterNetwork);
            int port = ((IPEndPoint)udp.Client.LocalEndPoint).Port;
            udp.Close();
            return port;
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
