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
        private readonly JavaScriptSerializer _serializer = new JavaScriptSerializer();

        public RemotePluginService(PluginService pluginService)
        {
            _pluginService = pluginService;
        }

        public void InstallFromGit(string repositoryOrIndexUrl)
        {
            if (string.IsNullOrEmpty(repositoryOrIndexUrl))
            {
                throw new InvalidOperationException("Repository URL is empty.");
            }

            PluginIndex pluginIndex = ReadPluginIndex(repositoryOrIndexUrl.Trim());
            ValidatePluginIndex(pluginIndex);

            string packagePath = DownloadPackage(pluginIndex);
            try
            {
                _pluginService.InstallPluginZip(packagePath);
            }
            finally
            {
                TryDeleteFile(packagePath);
            }
        }

        private PluginIndex ReadPluginIndex(string repositoryOrIndexUrl)
        {
            string indexJson = DownloadIndexJson(repositoryOrIndexUrl);
            PluginIndex pluginIndex = _serializer.Deserialize<PluginIndex>(indexJson);
            if (pluginIndex == null)
            {
                throw new InvalidOperationException("index.json is empty or invalid.");
            }

            return pluginIndex;
        }

        private string DownloadIndexJson(string repositoryOrIndexUrl)
        {
            if (repositoryOrIndexUrl.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
            {
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
                return DownloadText(rawMainUrl);
            }
            catch
            {
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
