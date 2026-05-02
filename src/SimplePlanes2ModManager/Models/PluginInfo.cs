namespace SimplePlanes2ModManager.Models
{
    internal sealed class PluginInfo
    {
        public string id { get; set; }
        public string name { get; set; }
        public string directoryName { get; set; }
        public string directoryPath { get; set; }
        public string entryFile { get; set; }
        public string version { get; set; }
        public bool isEnabled { get; set; }
    }
}
