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
        readonly bool _removeGuidsFromExceptions;
        readonly JsonValueFormatter _valueFormatter;

        /// <summary>
        /// Construct a <see cref="ElkStreamsJsonFormatter"/>, optionally supplying a formatter for
        /// <see cref="LogEventPropertyValue"/>s on the event.
        /// </summary>
        /// <param name="renderMessage">Whether to render the message in addition to template</param>
        /// <param name="valueFormatter">A value formatter, or null.</param>
        /// <param name="removeGuidsFromExceptions">Whether to remove GUIDs from exception's message and stacktrace</param>
        public ElkStreamsJsonFormatter(JsonValueFormatter valueFormatter = null, bool renderMessage = false, bool removeGuidsFromExceptions = false)
        {
            _valueFormatter = valueFormatter ?? new JsonValueFormatter("$type");
            _renderMessage = renderMessage;
            _removeGuidsFromExceptions = removeGuidsFromExceptions;
        }

        /// <summary>
        /// Format the log event into the output. Subsequent events will be newline-delimited.
        /// </summary>
        /// <param name="logEvent">The event to format.</param>
        /// <param name="output">The output.</param>
        public void Format(LogEvent logEvent, TextWriter output)
        {
            FormatEvent(logEvent, output, _valueFormatter, _renderMessage, _removeGuidsFromExceptions);
            output.WriteLine();
        }

        /// <summary>
        /// Format the log event into the output.
        /// </summary>
        /// <param name="logEvent">The event to format.</param>
        /// <param name="output">The output.</param>
        /// <param name="valueFormatter">A value formatter for <see cref="LogEventPropertyValue"/>s on the event.</param>
        /// <param name="renderMessage"></param>
        /// <param name="removeGuidsFromExceptions"></param>
        public static void FormatEvent(LogEvent logEvent, TextWriter output, JsonValueFormatter valueFormatter, bool renderMessage = false, bool removeGuidsFromExceptions = false)
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

            output.WriteProperty("Level", logEvent.Level.ToString());

            if (logEvent.Exception != null)
            {
                var exceptionString = removeGuidsFromExceptions
                                          ? logEvent.Exception.ToString().RemoveGuids()
                                          : logEvent.Exception.ToString();
                output.WriteProperty("Exception", exceptionString.Truncate(DefaultMaxExceptionLength));
            }

            foreach (var property in logEvent.Properties)
            {
                output.WriteProperty(property, valueFormatter);
            }

            output.Write("}");
        }
    }
}