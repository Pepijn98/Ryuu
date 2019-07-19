using Newtonsoft.Json;
using Ryuu.Interfaces;

namespace Ryuu.Models
{
    public class BlacklistModel : IBlacklist
    {
        [JsonProperty("username")]
        public string Username { get; set; }

        [JsonProperty("discriminator")]
        public string Discriminator { get; set; }

        [JsonProperty("reason")]
        public string Reason { get; set; }
    }
}