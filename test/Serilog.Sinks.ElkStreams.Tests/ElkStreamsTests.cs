using Xunit;

namespace Serilog.Sinks.ElkStreams.Tests
{
    public class ElkStreamsTests
    {
        [Fact(Skip = "Explicit")]
        public void Send()
        {
            const string serverUrl = "elkStreamsServer";
            const string apiKey = "apiKey";
            const string indexTemplate = "indexTemplate";

            var logger = new LoggerConfiguration()
                .WriteTo.ElkStreams(serverUrl, apiKey, indexTemplate)
                .Enrich.WithProperty("Facility", "ElkStreamsTests")
                .CreateLogger();

            logger.Debug("Hello, {World}!", "world");
            logger.Information("Hello, {World}!", "world");
            logger.Warning("Hello, {World}!", "world");
            logger.Error("Hello, {World}!", "world");
            logger.Fatal("Hello, {World}!", "world");
        }
    }
}
