using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Ryuu.Interfaces;
using Ryuu.Models;

namespace Ryuu.Handlers
{
    public static class GuildHandler
    {
        public static Dictionary<ulong, GuildModel> GuildConfigs { get; set; } = new Dictionary<ulong, GuildModel>();

        private const string ConfigPath = "data/GuildConfig.json";

        public static async Task SaveAsync<T>(Dictionary<ulong, T> configs) where T : IServer
            => await File.WriteAllTextAsync(ConfigPath, await Task.Run(() => JsonConvert.SerializeObject(configs, Formatting.Indented)).ConfigureAwait(false));

        public static async Task<Dictionary<ulong, T>> LoadServerConfigsAsync<T>() where T : IServer, new()
        {
            if (File.Exists(ConfigPath))
            {
                return JsonConvert.DeserializeObject<Dictionary<ulong, T>>(await File.ReadAllTextAsync(ConfigPath));
            }
            var newConfig = new Dictionary<ulong, T>();
            await SaveAsync(newConfig);
            return newConfig;
        }
    }
}