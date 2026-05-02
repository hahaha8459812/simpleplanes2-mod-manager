using SimplePlanes2ModManager.Models;
using System;
using System.IO;
using System.Web.Script.Serialization;

namespace SimplePlanes2ModManager.Services
{
    internal sealed class ManagerSettingsService
    {
        private readonly JavaScriptSerializer _serializer = new JavaScriptSerializer();
        private readonly string _settingsPath;

        public ManagerSettingsService()
        {
            string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string settingsDirectory = Path.Combine(appData, "SimplePlanes2ModManager");
            Directory.CreateDirectory(settingsDirectory);
            _settingsPath = Path.Combine(settingsDirectory, "settings.json");
        }

        public ManagerSettings Load()
        {
            if (!File.Exists(_settingsPath))
            {
                return new ManagerSettings
                {
                    gameDirectory = GetDefaultGameDirectory(),
                    language = "zh-CN"
                };
            }

            try
            {
                string json = File.ReadAllText(_settingsPath);
                ManagerSettings settings = _serializer.Deserialize<ManagerSettings>(json);
                if (settings == null)
                {
                    return CreateDefaultSettings();
                }

                EnsureDefaults(settings);
                return settings;
            }
            catch
            {
                return CreateDefaultSettings();
            }
        }

        public void Save(ManagerSettings settings)
        {
            if (settings == null)
            {
                throw new ArgumentNullException("settings");
            }

            string json = _serializer.Serialize(settings);
            File.WriteAllText(_settingsPath, json);
        }

        public void SaveLanguage(string language)
        {
            ManagerSettings settings = Load();
            settings.language = string.Equals(language, "en-US", StringComparison.OrdinalIgnoreCase) ? "en-US" : "zh-CN";
            Save(settings);
        }

        private static string GetDefaultGameDirectory()
        {
            string defaultPath = @"E:\Game\steam\steamapps\common\SimplePlanes 2";
            return Directory.Exists(defaultPath) ? defaultPath : string.Empty;
        }

        private static ManagerSettings CreateDefaultSettings()
        {
            return new ManagerSettings
            {
                gameDirectory = GetDefaultGameDirectory(),
                language = "zh-CN"
            };
        }

        private static void EnsureDefaults(ManagerSettings settings)
        {
            if (string.IsNullOrEmpty(settings.language))
            {
                settings.language = "zh-CN";
            }
        }
    }
}
