using System;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Ryuu.Controllers;
using Ryuu.Handlers;
using Ryuu.Models;

namespace Ryuu
{
    public static class Program
    {
        private static DiscordShardedClient _client;
        private static CommandHandler _command;
        private static DiscordSocketConfig _config;

        public const string PlayingStatus = "@{username} help | <prefix> help";

        public static async Task Main()
        {
            _config = new DiscordSocketConfig
            {
                LogLevel = LogSeverity.Info,
                AlwaysDownloadUsers = true,
                MessageCacheSize = 250,
                TotalShards = 1
            };
            _client = new DiscordShardedClient(_config);
            _command = new CommandHandler();
            
            ConfigHandler.DirectoryCheck();
            ConfigHandler.Config = await ConfigHandler.LoadConfigAsync();
            GuildHandler.GuildConfigs = await GuildHandler.LoadServerConfigsAsync<GuildModel>();
            BlacklistHandler.BlacklistConfigs = await BlacklistHandler.LoadBlacklistAsync<BlacklistModel>();

            string token;
            switch (ConfigHandler.Config.ReleaseEnv)
            {
                case "dev":
                case "development":
                    token = ConfigHandler.Config.DevToken;
                    break;
                case "prod":
                case "production":
                    token = ConfigHandler.Config.Token;
                    break;
                default:
                    Console.WriteLine("ReleaseEnv can only be production or development");
                    Environment.Exit(0);
                    return;
            }
            
            await _client.LoginAsync(TokenType.Bot, token);
            await _client.StartAsync();
            
            foreach( var shard in _client.Shards )
            {
                shard.Ready += async () => await shard.SetGameAsync(PlayingStatus.Replace("{username}", shard.CurrentUser.Username));
            }
            
            _client.UserJoined += Events.UserJoinedAsync;
            _client.UserLeft += Events.UserLeftAsync;
            _client.GuildAvailable += Events.HandleGuildConfigAsync;
            _client.LeftGuild += async guild => await Events.LeftGuildAsync(guild);
            _client.JoinedGuild += async guild => await Events.JoinedGuildAsync(guild);
            _client.UserBanned += Events.BannedUserAsync;

            await _command.Install(_client);

            _client.Log += Log;

            /*var cki = Console.ReadKey(true);

            if (cki.Key == ConsoleKey.Escape || cki.Key == ConsoleKey.X)
            {
                await _client.SetGameAsync("");
                await _client.SetStatusAsync(UserStatus.Invisible);
                
                await _client.LogoutAsync();
                await _client.StopAsync();

                Environment.Exit(0);
            }*/

            await Task.Delay(-1);
        }
        
        private static Task Log(LogMessage message)
        {
            switch (message.Severity)
            {
                case LogSeverity.Critical:
                case LogSeverity.Error:
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;
                case LogSeverity.Warning:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    break;
                case LogSeverity.Info:
                    Console.ForegroundColor = ConsoleColor.Green;
                    break;
                case LogSeverity.Verbose:
                case LogSeverity.Debug:
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    break;
                default:
                    Console.ForegroundColor = ConsoleColor.White;
                    break;
            }
            Console.WriteLine($"{DateTime.Now:HH:mm:ss tt} [{message.Severity}] {message.Source}: {message.Message} {message.Exception}");
            Console.ResetColor();

            return Task.CompletedTask;
        }
    }
}