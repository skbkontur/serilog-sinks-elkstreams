namespace Serilog.Sinks.ElkStreams
{
    class ElkStreamsApi
    {
        public const string AuthorizationScheme = "ELK";

        public static string NormalizeServerBaseAddress(string serverUrl)
        {
            var baseUri = serverUrl;
            if (!baseUri.EndsWith("/"))
                baseUri += "/";
            return baseUri;
        }
    }
}