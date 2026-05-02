using SimplePlanes2ModManager.Models;
using System;
using System.IO;

namespace SimplePlanes2ModManager.Services
{
    internal sealed class BepInExService
    {
        private readonly GameDirectoryService _gameDirectoryService;

        public BepInExService(GameDirectoryService gameDirectoryService)
        {
            _gameDirectoryService = gameDirectoryService;
        }

        public BepInExStatus GetStatus()
        {
            string gameDirectory = _gameDirectoryService.GetGameDirectory();
            bool hasDoorstop = File.Exists(Path.Combine(gameDirectory ?? string.Empty, "doorstop_config.ini"));
            bool hasProxyDll = File.Exists(Path.Combine(gameDirectory ?? string.Empty, "winhttp.dll"));
            bool hasCoreDll = File.Exists(Path.Combine(gameDirectory ?? string.Empty, "BepInEx", "core", "BepInEx.dll"));
            bool isInstalled = hasDoorstop && hasProxyDll && hasCoreDll;

            return new BepInExStatus
            {
                isInstalled = isInstalled,
                hasDoorstop = hasDoorstop,
                hasProxyDll = hasProxyDll,
                hasCoreDll = hasCoreDll,
                message = isInstalled ? "BepInEx looks installed." : "BepInEx is incomplete or not installed."
            };
        }

        public void InstallBepInExZip(string zipPath)
        {
            if (string.IsNullOrEmpty(zipPath) || !File.Exists(zipPath))
            {
                throw new FileNotFoundException("BepInEx zip does not exist.");
            }

            _gameDirectoryService.ThrowIfGameIsRunning();
            string gameDirectory = _gameDirectoryService.GetGameDirectoryOrThrow();
            ZipInstallService.ExtractZipToDirectorySafely(zipPath, gameDirectory);
        }
    }
}
