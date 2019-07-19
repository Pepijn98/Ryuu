using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Ryuu.Interfaces;
using Ryuu.Models;

namespace Ryuu.Handlers
{
    public static class BlacklistHandler
    {
        public static Dictionary<ulong, BlacklistModel> BlacklistConfigs { get; set; } = new Dictionary<ulong, BlacklistModel>();

        private const string ConfigPath = "data/Blacklist.json";

        private static async Task SaveAsync<T>(Dictionary<ulong, T> configs) where T : IBlacklist
            => File.WriteAllText(ConfigPath, await Task.Run(() => JsonConvert.SerializeObject(configs, Formatting.Indented)).ConfigureAwait(false));

        public static async Task<Dictionary<ulong, T>> LoadBlacklistAsync<T>() where T : IBlacklist, new()
        {
            if (File.Exists(ConfigPath))
            {
                return JsonConvert.DeserializeObject<Dictionary<ulong, T>>(File.ReadAllText(ConfigPath));
            }

            var newConfig = new Dictionary<ulong, T>();
            await SaveAsync(newConfig);
            return newConfig;
        }
    }
}