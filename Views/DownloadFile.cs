using Google.Apis.Drive.v3;
using Google.Apis.Download;
using System;
using System.Threading;
using Terminal.Gui;
using System.Text.RegularExpressions;

namespace Devil7.Utils.GDriveCLI.Views
{
    class DownloadFile
    {
        #region Variables
        private static DownloadStatus _Status;
        #endregion

        #region Properties
        private static FilesResource.GetRequest DownloadRequest { get; set; }
        private static CancellationTokenSource CancellationTokenSource { get; set; }
        private static Dialog ProgressDialog { get; set; }
        private static Label StatusLabel { get; set; }
        private static ProgressBar ProgressBar { get; set; }

        private static System.IO.FileStream FileStream { get; set; }
        private static long TotalSize { get; set; }
        private static long DownloadedSize { get; set; }

        private static DownloadStatus Status { get => _Status; set { _Status = value; UpdateStatus(); } }
        private static Models.FileListItem SelectedItem { get; set; }
        private static System.IO.FileInfo DownloadedFileInfo { get; set; }
        #endregion

        #region Public Methods
        public static void Start(Models.FileListItem selectedItem)
        {
            try
            {
                OpenDialog openDialog = new OpenDialog("Download File", "Select file to upload");
                openDialog.CanChooseDirectories = true;
                openDialog.CanChooseFiles = false;
                Application.Run(openDialog);

                string directoryPath = openDialog.FilePath.ToString().Trim();

                if (string.IsNullOrEmpty(directoryPath))
                    return;

                DownloadedFileInfo = new System.IO.FileInfo(System.IO.Path.Combine(directoryPath, selectedItem.Name));

                if (DownloadedFileInfo.Exists)
                {
                    int result = MessageBox.Query(50, 7, "Warning", "File with same name as selected for download alreay exists!", "Overwrite", "Keep Both", "Skip");
                    switch (result)
                    {
                        case 0:
                            Console.WriteLine("Overwriting file {0}", DownloadedFileInfo.Name);
                            break;
                        case 1:
                            do
                            {
                                string fileName = System.IO.Path.GetFileNameWithoutExtension(DownloadedFileInfo.FullName);
                                string extension = System.IO.Path.GetExtension(DownloadedFileInfo.FullName);

                                Match existingCountMatch = Regex.Match(fileName, Utils.Constants.REGEX_FILE_COUNT);
                                if (existingCountMatch.Success)
                                {
                                    fileName = string.Format("{0} ({1})", existingCountMatch.Groups[1].Value.Trim(), int.Parse(existingCountMatch.Groups[2].Value) + 1);
                                }
                                else
                                {
                                    fileName += " (1)";
                                }

                                DownloadedFileInfo = new System.IO.FileInfo(System.IO.Path.Combine(directoryPath, fileName + extension));
                            } while (DownloadedFileInfo.Exists);
                            break;
                        case 2:
                            return;
                    }
                }

                TotalSize = selectedItem.FileSize;
                DownloadedSize = 0;

                FileStream = new System.IO.FileStream(DownloadedFileInfo.FullName, System.IO.FileMode.Create);
                CancellationTokenSource = new CancellationTokenSource();

                SelectedItem = selectedItem;

                DownloadRequest = Utils.Drive.StartDownload(selectedItem.Id, FileStream, CancellationTokenSource.Token);

                ProgressDialog = new Dialog("Downloading File", 50, 10);

                Label fileNameLabel = new Label(DownloadedFileInfo.Name)
                {
                    X = 1,
                    Y = 1
                };
                ProgressDialog.Add(fileNameLabel);

                StatusLabel = new Label("Starting...")
                {
                    X = 1,
                    Y = 2
                };
                ProgressDialog.Add(StatusLabel);

                ProgressBar = new ProgressBar()
                {
                    X = 1,
                    Y = 3,
                    Width = 45
                };
                ProgressDialog.Add(ProgressBar);

                DownloadRequest.MediaDownloader.ProgressChanged += DownloadRequest_ProgressChanged;

                Button btnCancel = new Button("Cancel");
                btnCancel.Clicked += Cancel;
                ProgressDialog.AddButton(btnCancel);

                Application.Run(ProgressDialog);
            }
            catch (Exception ex)
            {
                MessageBox.ErrorQuery(50, 7, "Error", "Download failed! " + ex.Message, "Ok");
                Cancel();
            }
        }
        #endregion

        #region Private Methods

        private static void Cancel()
        {
            if (Status != DownloadStatus.Completed && Status != DownloadStatus.Failed &&
                CancellationTokenSource != null && !CancellationTokenSource.IsCancellationRequested) CancellationTokenSource.Cancel();

            if (FileStream != null) FileStream.Close();

            if (Status == DownloadStatus.Completed)
            {
                DownloadedFileInfo.LastWriteTime = SelectedItem.LastModified;
            }

            Application.MainLoop.Invoke(() =>
            {
                if (ProgressDialog != null && ProgressDialog.Running) ProgressDialog.Running = false;
            });
        }

        private static void UpdateStatus()
        {
            string statusText = "Status Unknown";
            float progress = ((float)DownloadedSize / (float)TotalSize);
            switch (Status)
            {
                case DownloadStatus.NotStarted:
                    statusText = "Waiting for start...";
                    break;
                case DownloadStatus.Downloading:
                    statusText = string.Format("Downloading {2:0}% ({0} of {1})", Utils.Misc.BytesToString(DownloadedSize), Utils.Misc.BytesToString(TotalSize), (progress * 100));
                    break;
                default:
                    statusText = "Starting...";
                    break;
            }

            Application.MainLoop.Invoke(() =>
            {
                ProgressBar.Fraction = progress;
                StatusLabel.Text = statusText;
            });
        }
        #endregion

        #region Event Handlers
        private static void DownloadRequest_ProgressChanged(IDownloadProgress e)
        {
            switch (e.Status)
            {
                case DownloadStatus.Downloading:
                    DownloadedSize = e.BytesDownloaded;
                    break;
                case DownloadStatus.Completed:
                    _Status = DownloadStatus.Completed;
                    Application.MainLoop.Invoke(() =>
                    {
                        MessageBox.Query(50, 7, "Done", "Download completed successfully", "Ok");
                        Cancel();
                    });
                    break;
                case DownloadStatus.Failed:
                    Application.MainLoop.Invoke(() =>
                    {
                        if (e.Exception != null)
                            MessageBox.ErrorQuery(50, 7, "Error", "Download failed! " + e.Exception.Message, "Ok");
                        else
                            MessageBox.ErrorQuery(50, 7, "Error", "Download failed due to unknown reason!", "Ok");
                        Cancel();
                    });
                    break;
                case DownloadStatus.NotStarted:
                    Console.WriteLine("DownloadFile: Download not started yet!");
                    break;
                default:
                    Console.WriteLine("DownloadFile: Download starting...!");
                    break;
            }
            Status = e.Status;
        }
        #endregion
    }
}
