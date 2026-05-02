using SimplePlanes2ModManager.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace SimplePlanes2ModManager.Services
{
    internal sealed class PluginService
    {
        private readonly GameDirectoryService _gameDirectoryService;

        public PluginService(GameDirectoryService gameDirectoryService)
        {
            _gameDirectoryService = gameDirectoryService;
        }

        public PluginInfo[] ListInstalledPlugins()
        {
            string pluginsDirectory;
            if (!TryGetExistingPluginsDirectory(out pluginsDirectory))
            {
                return new PluginInfo[0];
            }

            List<PluginInfo> plugins = new List<PluginInfo>();
            string[] pluginDirectories = Directory.GetDirectories(pluginsDirectory);
            Array.Sort(pluginDirectories, StringComparer.OrdinalIgnoreCase);

            for (int index = 0; index < pluginDirectories.Length; index++)
            {
                PluginInfo plugin = TryReadPluginInfo(pluginDirectories[index]);
                if (plugin != null)
                {
                    plugins.Add(plugin);
                }
            }

            return plugins.ToArray();
        }

        public void InstallPluginZip(string zipPath)
        {
            if (string.IsNullOrEmpty(zipPath) || !File.Exists(zipPath))
            {
                throw new FileNotFoundException("Plugin zip does not exist.");
            }

            _gameDirectoryService.ThrowIfGameIsRunning();
            string gameDirectory = _gameDirectoryService.GetGameDirectoryOrThrow();
            ZipInstallService.ExtractZipToDirectorySafely(zipPath, gameDirectory);
        }

        public void EnablePlugin(string pluginId)
        {
            _gameDirectoryService.ThrowIfGameIsRunning();
            string pluginDirectory = GetPluginDirectoryById(pluginId);
            string[] disabledDlls = Directory.GetFiles(pluginDirectory, "*.dll.disabled", SearchOption.TopDirectoryOnly);
            if (disabledDlls.Length == 0)
            {
                return;
            }

            for (int index = 0; index < disabledDlls.Length; index++)
            {
                string disabledPath = disabledDlls[index];
                string enabledPath = disabledPath.Substring(0, disabledPath.Length - ".disabled".Length);
                if (!File.Exists(enabledPath))
                {
                    File.Move(disabledPath, enabledPath);
                }
            }
        }

        public void DisablePlugin(string pluginId)
        {
            _gameDirectoryService.ThrowIfGameIsRunning();
            string pluginDirectory = GetPluginDirectoryById(pluginId);
            string[] enabledDlls = Directory.GetFiles(pluginDirectory, "*.dll", SearchOption.TopDirectoryOnly);
            for (int index = 0; index < enabledDlls.Length; index++)
            {
                string enabledPath = enabledDlls[index];
                string disabledPath = enabledPath + ".disabled";
                if (!File.Exists(disabledPath))
                {
                    File.Move(enabledPath, disabledPath);
                }
            }
        }

        public void UninstallPlugin(string pluginId)
        {
            _gameDirectoryService.ThrowIfGameIsRunning();
            string pluginDirectory = GetPluginDirectoryById(pluginId);
            Directory.Delete(pluginDirectory, true);
        }

        private bool TryGetExistingPluginsDirectory(out string pluginsDirectory)
        {
            pluginsDirectory = string.Empty;
            string gameDirectory = _gameDirectoryService.GetGameDirectory();
            if (!_gameDirectoryService.IsValidGameDirectory(gameDirectory))
            {
                return false;
            }

            pluginsDirectory = Path.Combine(gameDirectory, "BepInEx", "plugins");
            return Directory.Exists(pluginsDirectory);
        }

        private PluginInfo TryReadPluginInfo(string pluginDirectory)
        {
            string[] enabledDlls = Directory.GetFiles(pluginDirectory, "*.dll", SearchOption.TopDirectoryOnly);
            string[] disabledDlls = Directory.GetFiles(pluginDirectory, "*.dll.disabled", SearchOption.TopDirectoryOnly);
            string entryFile = enabledDlls.Length > 0 ? enabledDlls[0] : (disabledDlls.Length > 0 ? disabledDlls[0] : string.Empty);
            if (string.IsNullOrEmpty(entryFile))
            {
                return null;
            }

            string directoryName = Path.GetFileName(pluginDirectory);
            return new PluginInfo
            {
                id = directoryName,
                name = directoryName,
                directoryName = directoryName,
                directoryPath = pluginDirectory,
                entryFile = Path.GetFileName(entryFile),
                version = GetFileVersion(entryFile),
                isEnabled = enabledDlls.Length > 0
            };
        }

        private string GetPluginDirectoryById(string pluginId)
        {
            if (string.IsNullOrEmpty(pluginId) || pluginId.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
            {
                throw new InvalidOperationException("Invalid plugin id.");
            }

            string pluginsDirectory = _gameDirectoryService.GetPluginsDirectory();
            string pluginDirectory = Path.Combine(pluginsDirectory, pluginId);
            string fullPluginDirectory = Path.GetFullPath(pluginDirectory);
            string fullPluginsDirectory = Path.GetFullPath(pluginsDirectory);

            if (!fullPluginDirectory.StartsWith(fullPluginsDirectory, StringComparison.OrdinalIgnoreCase) ||
                !Directory.Exists(fullPluginDirectory))
            {
                throw new DirectoryNotFoundException("Plugin directory was not found.");
            }

            return fullPluginDirectory;
        }

        private static string GetFileVersion(string filePath)
        {
            try
            {
                FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo(filePath);
                return string.IsNullOrEmpty(versionInfo.FileVersion) ? "--" : versionInfo.FileVersion;
            }
            catch
            {
                return "--";
            }
        }
    }
}
