namespace SimplePlanes2ModManager.Models
{
    internal sealed class PluginInstallRecord
    {
        public string id { get; set; }
        public string name { get; set; }
        public string installedVersion { get; set; }
        public string latestVersion { get; set; }
        public string indexUrl { get; set; }
        public string repository { get; set; }
        public string entryDll { get; set; }
        public string pluginDirectory { get; set; }
        public string updateMessage { get; set; }
        public bool updateAvailable { get; set; }
        public bool updateCheckFailed { get; set; }
    }
}
