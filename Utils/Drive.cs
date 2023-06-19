using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Drive.v3;
using Google.Apis.Drive.v3.Data;
using Google.Apis.Services;
using Google.Apis.Upload;
using Google.Apis.Util.Store;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Devil7.Utils.GoogleDriveClient.Utils
{
    public class Drive
    {
        #region Variables
        private static readonly string[] Scopes = { DriveService.Scope.Drive };
        private static readonly string ApplicationName = "Google Drive CLI Client";

        private static DriveService Service = null;

        private static readonly IAuthorizationCodeFlow AuthFlow = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
        {
            ClientSecrets = new ClientSecrets { ClientId = Secrets.GDRIVE_CLIENT_ID, ClientSecret = Secrets.GDRIVE_CLIENT_SECRET },
            Scopes = Scopes,
            DataStore = new FileDataStore("Drive.Api.Auth.Store")
        });
        #endregion

        #region Properties
        public static FilesResource.CreateMediaUpload CurrentUploadRequest { get; }
        #endregion

        #region Public Functions
        public static bool IsAuthorized()
        {
            return AuthFlow.LoadTokenAsync(Environment.UserName, new CancellationToken()).Result != null;
        }

        public static bool Authorize()
        {
            Console.WriteLine("Authendication needed!");

            string redirectUrl = string.Format("http://localhost:{0}/", Misc.GetRandomUnusedPort());
            string token = "";
            string url = AuthFlow.CreateAuthorizationCodeRequest(redirectUrl).Build().ToString();

            if (Misc.IsRunningInsideSSH())
            {
                Console.WriteLine("Looks like you are running inside an SSH connection. Cannot create a local redirect server.\nGo to the following url in your browser and copy, paste the token from redirected URL:");
                Console.WriteLine(url);

                Console.WriteLine();
                Console.WriteLine("Enter verification code: ");

                token = Console.ReadLine();
            }
            else
            {
                Console.WriteLine("Opening browser...");
                Misc.LaunchURL(url);

                Console.WriteLine("Waiting for redirect...");
                using (CancellationTokenSource cts = new CancellationTokenSource())
                {
                    Task.Run(async delegate ()
                    {
                        await StartRedirectServer(redirectUrl, cts.Token, delegate (string code)
                        {
                            token = code;
                            cts.Cancel();
                        });
                    }, cts.Token).Wait();
                }
            }

            try
            {
                TokenResponse response = AuthFlow.ExchangeCodeForTokenAsync(Environment.UserName, token, redirectUrl, new CancellationToken()).Result;
                AuthFlow.DataStore.StoreAsync(Environment.UserName, response).Wait();
                return true;
            }
            catch (Exception ex)
            {
                string errorMessage = ex.Message;

                Match match = (new Regex(Constants.REGEX_AUTH_ERROR)).Match(ex.Message);
                if (match.Success)
                {
                    errorMessage = string.Format("{0} ({1})", match.Groups["Description"], match.Groups["Error"]);
                }

                Console.WriteLine(string.Format("Authendication Failed! {0}\nPlease try again...", errorMessage));
            }

            return false;
        }

        public static bool Init()
        {
            try
            {
                TokenResponse response = AuthFlow.LoadTokenAsync(Environment.UserName, new CancellationToken()).Result;
                Service = new DriveService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = new UserCredential(AuthFlow, Environment.UserName, response, Secrets.GDRIVE_PROJECT_ID),
                    ApplicationName = ApplicationName
                });
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(string.Format("Google drive API initialization failed! {0}", ex.Message));
            }
            return false;
        }

        public static Task<List<Models.FileListItem>> ListFiles(string id)
        {
            return Task.Run(delegate ()
            {
                List<Models.FileListItem> items = new List<Models.FileListItem>();
                try
                {
                    string NextPageToken = null;
                    int PageNumber = 1;
                    while (!string.IsNullOrWhiteSpace(NextPageToken) || PageNumber == 1)
                    {
                        FilesResource.ListRequest listRequest = Service.Files.List();
                        listRequest.Q = string.Format("\"{0}\" in parents and trashed=false", id);
                        listRequest.Fields = "nextPageToken, files(id, name, ownedByMe, modifiedTime, size, mimeType)";

                        if (PageNumber > 1)
                            listRequest.PageToken = NextPageToken;

                        FileList fileList = listRequest.Execute();

                        IList<File> files = fileList.Files;
                        NextPageToken = fileList.NextPageToken;

                        if (files != null && files.Count > 0)
                        {
                            foreach (var file in files)
                            {
                                items.Add(Misc.FileToItem(file));
                            }
                        }
                        else
                        {
                            Console.WriteLine("No files found.");
                        }

                        PageNumber++;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                return items;
            });
        }

        public static Task<Models.FileListItem> NewFolder(string parent, string folderName)
        {
            return Task.Run(delegate ()
            {
                File newFolder = new File()
                {
                    Name = folderName,
                    MimeType = Constants.MIME_GDRIVE_DIRECTORY,
                    Parents = new List<string>() { parent }
                };

                FilesResource.CreateRequest createRequest = Service.Files.Create(newFolder);
                createRequest.Fields = "id, name, ownedByMe, modifiedTime, size, mimeType";

                return Misc.FileToItem(createRequest.Execute());
            });
        }

        public static FilesResource.CreateMediaUpload StartUpload(string parent, System.IO.FileInfo fileInfo, System.IO.FileStream fileStream, CancellationToken cancellationToken)
        {
            string MimeType = MimeMapping.MimeUtility.GetMimeMapping(fileInfo.Extension);

            File file = new File()
            {
                Name = fileInfo.Name,
                MimeType = MimeType,
                Parents = new List<string>() { parent }
            };

            FilesResource.CreateMediaUpload uploadRequest = new FilesResource.CreateMediaUpload(Service, file, fileStream, MimeType)
            {
                Fields = "id, name, ownedByMe, modifiedTime, size, mimeType",
                ChunkSize = ResumableUpload.MinimumChunkSize
            };

            uploadRequest.UploadAsync(cancellationToken);

            return uploadRequest;
        }

        public static FilesResource.GetRequest StartDownload(string id, System.IO.FileStream fileStream, CancellationToken cancellationToken)
        {
            FilesResource.GetRequest downlodRequest = new FilesResource.GetRequest(Service, id);

            downlodRequest.MediaDownloader.ChunkSize = ResumableUpload.MinimumChunkSize;

            downlodRequest.DownloadAsync(fileStream, cancellationToken);

            return downlodRequest;
        }

        public static Task<bool> MoveToTrash(string id)
        {
            return Task.Run(() =>
            {
                bool success = false;

                try
                {
                    File file = new File()
                    {
                        Trashed = true
                    };

                    FilesResource.UpdateRequest deleteRequest = Service.Files.Update(file, id);
                    deleteRequest.Fields = "id, name, trashed";

                    File r = deleteRequest.Execute();

                    success = r.Trashed.GetValueOrDefault();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }

                return success;
            });
        }

        public static Task<bool> Delete(string id)
        {
            return Task.Run(() =>
            {
                bool success = false;

                try
                {
                    FilesResource.DeleteRequest deleteRequest = Service.Files.Delete(id);
                    success = deleteRequest.Execute() == "";
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }

                return success;
            });
        }

        #endregion

        #region Private Functions
        private static async Task StartRedirectServer(string url, CancellationToken cancellationToken, Action<string> callback)
        {
            var app = WebApplication.CreateBuilder().Build();

            app.MapGet("/", async (context) =>
            {
                string code = context.Request.Query["code"].ToString();
                await context.Response.Body.WriteAsync(Encoding.UTF8.GetBytes("Authendication successful! You can close this tab now."), cancellationToken);
                await context.Response.CompleteAsync();
                callback(code);
            });

            cancellationToken.Register(() =>
            {
                app.StopAsync().Wait();
            });

            await app.RunAsync(url);
        }
        #endregion
    }
}
