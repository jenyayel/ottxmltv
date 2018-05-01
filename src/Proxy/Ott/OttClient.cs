using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Proxy.Ott
{
    public class OttClient
    {
        private const string API_ENDPOINT = "http://ott.watch";
        private readonly HttpClient _client;
        private readonly ILogger<OttClient> _logger;

        public OttClient(ILogger<OttClient> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _client = new HttpClient()
            {
                BaseAddress = new Uri(API_ENDPOINT)
            };
            _client.DefaultRequestHeaders.Add("Accept", "application/json");
            _client.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate");
            _client.DefaultRequestHeaders.Add("Connection", "keep-alive");
        }

        public async Task<Channel[]> GetChannels()
        {
            var result = await _client.GetAsync("/api/channel_now");
            if (!result.IsSuccessStatusCode)
                throw new InvalidOperationException($"Failed to get channels {result.StatusCode} {await result.Content.ReadAsStringAsync()}");

            return JsonConvert
                .DeserializeObject<Dictionary<string, Channel>>(Encoding.UTF8.GetString(await result.Content.ReadAsByteArrayAsync()))
                .Select(kv => kv.Value)
                .ToArray();
        }

        public IEnumerable<Programme> GetEpg(Channel[] channels)
        {
            async Task<List<Programme>> getChannelProgrammes(Channel channel)
            {
                var programmes = new List<Programme>();
                var result = await _client.GetAsync($"/api/channel/{channel.ChannelId}");
                if (!result.IsSuccessStatusCode)
                {
                    _logger.LogError($"Failed to get EPG for channel {channel.ChannelId}: {result.StatusCode}");
                    return programmes;
                }

                using (var sr = new StreamReader(await result.Content.ReadAsStreamAsync()))
                using (var reader = new JsonTextReader(sr))
                {
                    var serializer = new JsonSerializer();
                    while (reader.Read())
                    {
                        if (Regex.IsMatch(reader.Path, @"epg_data\[\d+\]"))
                        {
                            var programme = serializer.Deserialize<Programme>(reader);

                            if (programme.StartTime < channel.ProgramStartTime)
                                continue; // programs that started before current program

                            programme.Channel = channel;
                            programmes.Add(programme);

                            if (programmes.Count == 100)
                                break;
                        }
                    }
                }
                return programmes;
            }

            return channels
                .AsParallel()
                .WithDegreeOfParallelism(20)
                .SelectMany(c => getChannelProgrammes(c).ConfigureAwait(false).GetAwaiter().GetResult());
        }
    }
}
