﻿using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Devil7.Utils.GDriveCLI.Utils
{
    public class Drive
    {
        #region Variables
        private static string[] Scopes = { DriveService.Scope.Drive };
        private static string ApplicationName = "Google Drive CLI Client";

        private static DriveService Service = null;

        private static readonly IAuthorizationCodeFlow AuthFlow = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
        {
            ClientSecrets = new ClientSecrets { ClientId = Utils.Secrets.GDRIVE_CLIENT_ID, ClientSecret = Utils.Secrets.GDRIVE_CLIENT_SECRET },
            Scopes = Scopes,
            DataStore = new FileDataStore("Drive.Api.Auth.Store")
        });
        #endregion

        #region Public Functions
        public static bool IsAuthorized()
        {
            return AuthFlow.LoadTokenAsync(Environment.UserName, new CancellationToken()).Result != null;
        }

        public static bool Authorize()
        {
            Console.WriteLine("Authendication needed!");
            Console.WriteLine("Go to the following url in your browser:");
            Console.WriteLine(AuthFlow.CreateAuthorizationCodeRequest(Utils.Constants.URI_GDRIVE_REDIRECT).Build().ToString());

            Console.WriteLine();
            Console.WriteLine("Enter verification code: ");

            string token = Console.ReadLine();

            try
            {
                TokenResponse response = AuthFlow.ExchangeCodeForTokenAsync(Environment.UserName, token, Utils.Constants.URI_GDRIVE_REDIRECT, new CancellationToken()).Result;
                AuthFlow.DataStore.StoreAsync(Environment.UserName, response).Wait();
                return true;
            }
            catch (Exception ex)
            {
                string errorMessage = ex.Message;

                Match match = (new Regex(Utils.Constants.REGEX_AUTH_ERROR)).Match(ex.Message);
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
                    HttpClientInitializer = new UserCredential(AuthFlow, Environment.UserName, response, Utils.Secrets.GDRIVE_PROJECT_ID),
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
                    FilesResource.ListRequest listRequest = Service.Files.List();
                    listRequest.Q = string.Format("\"{0}\" in parents", id);
                    listRequest.Fields = "nextPageToken, files(id, name, ownedByMe, modifiedTime, size, mimeType)";

                    IList<Google.Apis.Drive.v3.Data.File> files = listRequest.Execute().Files;
                    if (files != null && files.Count > 0)
                    {
                        foreach (var file in files)
                        {
                            items.Add(new Models.FileListItem(file.Id, file.Name, GetOwnersName(file), file.ModifiedTime.GetValueOrDefault(), file.Size.GetValueOrDefault(), file.MimeType));
                        }
                    }
                    else
                    {
                        Console.WriteLine("No files found.");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                return items;
            });
        }
        #endregion

        #region Private Functions
        private static string GetOwnersName(Google.Apis.Drive.v3.Data.File file)
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