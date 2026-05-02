using SimplePlanes2ModManager.Models;
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Web.Script.Serialization;

namespace SimplePlanes2ModManager.Services
{
    internal sealed class RemotePluginService
    {
        private readonly PluginService _pluginService;
        private readonly PluginInstallRecordService _recordService;
        private readonly JavaScriptSerializer _serializer = new JavaScriptSerializer();

        public RemotePluginService(PluginService pluginService, PluginInstallRecordService recordService)
        {
            _pluginService = pluginService;
            _recordService = recordService;
        }

        public void InstallFromGit(string repositoryOrIndexUrl)
        {
            if (string.IsNullOrEmpty(repositoryOrIndexUrl))
            {
                throw new InvalidOperationException("Repository URL is empty.");
            }

            string resolvedIndexUrl;
            PluginIndex pluginIndex = ReadPluginIndex(repositoryOrIndexUrl.Trim(), out resolvedIndexUrl);
            ValidatePluginIndex(pluginIndex);

            string packagePath = DownloadPackage(pluginIndex);
            try
            {
                PluginManifest manifest = _pluginService.InstallPluginZip(packagePath, pluginIndex);
                _recordService.SaveRecord(CreateInstallRecord(pluginIndex, manifest, resolvedIndexUrl));
            }
            finally
            {
                TryDeleteFile(packagePath);
            }
        }

        public void CheckForUpdates()
        {
            PluginInstallRecord[] records = _recordService.LoadRecords();
            for (int index = 0; index < records.Length; index++)
            {
                PluginInstallRecord record = records[index];
                if (record == null || string.IsNullOrEmpty(record.indexUrl))
                {
                    continue;
                }

                try
                {
                    string resolvedIndexUrl;
                    PluginIndex pluginIndex = ReadPluginIndex(record.indexUrl, out resolvedIndexUrl);
                    ValidatePluginIndex(pluginIndex);
                    ValidatePluginIndexMatchesRecord(pluginIndex, record);
                    record.latestVersion = pluginIndex.version;
                    record.updateAvailable = IsRemoteVersionNewer(record.installedVersion, pluginIndex.version);
                    record.updateCheckFailed = false;
                    record.updateMessage = record.updateAvailable
                        ? "Update available: " + record.installedVersion + " -> " + pluginIndex.version
                        : "Already up to date.";
                }
                catch (Exception exception)
                {
                    record.updateAvailable = false;
                    record.updateCheckFailed = true;
                    record.updateMessage = exception.Message;
                }
            }

            _recordService.SaveRecords(records);
        }

        public void RegisterInstalledPluginSource(string repositoryOrIndexUrl)
        {
            if (string.IsNullOrEmpty(repositoryOrIndexUrl))
            {
                throw new InvalidOperationException("Repository URL is empty.");
            }

            string resolvedIndexUrl;
            PluginIndex pluginIndex = ReadPluginIndex(repositoryOrIndexUrl.Trim(), out resolvedIndexUrl);
            ValidatePluginIndex(pluginIndex);

            string pluginDirectory;
            string installedVersion;
            if (!_pluginService.TryGetInstalledPluginVersion(pluginIndex.entryDll, out pluginDirectory, out installedVersion))
            {
                throw new InvalidOperationException("The plugin from this index.json is not installed.");
            }

            _recordService.SaveRecord(CreateInstallRecordFromInstalledPlugin(pluginIndex, resolvedIndexUrl, installedVersion));
        }

        public void UpdatePlugin(string pluginId)
        {
            PluginInstallRecord record = FindRecordById(pluginId);
            if (record == null || string.IsNullOrEmpty(record.indexUrl))
            {
                throw new InvalidOperationException("Plugin has no recorded update source.");
            }

            string resolvedIndexUrl;
            PluginIndex pluginIndex = ReadPluginIndex(record.indexUrl, out resolvedIndexUrl);
            ValidatePluginIndex(pluginIndex);
            ValidatePluginIndexMatchesRecord(pluginIndex, record);

            string packagePath = DownloadPackage(pluginIndex);
            try
            {
                PluginManifest manifest = _pluginService.InstallPluginZip(packagePath, pluginIndex);
                _recordService.SaveRecord(CreateInstallRecord(pluginIndex, manifest, resolvedIndexUrl));
            }
            finally
            {
                TryDeleteFile(packagePath);
            }
        }

        private PluginIndex ReadPluginIndex(string repositoryOrIndexUrl, out string resolvedIndexUrl)
        {
            string indexJson = DownloadIndexJson(repositoryOrIndexUrl, out resolvedIndexUrl);
            PluginIndex pluginIndex = _serializer.Deserialize<PluginIndex>(indexJson);
            if (pluginIndex == null)
            {
                throw new InvalidOperationException("index.json is empty or invalid.");
            }

            return pluginIndex;
        }

