using System;
using System.IO;
using Serilog.Events;
using Serilog.Formatting;
using Serilog.Formatting.Json;

namespace Serilog.Sinks.ElkStreams
{
    /// <summary>
    /// An <see cref="ITextFormatter"/> that writes events in a ElkStreams JSON format.
    /// </summary>
    public class ElkStreamsJsonFormatter : ITextFormatter
    {
        const int DefaultMaxMessageLength = 32 * 1024;
        const int DefaultMaxExceptionLength = 32 * 1024;

        readonly bool _renderMessage;
        readonly JsonValueFormatter _valueFormatter;

        /// <summary>
        /// Construct a <see cref="ElkStreamsJsonFormatter"/>, optionally supplying a formatter for
        /// <see cref="LogEventPropertyValue"/>s on the event.
        /// </summary>
        /// <param name="renderMessage">Whether to render the message in addition to template</param>
        /// <param name="valueFormatter">A value formatter, or null.</param>
        public ElkStreamsJsonFormatter(JsonValueFormatter valueFormatter = null, bool renderMessage = false)
        {
            _valueFormatter = valueFormatter ?? new JsonValueFormatter("$type");
            _renderMessage = renderMessage;
        }

        /// <summary>
        /// Format the log event into the output. Subsequent events will be newline-delimited.
        /// </summary>
        /// <param name="logEvent">The event to format.</param>
        /// <param name="output">The output.</param>
        public void Format(LogEvent logEvent, TextWriter output)
        {
            FormatEvent(logEvent, output, _valueFormatter, _renderMessage);
            output.WriteLine();
        }

        /// <summary>
        /// Format the log event into the output.
        /// </summary>
        /// <param name="logEvent">The event to format.</param>
        /// <param name="output">The output.</param>
        /// <param name="valueFormatter">A value formatter for <see cref="LogEventPropertyValue"/>s on the event.</param>
        /// <param name="renderMessage"></param>
        public static void FormatEvent(LogEvent logEvent, TextWriter output, JsonValueFormatter valueFormatter, bool renderMessage = false)
        {
            if (logEvent == null) throw new ArgumentNullException(nameof(logEvent));
            if (output == null) throw new ArgumentNullException(nameof(output));
            if (valueFormatter == null) throw new ArgumentNullException(nameof(valueFormatter));

            output.Write("{");

            output.WriteProperty("@timestamp", logEvent.Timestamp.ToUniversalTime().ToString("O"), false);
            output.WriteProperty("MessageTemplate", logEvent.MessageTemplate.Text.Truncate(DefaultMaxMessageLength));

            if (renderMessage)
            {
                output.WriteProperty("Message", logEvent.RenderMessage().Truncate(DefaultMaxMessageLength));
            }

            output.WriteProperty("Level", GetLevelMoniker(logEvent.Level));

            if (logEvent.Exception != null)
            {
                output.WriteProperty("Exception", logEvent.Exception.ToString().Truncate(DefaultMaxExceptionLength));
            }

            foreach (var property in logEvent.Properties)
            {
                output.WriteProperty(property, valueFormatter);
            }

            output.Write("}");
        }

        static string GetLevelMoniker(LogEventLevel logEventLevel)
        {
            switch (logEventLevel)
            {
                case LogEventLevel.Verbose:
                    return "Verbose";
                case LogEventLevel.Debug:
                    return "Debug";
                case LogEventLevel.Information:
                    return "Information";
                case LogEventLevel.Warning:
                    return "Warning";
                case LogEventLevel.Error:
                    return "Error";
                case LogEventLevel.Fatal:
                    return "Fatal";
                default:
                    return logEventLevel.ToString();
            }
        }
    }
}