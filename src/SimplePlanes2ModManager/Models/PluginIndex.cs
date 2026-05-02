namespace SimplePlanes2ModManager.Models
{
    internal sealed class PluginIndex
    {
        public string id { get; set; }
        public string name { get; set; }
        public string version { get; set; }
        public string description { get; set; }
        public string fileName { get; set; }
        public string downloadUrl { get; set; }
        public string sha256 { get; set; }
        public string repository { get; set; }
        public string entryDll { get; set; }
    }
}