        private string DownloadIndexJson(string repositoryOrIndexUrl, out string resolvedIndexUrl)
        {
            if (repositoryOrIndexUrl.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
            {
                resolvedIndexUrl = repositoryOrIndexUrl;
                return DownloadText(repositoryOrIndexUrl);
            }

            string rawMainUrl;
            string rawMasterUrl;
            if (!TryBuildGitHubRawIndexUrls(repositoryOrIndexUrl, out rawMainUrl, out rawMasterUrl))
            {
                throw new InvalidOperationException("Only GitHub repository URLs or direct index.json URLs are supported.");
            }

            try
            {
                resolvedIndexUrl = rawMainUrl;
                return DownloadText(rawMainUrl);
            }
            catch
            {
                resolvedIndexUrl = rawMasterUrl;
                return DownloadText(rawMasterUrl);
            }
        }

        private static bool TryBuildGitHubRawIndexUrls(string repositoryUrl, out string rawMainUrl, out string rawMasterUrl)
        {
            rawMainUrl = string.Empty;
            rawMasterUrl = string.Empty;

            Uri uri;
            if (!Uri.TryCreate(repositoryUrl, UriKind.Absolute, out uri))
            {
                return false;
            }

            if (!string.Equals(uri.Host, "github.com", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            string[] segments = uri.AbsolutePath.Trim('/').Split('/');
            if (segments.Length < 2)
            {
                return false;
            }

            string owner = segments[0];
            string repo = segments[1];
            if (repo.EndsWith(".git", StringComparison.OrdinalIgnoreCase))
            {
                repo = repo.Substring(0, repo.Length - 4);
            }

            rawMainUrl = "https://raw.githubusercontent.com/" + owner + "/" + repo + "/main/index.json";
            rawMasterUrl = "https://raw.githubusercontent.com/" + owner + "/" + repo + "/master/index.json";
            return true;
        }

        private static void ValidatePluginIndex(PluginIndex pluginIndex)
        {
            if (string.IsNullOrEmpty(pluginIndex.id))
            {
                throw new InvalidOperationException("index.json is missing id.");
            }

            if (string.IsNullOrEmpty(pluginIndex.name))
            {
                throw new InvalidOperationException("index.json is missing name.");
            }

            if (string.IsNullOrEmpty(pluginIndex.version))
            {
                throw new InvalidOperationException("index.json is missing version.");
            }

            if (string.IsNullOrEmpty(pluginIndex.fileName))
            {
                throw new InvalidOperationException("index.json is missing fileName.");
            }

            if (string.IsNullOrEmpty(pluginIndex.downloadUrl))
            {
                throw new InvalidOperationException("index.json is missing downloadUrl.");
            }

            Uri downloadUri;
            if (!Uri.TryCreate(pluginIndex.downloadUrl, UriKind.Absolute, out downloadUri) ||
                (downloadUri.Scheme != Uri.UriSchemeHttp && downloadUri.Scheme != Uri.UriSchemeHttps))
            {
                throw new InvalidOperationException("index.json downloadUrl must be an http or https URL.");
            }
        }

        private static string DownloadText(string url)
        {
            using (WebClient client = CreateWebClient())
            {
                byte[] bytes = client.DownloadData(url);
                return Encoding.UTF8.GetString(bytes);
            }
        }

        private static string DownloadPackage(PluginIndex pluginIndex)
        {
            string safeFileName = Path.GetFileName(pluginIndex.fileName);
            if (string.IsNullOrEmpty(safeFileName) || !safeFileName.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("index.json fileName must be a zip file name.");
            }

            string tempDirectory = Path.Combine(Path.GetTempPath(), "SimplePlanes2ModManager");
            Directory.CreateDirectory(tempDirectory);
            string packagePath = Path.Combine(tempDirectory, Guid.NewGuid().ToString("N") + "-" + safeFileName);

            using (WebClient client = CreateWebClient())
            {
                client.DownloadFile(pluginIndex.downloadUrl, packagePath);
            }

            return packagePath;
        }

        private PluginInstallRecord FindRecordById(string pluginId)
        {
            PluginInstallRecord[] records = _recordService.LoadRecords();
            for (int index = 0; index < records.Length; index++)
            {
                PluginInstallRecord record = records[index];
                if (record == null)
                {
                    continue;
                }

                if (string.Equals(record.id, pluginId, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(Path.GetFileName((record.pluginDirectory ?? string.Empty).Replace('\\', '/').TrimEnd('/')), pluginId, StringComparison.OrdinalIgnoreCase))
                {
                    return record;
                }
            }

            return null;
        }

        private static PluginInstallRecord CreateInstallRecord(PluginIndex pluginIndex, PluginManifest manifest, string resolvedIndexUrl)
        {
            return new PluginInstallRecord
            {
                id = manifest.id,
                name = manifest.name,
                installedVersion = manifest.version,
                latestVersion = pluginIndex.version,
                indexUrl = resolvedIndexUrl,
                repository = string.IsNullOrEmpty(pluginIndex.repository) ? string.Empty : pluginIndex.repository,
                entryDll = manifest.entryDll,
                pluginDirectory = GetPluginDirectory(manifest),
                updateAvailable = false,
                updateCheckFailed = false,
                updateMessage = "Installed."
            };
        }

        private static PluginInstallRecord CreateInstallRecordFromInstalledPlugin(PluginIndex pluginIndex, string resolvedIndexUrl, string installedVersion)
        {
            bool updateAvailable = IsRemoteVersionNewer(installedVersion, pluginIndex.version);
            return new PluginInstallRecord
            {
                id = pluginIndex.id,
                name = pluginIndex.name,
                installedVersion = installedVersion,
                latestVersion = pluginIndex.version,
                indexUrl = resolvedIndexUrl,
                repository = string.IsNullOrEmpty(pluginIndex.repository) ? string.Empty : pluginIndex.repository,
                entryDll = pluginIndex.entryDll,
                pluginDirectory = GetPluginDirectory(pluginIndex.entryDll),
                updateAvailable = updateAvailable,
                updateCheckFailed = false,
                updateMessage = updateAvailable
                    ? "Update available: " + installedVersion + " -> " + pluginIndex.version
                    : "Already up to date."
            };
        }

        private static string GetPluginDirectory(PluginManifest manifest)
        {
            if (!string.IsNullOrEmpty(manifest.pluginDirectory))
            {
                return manifest.pluginDirectory;
            }

            return GetPluginDirectory(manifest.entryDll);
        }

        private static string GetPluginDirectory(string entryDll)
        {
            string normalizedEntryDll = (entryDll ?? string.Empty).Replace('\\', '/');
            int separatorIndex = normalizedEntryDll.LastIndexOf('/');
            return separatorIndex > 0 ? normalizedEntryDll.Substring(0, separatorIndex) : string.Empty;
        }

        private static void ValidatePluginIndexMatchesRecord(PluginIndex pluginIndex, PluginInstallRecord record)
        {
            if (!string.Equals(pluginIndex.id, record.id, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("index.json id does not match the installed plugin record.");
            }
        }

        private static bool IsRemoteVersionNewer(string installedVersion, string remoteVersion)
        {
            int comparisonResult;
            if (!TryCompareVersions(installedVersion, remoteVersion, out comparisonResult))
            {
                return !string.Equals(installedVersion, remoteVersion, StringComparison.OrdinalIgnoreCase);
            }

            return comparisonResult < 0;
        }

        private static bool TryCompareVersions(string leftVersion, string rightVersion, out int comparisonResult)
        {
            comparisonResult = 0;
            int[] leftParts;
            int[] rightParts;
            if (!TryParseVersion(leftVersion, out leftParts) || !TryParseVersion(rightVersion, out rightParts))
            {
                return false;
            }

            int partCount = Math.Max(leftParts.Length, rightParts.Length);
            for (int index = 0; index < partCount; index++)
            {
                int leftPart = index < leftParts.Length ? leftParts[index] : 0;
                int rightPart = index < rightParts.Length ? rightParts[index] : 0;
                if (leftPart == rightPart)
                {
                    continue;
                }

                comparisonResult = leftPart < rightPart ? -1 : 1;
                return true;
            }

            comparisonResult = 0;
            return true;
        }

        private static bool TryParseVersion(string version, out int[] parts)
        {
            parts = new int[0];
            if (string.IsNullOrEmpty(version))
            {
                return false;
            }

            string normalizedVersion = version.Trim();
            if (normalizedVersion.StartsWith("v", StringComparison.OrdinalIgnoreCase))
            {
                normalizedVersion = normalizedVersion.Substring(1);
            }

            string[] textParts = normalizedVersion.Split('.');
            if (textParts.Length == 0)
            {
                return false;
            }

            int[] parsedParts = new int[textParts.Length];
            for (int index = 0; index < textParts.Length; index++)
            {
                if (!int.TryParse(textParts[index], out parsedParts[index]))
                {
                    return false;
                }
            }

            parts = parsedParts;
            return true;
        }

        private static WebClient CreateWebClient()
        {
            WebClient client = new WebClient();
            client.Headers[HttpRequestHeader.UserAgent] = "SimplePlanes2ModManager";
            return client;
        }

        private static void TryDeleteFile(string filePath)
        {
            try
            {
                if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
            }
            catch
            {
            }
        }
    }
}
