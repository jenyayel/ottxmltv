using Newtonsoft.Json;

namespace Proxy.Ott
{
    public class Programme
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("time")]
        public int StartTime { get; set; }

        [JsonProperty("time_to")]
        public int EndTime { get; set; }

        [JsonProperty("descr")]
        public string Description { get; set; }

        public Channel Channel { get; set; }
    }
}
