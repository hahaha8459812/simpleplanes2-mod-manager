namespace SimplePlanes2ModManager.Models
{
    internal sealed class BridgeResponse
    {
        public bool ok { get; set; }
        public bool cancelled { get; set; }
        public string message { get; set; }
        public object data { get; set; }

        public static BridgeResponse Success(object data)
        {
            return new BridgeResponse
            {
                ok = true,
                cancelled = false,
                message = string.Empty,
                data = data
            };
        }

        public static BridgeResponse Failure(string message)
        {
            return new BridgeResponse
            {
                ok = false,
                cancelled = false,
                message = string.IsNullOrEmpty(message) ? "Operation failed." : message,
                data = null
            };
        }

        public static BridgeResponse Cancelled(string message)
        {
            return new BridgeResponse
            {
                ok = false,
                cancelled = true,
                message = string.IsNullOrEmpty(message) ? "Cancelled." : message,
                data = null
            };
        }
    }
}
