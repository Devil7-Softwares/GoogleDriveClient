namespace Devil7.Utils.GoogleDriveClient.Utils
{
    public class Constants
    {
        public const string REGEX_AUTH_ERROR = ".*\\(Error:\\\"(?<Error>.*)\\\", Description:\\\"(?<Description>.*)\\\", Uri:\\\"(?<Uri>.*)\\\"\\)";
        public const string REGEX_FILE_COUNT = "(.*)\\(([0-9]+)\\)";

        public const string MIME_GDRIVE_DIRECTORY = "application/vnd.google-apps.folder";
    }
}
