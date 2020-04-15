using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Devil7.Utils.GoogleDriveClient.Utils
{
    public class Settings
    {
        #region Variables
        private static readonly string SettingsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Devil7.Utils.GoogleDriveClient.json");
        #endregion

        #region Properties
        [JsonProperty]
        public static SortBy SortBy { get; set; }

        [JsonProperty]
        public static SortOrder SortOrder { get; set; }
        #endregion

        #region Public Methods
        public static void Load()
        {
            if (File.Exists(SettingsPath))
            {
                JsonConvert.DeserializeObject<Settings>(File.ReadAllText(SettingsPath));
            }
        }

        public static void Save()
        {
            File.WriteAllText(SettingsPath, JsonConvert.SerializeObject(new Settings()));
        }
        #endregion
    }
}
