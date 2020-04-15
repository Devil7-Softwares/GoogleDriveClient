namespace Devil7.Utils.GDriveCLI.Utils
{
    public class Constants
    {
        public const string REGEX_AUTH_ERROR = ".*\\(Error:\\\"(?<Error>.*)\\\", Description:\\\"(?<Description>.*)\\\", Uri:\\\"(?<Uri>.*)\\\"\\)";
        public const string REGEX_FILE_COUNT = "(.*)\\(([0-9]+)\\)";

        public const string MIME_GDRIVE_DIRECTORY = "application/vnd.google-apps.folder";
        public const string URI_GDRIVE_REDIRECT = "urn:ietf:wg:oauth:2.0:oob";
    }
}
