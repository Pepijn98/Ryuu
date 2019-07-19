using Newtonsoft.Json;
using Ryuu.Handlers;
using Ryuu.Interfaces;

namespace Ryuu.Models
{
    public class GuildModel : IServer
    {
        [JsonProperty("JoinLeave")]
        public ulong JoinLeave { get; set; }

        [JsonProperty("ModLog")]
        public ulong ModLog { get; set; }

        [JsonProperty("ModRole")]
        public ulong ModRole { get; set; }

        [JsonProperty("CommandPrefix")]
        public string CommandPrefix { get; set; } = ConfigHandler.Config.Prefix;
    }
}