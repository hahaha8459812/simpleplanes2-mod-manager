namespace SimplePlanes2ModManager.Models
{
    internal sealed class PluginManifest
    {
        public string id { get; set; }
        public string name { get; set; }
        public string version { get; set; }
        public string description { get; set; }
        public string fileName { get; set; }
        public string entryDll { get; set; }
        public string pluginDirectory { get; set; }
        public string[] configFiles { get; set; }
    }
}
