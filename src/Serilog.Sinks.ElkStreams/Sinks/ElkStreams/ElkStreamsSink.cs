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
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Serilog.Debugging;
using Serilog.Events;
using Serilog.Sinks.PeriodicBatching;

namespace Serilog.Sinks.ElkStreams
{
    /// <summary>
    /// Writes log events to the ElkStreams.
    /// </summary>
    public class ElkStreamsSink : PeriodicBatchingSink
    {
        const int EventSize = 32 * 1024;
        public const int DefaultBatchPostingLimit = 512;
        public const int DefaultQueueSizeLimit = 65536;
        public static readonly TimeSpan DefaultPeriod = TimeSpan.FromSeconds(1);

        readonly HttpClient _httpClient;
        readonly string _indexTemplate;
        readonly ElkStreamsJsonFormatter _formatter;
        readonly MemoryStream _eventsStream;
        readonly StreamWriter _eventsWriter;

        /// <summary>
        /// The sink that writes log events to the ElkStreams server.
        /// </summary>
        /// <param name="serverUrl">The base URL of the ElkStreams server that log events will be written to.</param>
        /// <param name="apiKey">A ElkStreams <i>API key</i> that authenticates the client to the ElkStreams server.</param>
        /// <param name="indexTemplate">The index name formatter. A string.Format using the DateTimeOffset of the event is run over this string.</param>
        /// <param name="renderMessage">Whether to render the message in addition to template</param>
        /// <param name="batchSizeLimit">The maximum number of events to post in a single batch.</param>
        /// <param name="period">The time to wait between checking for event batches.</param>
        /// <param name="queueSizeLimit">Maximum number of events in the queue.</param>
        public ElkStreamsSink(
            string serverUrl,
            string apiKey,
            string indexTemplate,
            bool renderMessage,
            int batchSizeLimit,
            TimeSpan period,
            int queueSizeLimit)
            : base(batchSizeLimit, period, queueSizeLimit)
        {
            _indexTemplate = indexTemplate;
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri(ElkStreamsApi.NormalizeServerBaseAddress(serverUrl));
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(ElkStreamsApi.AuthorizationScheme, apiKey);
            _formatter = new ElkStreamsJsonFormatter(renderMessage: renderMessage);
            _eventsStream = new MemoryStream();
            _eventsWriter = new StreamWriter(_eventsStream, new UTF8Encoding(false, true), EventSize);
        }

        /// <summary>
        /// The sink that writes log events to the ElkStreams server.
        /// </summary>
        /// <param name="serverUrl">The base URL of the ElkStreams server that log events will be written to.</param>
        /// <param name="apiKey">A ElkStreams <i>API key</i> that authenticates the client to the ElkStreams server.</param>
        /// <param name="indexTemplate">The index name formatter. A string.Format using the DateTimeOffset of the event is run over this string.</param>
        /// <param name="renderMessage">Whether to render the message in addition to template</param>
        /// <param name="batchSizeLimit">The maximum number of events to post in a single batch.</param>
        /// <param name="period">The time to wait between checking for event batches.</param>
        public ElkStreamsSink(
            string serverUrl,
            string apiKey,
            string indexTemplate,
            bool renderMessage,
            int batchSizeLimit,
            TimeSpan period)
            : base(batchSizeLimit, period)
        {
            _indexTemplate = indexTemplate;
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri(ElkStreamsApi.NormalizeServerBaseAddress(serverUrl));
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(ElkStreamsApi.AuthorizationScheme, apiKey);
            _formatter = new ElkStreamsJsonFormatter(renderMessage: renderMessage);
            _eventsStream = new MemoryStream();
            _eventsWriter = new StreamWriter(_eventsStream, new UTF8Encoding(false, true), EventSize);
        }

        /// <summary>
        /// Emit a batch of log events, running asynchronously.
        /// </summary>
        /// <param name="events">The events to emit.</param>
        /// <remarks>Override either <see cref="PeriodicBatchingSink.EmitBatch"/> or <see cref="PeriodicBatchingSink.EmitBatchAsync"/>,
        /// not both.</remarks>
        protected override async Task EmitBatchAsync(IEnumerable<LogEvent> events)
        {
            var uploadEventRequest = BuildRequest(events);
            var uploadEventResponse = await _httpClient.SendAsync(uploadEventRequest).ConfigureAwait(false);
            uploadEventResponse.EnsureSuccessStatusCode();
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
            {
                _httpClient.Dispose();
            }
        }

        HttpRequestMessage BuildRequest(IEnumerable<LogEvent> logEvents)
        {
            _eventsStream.Position = 0;

            foreach (var logEvent in logEvents)
            {
                var position = _eventsStream.Position;
                try
                {
                    try
                    {
                        _formatter.Format(logEvent, _eventsWriter);
                    }
                    finally
                    {
                        _eventsWriter.Flush();
                    }
                }
                catch (Exception exception)
                {
                    _eventsStream.Position = position;

                    LogNonFormattableEvent(logEvent, exception);
                }
            }

            return new HttpRequestMessage(HttpMethod.Post, GetUploadPath())
            {
                Content = new ByteArrayContent(_eventsStream.GetBuffer(), 0, (int)_eventsStream.Position)
            };
        }

        string GetUploadPath()
        {
            return $"logs/{_indexTemplate}-{DateTime.Now.ToString("yyyy.MM.dd")}";
        }

        static void LogNonFormattableEvent(LogEvent logEvent, Exception ex)
        {
            SelfLog.WriteLine(
                "Event at {0} with message template {1} could not be formatted into JSON for ElkStreams and will be dropped: {2}",
                logEvent.Timestamp.ToString("o"), logEvent.MessageTemplate.Text, ex);
        }
    }
}