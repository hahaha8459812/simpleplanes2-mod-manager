using SimplePlanes2ModManager.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Web.Script.Serialization;

namespace SimplePlanes2ModManager.Services
{
    internal sealed class PluginService
    {
        private static readonly string[] PackageMetadataFiles =
        {
            "mod.json",
            "README.md",
            "README.en.md"
        };

        private readonly GameDirectoryService _gameDirectoryService;
        private readonly PluginInstallRecordService _recordService;
        private readonly JavaScriptSerializer _serializer = new JavaScriptSerializer();

        public PluginService(GameDirectoryService gameDirectoryService, PluginInstallRecordService recordService)
        {
            _gameDirectoryService = gameDirectoryService;
            _recordService = recordService;
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

        public PluginManifest InstallPluginZip(string zipPath)
        {
            return InstallPluginZip(zipPath, null);
        }

        public PluginManifest InstallPluginZip(string zipPath, PluginIndex expectedPluginIndex)
        {
            if (string.IsNullOrEmpty(zipPath) || !File.Exists(zipPath))
            {
                throw new FileNotFoundException("Plugin zip does not exist.");
            }

            _gameDirectoryService.ThrowIfGameIsRunning();
            string gameDirectory = _gameDirectoryService.GetGameDirectoryOrThrow();
            string packageFileName = expectedPluginIndex == null ? Path.GetFileName(zipPath) : expectedPluginIndex.fileName;
            PluginManifest manifest = ValidatePluginPackage(zipPath, packageFileName);
            if (expectedPluginIndex != null)
            {
                ValidatePluginIndexMatchesManifest(expectedPluginIndex, manifest);
            }

            ValidatePluginPackageWriteScope(zipPath, manifest);
            ZipInstallService.ExtractZipToDirectorySafely(zipPath, gameDirectory, PackageMetadataFiles, GetProtectedConfigFiles(manifest));
            return manifest;
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

        public bool TryGetInstalledPluginVersion(string packageEntryDll, out string pluginDirectory, out string installedVersion)
        {
            pluginDirectory = string.Empty;
            installedVersion = string.Empty;

            string pluginDirectoryName = GetPluginDirectoryNameFromEntryDll(packageEntryDll);
            if (string.IsNullOrEmpty(pluginDirectoryName))
            {
                return false;
            }

            string pluginsDirectory;
            if (!TryGetExistingPluginsDirectory(out pluginsDirectory))
            {
                return false;
            }

            string candidatePluginDirectory = Path.Combine(pluginsDirectory, pluginDirectoryName);
            if (!Directory.Exists(candidatePluginDirectory))
            {
                return false;
            }

            string entryDllName = Path.GetFileName(packageEntryDll);
            string entryDllPath = Path.Combine(candidatePluginDirectory, entryDllName);
            string disabledEntryDllPath = entryDllPath + ".disabled";
            if (File.Exists(entryDllPath))
            {
                pluginDirectory = candidatePluginDirectory;
                installedVersion = GetFileVersion(entryDllPath);
                return true;
            }

            if (File.Exists(disabledEntryDllPath))
            {
                pluginDirectory = candidatePluginDirectory;
                installedVersion = GetFileVersion(disabledEntryDllPath);
                return true;
            }

            return false;
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

        private PluginManifest ValidatePluginPackage(string zipPath, string packageFileName)
        {
            using (ZipArchive archive = ZipFile.OpenRead(zipPath))
            {
                ZipArchiveEntry manifestEntry = archive.GetEntry("mod.json");
                if (manifestEntry == null)
                {
                    throw new InvalidOperationException("Plugin zip must contain mod.json at the package root.");
                }

                PluginManifest manifest = ReadPluginManifest(manifestEntry);
                ValidatePluginManifest(manifest);
                ValidatePackageFileName(manifest, packageFileName);
                ValidateManifestPaths(manifest);
                ValidatePackageEntries(archive, manifest);
                return manifest;
            }
        }

        private PluginManifest ReadPluginManifest(ZipArchiveEntry manifestEntry)
        {
            using (Stream stream = manifestEntry.Open())
            using (StreamReader reader = new StreamReader(stream))
            {
                PluginManifest manifest = _serializer.Deserialize<PluginManifest>(reader.ReadToEnd());
                if (manifest == null)
                {
                    throw new InvalidOperationException("mod.json is empty or invalid.");
                }

                return manifest;
            }
        }

        private static void ValidatePluginManifest(PluginManifest manifest)
        {
            if (string.IsNullOrEmpty(manifest.id))
            {
                throw new InvalidOperationException("mod.json is missing id.");
            }

            if (string.IsNullOrEmpty(manifest.name))
            {
                throw new InvalidOperationException("mod.json is missing name.");
            }

            if (string.IsNullOrEmpty(manifest.version))
            {
                throw new InvalidOperationException("mod.json is missing version.");
            }

            if (string.IsNullOrEmpty(manifest.description))
            {
                throw new InvalidOperationException("mod.json is missing description.");
            }

            if (string.IsNullOrEmpty(manifest.fileName))
            {
                throw new InvalidOperationException("mod.json is missing fileName.");
            }

            if (string.IsNullOrEmpty(manifest.entryDll))
            {
                throw new InvalidOperationException("mod.json is missing entryDll.");
            }
        }

        private static void ValidateManifestPaths(PluginManifest manifest)
        {
            if (!IsSafeRelativePath(manifest.entryDll) ||
                !manifest.entryDll.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("mod.json entryDll must be a relative dll path.");
            }

            string normalizedEntryDll = NormalizePackagePath(manifest.entryDll);
            if (!normalizedEntryDll.StartsWith("BepInEx/plugins/", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("mod.json entryDll must be under BepInEx/plugins.");
            }

            if (!string.IsNullOrEmpty(manifest.pluginDirectory) &&
                !IsSafeRelativePath(manifest.pluginDirectory))
            {
                throw new InvalidOperationException("mod.json pluginDirectory must be a safe relative path.");
            }

            if (!string.IsNullOrEmpty(manifest.pluginDirectory))
            {
                string normalizedPluginDirectory = NormalizePackagePath(manifest.pluginDirectory).TrimEnd('/');
                if (!normalizedPluginDirectory.StartsWith("BepInEx/plugins/", StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidOperationException("mod.json pluginDirectory must be under BepInEx/plugins.");
                }

                if (!normalizedEntryDll.StartsWith(normalizedPluginDirectory + "/", StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidOperationException("mod.json entryDll must be inside pluginDirectory.");
                }
            }

            if (manifest.configFiles == null)
            {
                return;
            }

            for (int index = 0; index < manifest.configFiles.Length; index++)
            {
                string configFile = manifest.configFiles[index];
                if (!IsSafeRelativePath(configFile))
                {
                    throw new InvalidOperationException("mod.json configFiles contains an unsafe path.");
                }

                if (!IsDeclaredConfigPathAllowed(manifest, configFile))
                {
                    throw new InvalidOperationException("mod.json configFiles must be under BepInEx/config or pluginDirectory.");
                }
            }
        }

        private static bool IsDeclaredConfigPathAllowed(PluginManifest manifest, string configFile)
        {
            string normalizedConfigFile = NormalizePackagePath(configFile);
            if (normalizedConfigFile.StartsWith("BepInEx/config/", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            if (string.IsNullOrEmpty(manifest.pluginDirectory))
            {
                return false;
            }

            string normalizedPluginDirectory = NormalizePackagePath(manifest.pluginDirectory).TrimEnd('/');
            return normalizedConfigFile.StartsWith(normalizedPluginDirectory + "/", StringComparison.OrdinalIgnoreCase);
        }

        private static string[] GetProtectedConfigFiles(PluginManifest manifest)
        {
            if (manifest.configFiles == null || manifest.configFiles.Length == 0)
            {
                return new string[0];
            }

            List<string> protectedConfigFiles = new List<string>();
            for (int index = 0; index < manifest.configFiles.Length; index++)
            {
                string configFile = NormalizePackagePath(manifest.configFiles[index]);
                if (!string.IsNullOrEmpty(configFile))
                {
                    protectedConfigFiles.Add(configFile);
                }
            }

            return protectedConfigFiles.ToArray();
        }

        private static void ValidatePackageFileName(PluginManifest manifest, string packageFileName)
        {
            string manifestFileName = Path.GetFileName(manifest.fileName);
            if (string.IsNullOrEmpty(manifestFileName) ||
                !manifestFileName.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("mod.json fileName must be a zip file name.");
            }

            if (!string.IsNullOrEmpty(packageFileName) &&
                !string.Equals(manifestFileName, Path.GetFileName(packageFileName), StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("mod.json fileName must match the plugin zip file name.");
            }
        }

        private static void ValidatePackageEntries(ZipArchive archive, PluginManifest manifest)
        {
            bool hasEntryDll = false;
            string normalizedEntryDll = NormalizePackagePath(manifest.entryDll);
            string normalizedPluginDirectory = GetNormalizedPluginDirectory(manifest);

            foreach (ZipArchiveEntry entry in archive.Entries)
            {
                string normalizedEntryName = NormalizePackagePath(entry.FullName);
                if (string.IsNullOrEmpty(normalizedEntryName))
                {
                    continue;
                }

                if (IsForbiddenPluginPackageEntry(normalizedEntryName))
                {
                    throw new InvalidOperationException("Plugin zip must not include BepInEx core files or user config: " + entry.FullName);
                }

                if (!IsAllowedPluginPackageEntry(normalizedEntryName, normalizedPluginDirectory))
                {
                    throw new InvalidOperationException("Plugin zip contains a file outside its plugin directory: " + entry.FullName);
                }

                if (string.Equals(normalizedEntryName, normalizedEntryDll, StringComparison.OrdinalIgnoreCase))
                {
                    hasEntryDll = true;
                }
            }

            if (!hasEntryDll)
            {
                throw new InvalidOperationException("mod.json entryDll was not found in the plugin zip.");
            }
        }

        private static void ValidatePluginPackageWriteScope(string zipPath, PluginManifest manifest)
        {
            using (ZipArchive archive = ZipFile.OpenRead(zipPath))
            {
                string normalizedPluginDirectory = GetNormalizedPluginDirectory(manifest);
                foreach (ZipArchiveEntry entry in archive.Entries)
                {
                    string normalizedEntryName = NormalizePackagePath(entry.FullName);
                    if (string.IsNullOrEmpty(normalizedEntryName) || IsPackageDirectoryEntry(normalizedEntryName))
                    {
                        continue;
                    }

                    if (!IsAllowedPluginPackageEntry(normalizedEntryName, normalizedPluginDirectory))
                    {
                        throw new InvalidOperationException("Plugin zip may only install files under " + normalizedPluginDirectory + ": " + entry.FullName);
                    }
                }
            }
        }

        private static bool IsAllowedPluginPackageEntry(string normalizedEntryName, string normalizedPluginDirectory)
        {
            if (IsPackageDirectoryEntry(normalizedEntryName))
            {
                return true;
            }

            if (IsPackageMetadataFile(normalizedEntryName))
            {
                return true;
            }

            return IsPackagePathOrChild(normalizedEntryName, normalizedPluginDirectory);
        }

        private static bool IsPackageDirectoryEntry(string normalizedEntryName)
        {
            return normalizedEntryName.EndsWith("/", StringComparison.Ordinal);
        }

        private static bool IsPackageMetadataFile(string normalizedEntryName)
        {
            for (int index = 0; index < PackageMetadataFiles.Length; index++)
            {
                if (string.Equals(normalizedEntryName, PackageMetadataFiles[index], StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        private static string GetNormalizedPluginDirectory(PluginManifest manifest)
        {
            if (!string.IsNullOrEmpty(manifest.pluginDirectory))
            {
                return NormalizePackagePath(manifest.pluginDirectory).TrimEnd('/');
            }

            string normalizedEntryDll = NormalizePackagePath(manifest.entryDll);
            int separatorIndex = normalizedEntryDll.LastIndexOf('/');
            if (separatorIndex <= 0)
            {
                throw new InvalidOperationException("mod.json entryDll must be inside a plugin directory.");
            }

            return normalizedEntryDll.Substring(0, separatorIndex);
        }

        private static bool IsForbiddenPluginPackageEntry(string normalizedEntryName)
        {
            string entryName = normalizedEntryName.TrimEnd('/');
            if (string.Equals(entryName, "winhttp.dll", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(entryName, "doorstop_config.ini", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(entryName, ".doorstop_version", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(entryName, "changelog.txt", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            return IsPackagePathOrChild(entryName, "BepInEx/core") ||
                   IsPackagePathOrChild(entryName, "BepInEx/patchers") ||
                   IsPackagePathOrChild(entryName, "BepInEx/config") ||
                   string.Equals(entryName, "BepInEx/LogOutput.log", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsPackagePathOrChild(string normalizedEntryName, string parentPath)
        {
            return string.Equals(normalizedEntryName, parentPath, StringComparison.OrdinalIgnoreCase) ||
                   normalizedEntryName.StartsWith(parentPath + "/", StringComparison.OrdinalIgnoreCase);
        }

        private static void ValidatePluginIndexMatchesManifest(PluginIndex pluginIndex, PluginManifest manifest)
        {
            if (!string.Equals(pluginIndex.id, manifest.id, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("index.json id does not match mod.json id.");
            }

            if (!string.Equals(pluginIndex.version, manifest.version, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("index.json version does not match mod.json version.");
            }

            if (!string.Equals(pluginIndex.fileName, manifest.fileName, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("index.json fileName does not match mod.json fileName.");
            }

            if (!string.IsNullOrEmpty(pluginIndex.entryDll) &&
                !string.Equals(NormalizePackagePath(pluginIndex.entryDll), NormalizePackagePath(manifest.entryDll), StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("index.json entryDll does not match mod.json entryDll.");
            }
        }

        private static bool IsSafeRelativePath(string relativePath)
        {
            if (string.IsNullOrEmpty(relativePath))
            {
                return false;
            }

            string normalizedPath = relativePath.Replace('/', Path.DirectorySeparatorChar);
            if (Path.IsPathRooted(normalizedPath))
            {
                return false;
            }

            string[] segments = normalizedPath.Split(Path.DirectorySeparatorChar);
            for (int index = 0; index < segments.Length; index++)
            {
                if (segments[index] == "..")
                {
                    return false;
                }
            }

            return true;
        }

        private static string NormalizePackagePath(string relativePath)
        {
            return (relativePath ?? string.Empty).Replace('\\', '/').TrimStart('/');
        }

        private static string GetPluginDirectoryNameFromEntryDll(string packageEntryDll)
        {
            string normalizedEntryDll = NormalizePackagePath(packageEntryDll);
            string pluginsPrefix = "BepInEx/plugins/";
            if (!normalizedEntryDll.StartsWith(pluginsPrefix, StringComparison.OrdinalIgnoreCase))
            {
                return string.Empty;
            }

            string pluginRelativePath = normalizedEntryDll.Substring(pluginsPrefix.Length);
            int separatorIndex = pluginRelativePath.IndexOf('/');
            return separatorIndex > 0 ? pluginRelativePath.Substring(0, separatorIndex) : string.Empty;
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
            PluginInstallRecord record = _recordService.FindRecordForPluginDirectory(directoryName);
            string installedVersion = record != null && !string.IsNullOrEmpty(record.installedVersion)
                ? record.installedVersion
                : GetFileVersion(entryFile);

            return new PluginInfo
            {
                id = directoryName,
                name = record != null && !string.IsNullOrEmpty(record.name) ? record.name : directoryName,
                directoryName = directoryName,
                directoryPath = pluginDirectory,
                entryFile = Path.GetFileName(entryFile),
                version = installedVersion,
                installedVersion = installedVersion,
                latestVersion = record != null ? record.latestVersion : string.Empty,
                indexUrl = record != null ? record.indexUrl : string.Empty,
                repository = record != null ? record.repository : string.Empty,
                updateMessage = record != null ? record.updateMessage : string.Empty,
                hasInstallRecord = record != null,
                canCheckUpdates = record != null && !string.IsNullOrEmpty(record.indexUrl),
                updateAvailable = record != null && record.updateAvailable,
                updateCheckFailed = record != null && record.updateCheckFailed,
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
            string fullPluginsDirectory = EnsureTrailingSeparator(Path.GetFullPath(pluginsDirectory));

            if (!EnsureTrailingSeparator(fullPluginDirectory).StartsWith(fullPluginsDirectory, StringComparison.OrdinalIgnoreCase) ||
                !Directory.Exists(fullPluginDirectory))
            {
                throw new DirectoryNotFoundException("Plugin directory was not found.");
            }

            return fullPluginDirectory;
        }

        private static string EnsureTrailingSeparator(string directoryPath)
        {
            if (directoryPath.EndsWith(Path.DirectorySeparatorChar.ToString(), StringComparison.Ordinal))
            {
                return directoryPath;
            }

            return directoryPath + Path.DirectorySeparatorChar;
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
