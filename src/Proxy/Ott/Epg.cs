using Newtonsoft.Json;

namespace Proxy.Ott
{
    public class Epg
    {
        [JsonProperty("epg_data")]
        public Programme[] EpgData { get; set; }
    }
}
