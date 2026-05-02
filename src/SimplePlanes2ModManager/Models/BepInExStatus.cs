namespace SimplePlanes2ModManager.Models
{
    internal sealed class BepInExStatus
    {
        public bool isInstalled { get; set; }
        public bool hasDoorstop { get; set; }
        public bool hasProxyDll { get; set; }
        public bool hasCoreDll { get; set; }
        public string message { get; set; }
    }
}
