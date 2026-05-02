namespace SimplePlanes2ModManager.Models
{
    internal sealed class GameDirectoryState
    {
        public string path { get; set; }
        public bool isConfigured { get; set; }
        public bool isValid { get; set; }
        public bool isGameRunning { get; set; }
        public string message { get; set; }
    }
}
