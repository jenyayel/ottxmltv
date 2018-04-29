using Newtonsoft.Json;

namespace Proxy.Ott
{
    public class Channel
    {
        [JsonProperty("ch_id")]
        public string ChannelId { get; set; }

        [JsonProperty("channel_name")]
        public string ChannelName { get; set; }

        [JsonProperty("img")]
        public string ChannelImage { get; set; }

        [JsonProperty("time")]
        public int ProgramStartTime { get; set; }
    }
}
