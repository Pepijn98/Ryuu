using Newtonsoft.Json;
using Ryuu.Interfaces;

namespace Ryuu.Models
{
    public class CleverbotModel : ICleverbot
    {
        [JsonProperty("botsay")]
        public string Botsay { get; set; }
    }
}