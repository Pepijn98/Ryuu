using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Discord.WebSocket;
using Newtonsoft.Json;
using Ryuu.Handlers;
using Ryuu.Models;

namespace Ryuu.Utilities
{
    public static class Utils
    {
        internal const string UserAgent = "Ryuu/v0.0.1 - (https://github.com/KurozeroPB/Ryuu)";
        
        public static string CurrentDate()
        {
            return DateTime.Now.ToLongDateString();
        }

        public static async Task DeleteGuildConfigAsync(SocketGuild guild)
        {
            if (GuildHandler.GuildConfigs.ContainsKey(guild.Id))
            {
                GuildHandler.GuildConfigs.Remove(guild.Id);
            }
            await GuildHandler.SaveAsync(GuildHandler.GuildConfigs);
        }

        public static async Task CreateGuildConfigAsync(SocketGuild guild)
        {
            var createConfig = new GuildModel();
            if (!GuildHandler.GuildConfigs.ContainsKey(guild.Id))
            {
                GuildHandler.GuildConfigs.Add(guild.Id, createConfig);
            }
            await GuildHandler.SaveAsync(GuildHandler.GuildConfigs);
        }

        // Request cleverbot response
        public static async Task<string> Cleverbot(string query, string convoId)
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Add("User-Agent", UserAgent);

            var stringTask = client.GetStringAsync($"http://api.program-o.com/v2/chatbot/?bot_id=12&say={query}&convo_id={convoId}&format=json");
            var response = await stringTask;
            
            var cleverbot = JsonConvert.DeserializeObject<CleverbotModel>(response);

            return cleverbot.Botsay;
        }
        
        // Request catgirl
        public static async Task<string> Catgirl(bool nsfw)
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Add("User-Agent", UserAgent);

            var json = await client.GetStringAsync(nsfw ? "https://nekos.moe/api/v1/random/image?count=1&nsfw=true" : "https://nekos.moe/api/v1/random/image?count=1&nsfw=false");

            CatgirlModel catgirl;
            try
            {
                catgirl = JsonConvert.DeserializeObject<CatgirlModel>(json);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

            return catgirl.Images[0].Id;
        }
    }
}
