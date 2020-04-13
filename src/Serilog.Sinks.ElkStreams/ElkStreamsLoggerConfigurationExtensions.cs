// Copyright 2014 Serilog Contributors
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using Serilog.Configuration;
using Serilog.Events;
using Serilog.Sinks.ElkStreams;

namespace Serilog
{
    /// <summary>
    /// Adds the WriteTo.ElkStreams() extension method to <see cref="LoggerConfiguration"/>.
    /// </summary>
    public static class ElkStreamsLoggerConfigurationExtensions
    {
        /// <summary>
        /// Adds a sink that writes log events to the ElkStreams server.
        /// </summary>
        /// <param name="loggerConfiguration">The logger configuration.</param>
        /// <param name="indexTemplate">The index name formatter. A string.Format using the DateTimeOffset of the event is run over this string.</param>
        /// <param name="renderMessage">Whether to render the message in addition to template</param>
        /// <param name="restrictedToMinimumLevel">The minimum log event level required in order to write an event to the sink.</param>
        /// <param name="batchPostingLimit">The maximum number of events to post in a single batch.</param>
        /// <param name="period">The time to wait between checking for event batches.</param>
        /// <param name="serverUrl">The base URL of the ElkStreams server that log events will be written to.</param>
        /// <param name="apiKey">A ElkStreams <i>API key</i> that authenticates the client to the ElkStreams server.</param>
        /// <param name="queueSizeLimit">The maximum number of events that will be held in-memory while waiting to ship them to
        /// ElkStreams. Beyond this limit, events will be dropped.</param>
        /// <returns>Logger configuration, allowing configuration to continue.</returns>
        /// <exception cref="ArgumentNullException">A required parameter is null.</exception>
        public static LoggerConfiguration ElkStreams(
            this LoggerSinkConfiguration loggerConfiguration,
            string serverUrl,
            string apiKey,
            string indexTemplate,
            bool renderMessage = false,
            LogEventLevel restrictedToMinimumLevel = LevelAlias.Minimum,
            int batchPostingLimit = ElkStreamsSink.DefaultBatchPostingLimit,
            TimeSpan? period = null,
            int queueSizeLimit = ElkStreamsSink.DefaultQueueSizeLimit)
        {
            if (loggerConfiguration == null) throw new ArgumentNullException(nameof(loggerConfiguration));
            if (serverUrl == null) throw new ArgumentNullException(nameof(serverUrl));
            if (apiKey == null) throw new ArgumentNullException(nameof(apiKey));
            if (indexTemplate == null) throw new ArgumentNullException(nameof(indexTemplate));

            var defaultedPeriod = period ?? ElkStreamsSink.DefaultPeriod;

            var sink = new ElkStreamsSink(
                serverUrl,
                apiKey,
                indexTemplate,
                renderMessage,
                batchPostingLimit,
                defaultedPeriod,
                queueSizeLimit
            );
            return loggerConfiguration.Sink(sink, restrictedToMinimumLevel);
        }
    }
}