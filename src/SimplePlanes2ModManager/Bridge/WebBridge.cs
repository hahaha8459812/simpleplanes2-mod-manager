using SimplePlanes2ModManager.Models;
using SimplePlanes2ModManager.Services;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Web.Script.Serialization;
using System.Windows.Forms;

namespace SimplePlanes2ModManager.Bridge
{
    [ComVisible(true)]
    [ClassInterface(ClassInterfaceType.AutoDispatch)]
    public sealed class WebBridge
    {
        private readonly Form _owner;
        private readonly ManagerSettingsService _settingsService;
        private readonly GameDirectoryService _gameDirectoryService;
        private readonly BepInExService _bepInExService;
        private readonly PluginService _pluginService;
        private readonly JavaScriptSerializer _serializer;

        internal WebBridge(
            Form owner,
            ManagerSettingsService settingsService,
            GameDirectoryService gameDirectoryService,
            BepInExService bepInExService,
            PluginService pluginService)
        {
            _owner = owner;
            _settingsService = settingsService;
            _gameDirectoryService = gameDirectoryService;
            _bepInExService = bepInExService;
            _pluginService = pluginService;
            _serializer = new JavaScriptSerializer();
        }

        public string GetState()
        {
            return ToJson(BridgeResponse.Success(new
            {
                settings = _settingsService.Load(),
                game = _gameDirectoryService.GetGameDirectoryState(),
                bepinex = _bepInExService.GetStatus(),
                plugins = _pluginService.ListInstalledPlugins()
            }));
        }

        public string BrowseGameDirectory()
        {
            using (FolderBrowserDialog dialog = new FolderBrowserDialog())
            {
                dialog.Description = "Select the SimplePlanes 2 game folder";
                dialog.SelectedPath = _gameDirectoryService.GetInitialDirectoryForDialog();
                dialog.ShowNewFolderButton = false;

                if (dialog.ShowDialog(_owner) != DialogResult.OK)
                {
                    return ToJson(BridgeResponse.Cancelled("Selection cancelled."));
                }

                return SetGameDirectory(dialog.SelectedPath);
            }
        }

        public string SetGameDirectory(string gameDirectory)
        {
            try
            {
                _gameDirectoryService.SaveGameDirectory(gameDirectory);
                return GetState();
            }
            catch (Exception exception)
            {
                return ToJson(BridgeResponse.Failure(exception.Message));
            }
        }

        public string SetLanguage(string language)
        {
            try
            {
                _settingsService.SaveLanguage(language);
                return GetState();
            }
            catch (Exception exception)
            {
                return ToJson(BridgeResponse.Failure(exception.Message));
            }
        }

        public string SelectAndInstallPluginZip()
        {
            return SelectZipAndInstall("Select plugin zip", delegate(string zipPath)
            {
                _pluginService.InstallPluginZip(zipPath);
            });
        }

        public string SelectAndInstallBepInExZip()
        {
            return SelectZipAndInstall("Select BepInEx 5 Mono x64 zip", delegate(string zipPath)
            {
                _bepInExService.InstallBepInExZip(zipPath);
            });
        }

        public string InstallBundledBepInEx()
        {
            try
            {
                _bepInExService.InstallBundledBepInEx();
                return GetState();
            }
            catch (Exception exception)
            {
                return ToJson(BridgeResponse.Failure(exception.Message));
            }
        }

        public string EnablePlugin(string pluginId)
        {
            return RunPluginMutation(delegate { _pluginService.EnablePlugin(pluginId); });
        }

        public string DisablePlugin(string pluginId)
        {
            return RunPluginMutation(delegate { _pluginService.DisablePlugin(pluginId); });
        }

        public string UninstallPlugin(string pluginId)
        {
            DialogResult result = MessageBox.Show(
                _owner,
                "Uninstall this plugin directory? User config under BepInEx\\config will be kept.",
                "Confirm uninstall",
                MessageBoxButtons.OKCancel,
                MessageBoxIcon.Warning);

            if (result != DialogResult.OK)
            {
                return ToJson(BridgeResponse.Cancelled("Uninstall cancelled."));
            }

            return RunPluginMutation(delegate { _pluginService.UninstallPlugin(pluginId); });
        }

        public string OpenGameDirectory()
        {
            return OpenDirectory(_gameDirectoryService.GetGameDirectory());
        }

        public string OpenPluginsDirectory()
        {
            return OpenDirectory(_gameDirectoryService.GetPluginsDirectory());
        }

        public string OpenConfigDirectory()
        {
            return OpenDirectory(_gameDirectoryService.GetConfigDirectory());
        }

        private string SelectZipAndInstall(string title, Action<string> installAction)
        {
            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.Title = title;
                dialog.Filter = "Zip packages (*.zip)|*.zip|All files (*.*)|*.*";
                dialog.CheckFileExists = true;
                dialog.Multiselect = false;

                if (dialog.ShowDialog(_owner) != DialogResult.OK)
                {
                    return ToJson(BridgeResponse.Cancelled("Selection cancelled."));
                }

                try
                {
                    installAction(dialog.FileName);
                    return GetState();
                }
                catch (Exception exception)
                {
                    return ToJson(BridgeResponse.Failure(exception.Message));
                }
            }
        }

        private string RunPluginMutation(Action action)
        {
            try
            {
                action();
                return GetState();
            }
            catch (Exception exception)
            {
                return ToJson(BridgeResponse.Failure(exception.Message));
            }
        }

        private string OpenDirectory(string directoryPath)
        {
            try
            {
                if (string.IsNullOrEmpty(directoryPath) || !Directory.Exists(directoryPath))
                {
                    return ToJson(BridgeResponse.Failure("Directory does not exist."));
                }

                Process.Start("explorer.exe", "\"" + directoryPath + "\"");
                return ToJson(BridgeResponse.Success("Opened directory."));
            }
            catch (Exception exception)
            {
                return ToJson(BridgeResponse.Failure(exception.Message));
            }
        }

        private string ToJson(object value)
        {
            return _serializer.Serialize(value);
        }
    }
}
