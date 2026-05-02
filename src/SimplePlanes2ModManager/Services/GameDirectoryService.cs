using SimplePlanes2ModManager.Models;
using System;
using System.Diagnostics;
using System.IO;

namespace SimplePlanes2ModManager.Services
{
    internal sealed class GameDirectoryService
    {
        private readonly ManagerSettingsService _settingsService;

        public GameDirectoryService(ManagerSettingsService settingsService)
        {
            _settingsService = settingsService;
        }

        public string GetGameDirectory()
        {
            ManagerSettings settings = _settingsService.Load();
            return settings != null ? settings.gameDirectory : string.Empty;
        }

        public void SaveGameDirectory(string gameDirectory)
        {
            if (string.IsNullOrEmpty(gameDirectory))
            {
                throw new InvalidOperationException("Game directory is empty.");
            }

            string fullPath = Path.GetFullPath(gameDirectory);
            if (!Directory.Exists(fullPath))
            {
                throw new DirectoryNotFoundException("Game directory does not exist.");
            }

            ManagerSettings settings = _settingsService.Load();
            settings.gameDirectory = fullPath;
            _settingsService.Save(settings);
        }

        public GameDirectoryState GetGameDirectoryState()
        {
            string gameDirectory = GetGameDirectory();
            bool isConfigured = !string.IsNullOrEmpty(gameDirectory);
            bool isValid = IsValidGameDirectory(gameDirectory);
            return new GameDirectoryState
            {
                path = gameDirectory,
                isConfigured = isConfigured,
                isValid = isValid,
                isGameRunning = IsGameRunning(),
                message = GetGameDirectoryMessage(gameDirectory, isConfigured, isValid)
            };
        }

        public string GetInitialDirectoryForDialog()
        {
            string gameDirectory = GetGameDirectory();
            if (!string.IsNullOrEmpty(gameDirectory) && Directory.Exists(gameDirectory))
            {
                return gameDirectory;
            }

            return Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
        }

        public string GetPluginsDirectory()
        {
            return Path.Combine(GetGameDirectoryOrThrow(), "BepInEx", "plugins");
        }

        public string GetConfigDirectory()
        {
            return Path.Combine(GetGameDirectoryOrThrow(), "BepInEx", "config");
        }

        public string GetGameDirectoryOrThrow()
        {
            string gameDirectory = GetGameDirectory();
            if (!IsValidGameDirectory(gameDirectory))
            {
                throw new InvalidOperationException("Select a valid SimplePlanes 2 game directory first.");
            }

            return Path.GetFullPath(gameDirectory);
        }

        public void ThrowIfGameIsRunning()
        {
            if (IsGameRunning())
            {
                throw new InvalidOperationException("SimplePlanes 2 is running. Close the game before changing plugins.");
            }
        }

        public bool IsValidGameDirectory(string gameDirectory)
        {
            if (string.IsNullOrEmpty(gameDirectory))
            {
                return false;
            }

            return File.Exists(Path.Combine(gameDirectory, "SimplePlanes 2.exe"));
        }

        public bool IsGameRunning()
        {
            return IsProcessRunning("SimplePlanes 2") ||
                   IsProcessRunning("SimplePlanes2") ||
                   IsProcessRunning("SimplePlanes");
        }

        private static bool IsProcessRunning(string processName)
        {
            try
            {
                return Process.GetProcessesByName(processName).Length > 0;
            }
            catch
            {
                return false;
            }
        }

        private static string GetGameDirectoryMessage(string gameDirectory, bool isConfigured, bool isValid)
        {
            if (!isConfigured)
            {
                return "Select the SimplePlanes 2 game folder.";
            }

            if (!Directory.Exists(gameDirectory))
            {
                return "Configured directory does not exist.";
            }

            return isValid ? "Game directory is valid." : "SimplePlanes 2.exe was not found in this folder.";
        }
    }
}
