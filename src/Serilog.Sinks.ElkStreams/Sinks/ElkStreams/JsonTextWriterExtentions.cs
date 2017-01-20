using System.Collections.Generic;
using System.IO;
using Serilog.Events;
using Serilog.Formatting.Json;

namespace Serilog.Sinks.ElkStreams
{
    static class JsonTextWriterExtentions
    {
        public static void WriteProperty(this TextWriter writer, string key, string value, bool writeComma = true)
        {
            if (writeComma)
            {
                writer.Write(",");
            }

            writer.Write('"');
            writer.Write(key);
            writer.Write('"');
            writer.Write(':');
            JsonValueFormatter.WriteQuotedJsonString(value, writer);
        }

        public static void WriteProperty(this TextWriter writer, KeyValuePair<string, LogEventPropertyValue> property, JsonValueFormatter valueFormatter, bool writeComma = true)
        {
            if (writeComma)
            {
                writer.Write(",");
            }

            writer.Write('"');
            writer.Write(property.Key);
            writer.Write('"');
            writer.Write(':');
            valueFormatter.Format(property.Value, writer);
        }
    }
}