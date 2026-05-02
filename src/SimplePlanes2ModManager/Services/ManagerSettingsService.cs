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
                    gameDirectory = GetDefaultGameDirectory()
                };
            }

            try
            {
                string json = File.ReadAllText(_settingsPath);
                ManagerSettings settings = _serializer.Deserialize<ManagerSettings>(json);
                if (settings == null)
                {
                    return new ManagerSettings();
                }

                return settings;
            }
            catch
            {
                return new ManagerSettings();
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

        private static string GetDefaultGameDirectory()
        {
            string defaultPath = @"E:\Game\steam\steamapps\common\SimplePlanes 2";
            return Directory.Exists(defaultPath) ? defaultPath : string.Empty;
        }
    }
}
