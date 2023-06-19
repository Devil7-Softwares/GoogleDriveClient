using Google.Apis.Drive.v3;
using Google.Apis.Drive.v3.Data;
using Google.Apis.Upload;
using System;
using System.Threading;
using Terminal.Gui;

namespace Devil7.Utils.GoogleDriveClient.Views
{
    class UploadFile
    {
        #region Variables
        private static UploadStatus _Status;
        #endregion

        #region Properties
        private static FilesResource.CreateMediaUpload UploadRequest { get; set; }
        private static CancellationTokenSource CancellationTokenSource { get; set; }
        private static Dialog ProgressDialog { get; set; }
        private static Terminal.Gui.Label StatusLabel { get; set; }
        private static ProgressBar ProgressBar { get; set; }
        private static Button PauseButton { get; set; }

        private static System.IO.FileStream FileStream { get; set; }
        private static long TotalSize { get; set; }
        private static long UploadedSize { get; set; }

        private static UploadStatus Status { get => _Status; set { _Status = value; UpdateStatus(); } }
        private static File UploadedFile { get; set; }

        public static bool IsPaused { get; private set; }
        #endregion

        #region Public Methods
        public static void Start()
        {
            try
            {
                OpenDialog openDialog = new OpenDialog("Upload File", "Select file to upload");
                Application.Run(openDialog);

                string filePath = openDialog.FilePath.ToString().Trim();

                if (string.IsNullOrEmpty(filePath))
                    return;

                System.IO.FileInfo fileInfo = new System.IO.FileInfo(filePath);

                if (!fileInfo.Exists)
                {
                    MessageBox.ErrorQuery(50, 7, "Error", "Invalid/No file selected. Please try again.", "Ok");
                    return;
                }

                TotalSize = fileInfo.Length;
                UploadedSize = 0;
                IsPaused = false;

                FileStream = new System.IO.FileStream(filePath, System.IO.FileMode.Open);
                CancellationTokenSource = new CancellationTokenSource();

                UploadRequest = Utils.Drive.StartUpload(MyDrive.CurrentDirectory.Id, fileInfo, FileStream, CancellationTokenSource.Token);

                ProgressDialog = new Dialog("Uploading File", 50, 10);

                Terminal.Gui.Label fileNameLabel = new Terminal.Gui.Label(System.IO.Path.GetFileName(filePath))
                {
                    X = 1,
                    Y = 1
                };
                ProgressDialog.Add(fileNameLabel);

                StatusLabel = new Terminal.Gui.Label("Starting...")
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

                UploadRequest.ProgressChanged += UploadRequest_ProgressChanged;
                UploadRequest.ResponseReceived += UploadRequest_ResponseReceived;

                PauseButton = new Button("Pause", true);
                PauseButton.Clicked += Pause;
                ProgressDialog.AddButton(PauseButton);

                Button btnCancel = new Button("Cancel");
                btnCancel.Clicked += Cancel;
                ProgressDialog.AddButton(btnCancel);

                Application.Run(ProgressDialog);
            }
            catch (Exception ex)
            {
                MessageBox.ErrorQuery(50, 7, "Error", "Upload failed! " + ex.Message, "Ok");
                Cancel();
            }
        }
        #endregion

        #region Private Methods
        private static void Pause()
        {
            if (IsPaused)
            {
                IsPaused = false;
                CancellationTokenSource = new CancellationTokenSource();
                UploadRequest.ResumeAsync(CancellationTokenSource.Token);

                Application.MainLoop.Invoke(() => PauseButton.Text = "Pause");
            }
            else
            {
                IsPaused = true;
                CancellationTokenSource.Cancel();

                Application.MainLoop.Invoke(() => PauseButton.Text = "Resume");
            }
        }

        private static void Cancel()
        {
            if (Status != UploadStatus.Completed && Status != UploadStatus.Failed &&
                CancellationTokenSource != null && !CancellationTokenSource.IsCancellationRequested) CancellationTokenSource.Cancel();

            if (FileStream != null) FileStream.Close();

            Application.MainLoop.Invoke(() =>
            {
                if (ProgressDialog != null && ProgressDialog.Running) ProgressDialog.Running = false;

                if (Status == UploadStatus.Completed)
                {
                    Models.FileListItem listItem = Utils.Misc.FileToItem(UploadedFile);
                    MyDrive.Files.Add(listItem);

                    MyDrive.SortItems();
                    MyDrive.SelectItem(listItem);
                }
            });
        }

        private static void UpdateStatus()
        {
            string statusText = "Status Unknown";
            float progress = ((float)UploadedSize / (float)TotalSize);
            switch (Status)
            {
                case UploadStatus.NotStarted:
                    statusText = "Waiting for start...";
                    break;
                case UploadStatus.Starting:
                    statusText = "Starting...";
                    break;
                case UploadStatus.Uploading:
                    statusText = string.Format("Uploading {2:0}% ({0} of {1})", Utils.Misc.BytesToString(UploadedSize), Utils.Misc.BytesToString(TotalSize), (progress * 100));
                    break;
                case UploadStatus.Failed:
                    if (IsPaused) statusText = string.Format("Paused at {2:0}% ({0} of {1})", Utils.Misc.BytesToString(UploadedSize), Utils.Misc.BytesToString(TotalSize), (((float)UploadedSize / (float)TotalSize) * 100));
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
        private static void UploadRequest_ProgressChanged(IUploadProgress e)
        {
            switch (e.Status)
            {
                case UploadStatus.Uploading:
                    UploadedSize = e.BytesSent;
                    break;
                case UploadStatus.Completed:
                    _Status = UploadStatus.Completed;

                    Application.MainLoop.Invoke(() =>
                    {
                        MessageBox.Query(50, 7, "Done", "Upload completed successfully", "Ok");
                        Cancel();
                    });
                    break;
                case UploadStatus.Failed:
                    if (!IsPaused)
                    {
                        Application.MainLoop.Invoke(() =>
                        {
                            if (e.Exception != null)
                                MessageBox.ErrorQuery(50, 7, "Error", "Upload failed! " + e.Exception.Message, "Ok");
                            else
                                MessageBox.ErrorQuery(50, 7, "Error", "Upload failed due to unknown reason!", "Ok");
                            Cancel();
                        });
                    }
                    break;
                case UploadStatus.NotStarted:
                    Console.WriteLine("UploadFile: Upload not started yet!");
                    break;
                case UploadStatus.Starting:
                    Console.WriteLine("UploadFile: Upload starting...!");
                    break;
            }
            Status = e.Status;
        }

        private static void UploadRequest_ResponseReceived(File File)
        {
            UploadedFile = File;
        }
        #endregion
    }
}
