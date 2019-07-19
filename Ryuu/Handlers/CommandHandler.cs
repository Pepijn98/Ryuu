using System;
using System.Reflection;
using System.Threading.Tasks;
using ColoredConsole;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;

namespace Ryuu.Handlers
{
    public class CommandHandler
    {
        public static CommandService Commands;
        private DiscordShardedClient _client;
        private IServiceProvider _services;
        private ICommandContext _ctx;

        public async Task Install(DiscordShardedClient c)
        {
            _client = c;
            
            var services = new ServiceCollection()
                .AddSingleton<Random>()
                .AddSingleton(_client);
            
            foreach (var sc in _client.Shards)
            {
                services.AddSingleton(new InteractiveService(sc));
            }
            
            _services = services.BuildServiceProvider();
            
            Commands = new CommandService();
            await Commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);

            _client.MessageReceived += HandleCommand;
        }


        private async Task HandleCommand(SocketMessage s)
        {
            if (!(s is SocketUserMessage msg)) return;

            _ctx = new ShardedCommandContext(_client, msg);
            // _ctx = new CommandContext(_client, msg);

            var argPos = 0;

            var config = GuildHandler.GuildConfigs[((SocketGuildChannel) msg.Channel).Guild.Id];
            
            string prefix;
            switch (ConfigHandler.Config.ReleaseEnv)
            {
                case "dev":
                case "development":
                    prefix = ConfigHandler.Config.DevPrefix;
                    break;
                case "prod":
                case "production":
                    prefix = config.CommandPrefix;
                    break;
                default:
                    Console.WriteLine("ReleaseEnv can only be production or development");
                    Environment.Exit(0);
                    return;
            }

            if (msg.HasStringPrefix(prefix, ref argPos) || msg.HasMentionPrefix(_client.CurrentUser, ref argPos))
            {

                var result = await Commands.ExecuteAsync(_ctx, argPos, _services);
                if (!result.IsSuccess)
                {
                    if (result.ErrorReason.Contains("Unknown command"))
                    {
                        return;
                    }

                    await _ctx.Channel.SendMessageAsync($"```diff\n- {result.ErrorReason}\n\n" +
                                                       $"Use: {config.CommandPrefix}help <command_name> for more detail on a certain command```");
                }

                var commandGuild = _ctx.Channel is SocketGuildChannel ? _ctx.Guild.Name : "DM";
                
                ColorConsole.WriteLine($"{DateTime.Now:HH:mm:ss tt} [Cmd] ".Green(),
                                       $"{commandGuild}".Magenta(),
                                       " >> ".White(),
                                       $"{_ctx.Message.Author.Username}#{_ctx.Message.Author.Discriminator}".Green(),
                                       " > ".White(),
                                       $"{_ctx.Message.Content}".Cyan());

                if (BlacklistHandler.BlacklistConfigs.ContainsKey(s.Author.Id))
                {
                    await s.Channel.SendMessageAsync(":no_entry: Sorry, but you may not use this bot!");
                }
            }
        }
    }
}