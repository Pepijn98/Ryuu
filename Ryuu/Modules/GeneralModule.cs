using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Ryuu.Handlers;
using Ryuu.Utilities;

// ReSharper disable UnusedMember.Global

namespace Ryuu.Modules
{
    [Name("general")]
    public class GeneralModule : InteractiveBase<ShardedCommandContext>
    {
        [Command("help")]
        [Summary("Show all commands or info about a specified command")]
        [RequireBotPermission(ChannelPermission.SendMessages | ChannelPermission.EmbedLinks)]
        public async Task HelpAsync([Remainder, Name("command_name **(optional)**")] string command = null)
        {
            var commands = CommandHandler.Commands;
            
            var config = GuildHandler.GuildConfigs[Context.Guild.Id];
            string prefix;
            switch (ConfigHandler.Config.ReleaseEnv)
            {
                case "dev":
                case "development":
                    prefix = ConfigHandler.Config.DevPrefix;
                    break;
                case "prod":
                case "production":
                    prefix = config.CommandPrefix ?? ConfigHandler.Config.Prefix;
                    break;
                default:
                    Console.WriteLine("ReleaseEnv can only be production or development");
                    Environment.Exit(0);
                    return;
            }
            
            if (command == null)
            {
                var embed = new EmbedBuilder
                {
                    Color = new Color(ConfigHandler.Config.EmbedColor),
                    Description = "These are my commands:"
                };
            
                foreach (var module in commands.Modules)
                {
                    string description = null;
                    foreach (var cmd in module.Commands)
                    {
                        var result = await cmd.CheckPreconditionsAsync(Context);
                        if (result.IsSuccess) description += $"{prefix}{cmd.Aliases.First()}\n";
                    }
                
                    if (!string.IsNullOrWhiteSpace(description))
                    {
                        embed.AddField(module.Name, description, true);
                    }
                }

                await ReplyAsync("", embed: embed.Build());
            }
            else
            {
                var result = commands.Search(Context, command);

                if (!result.IsSuccess)
                {
                    await ReplyAsync($"Sorry, I couldn't find a command with the name **{command}**.");
                    return;
                }

                var embed = new EmbedBuilder
                {
                    Color = new Color(ConfigHandler.Config.EmbedColor)
                };

                foreach (var match in result.Commands)
                {
                    var cmd = match.Command;

                    embed.AddField(string.Join(", ", cmd.Aliases), $"Parameters: {string.Join(", ", cmd.Parameters.Select(p => p.Name))}\n" + 
                                                                   $"Summary: {cmd.Summary}");
                }

                await ReplyAsync("", embed: embed.Build());
            }
        }

        [Command("ping", RunMode = RunMode.Async)]
        [Summary("Show the bots latency")]
        [RequireBotPermission(ChannelPermission.SendMessages)]
        public async Task PingAsync()
        {
            var msg = await Context.Channel.SendMessageAsync($"Pong!\nLatency: **{Context.Client.Latency}** ms.");

            if (msg != null)
            {
                var ping = (msg.Timestamp - Context.Message.Timestamp).TotalMilliseconds;
                await msg.ModifyAsync(m => m.Content = $"Pong!\n\nRest latency: **{ping}** ms\nGateway latency: **{Context.Client.Latency}** ms");
            }
        }

        [Command("date", RunMode = RunMode.Async)]
        [Summary("Get current date")]
        [RequireBotPermission(ChannelPermission.SendMessages)]
        public async Task DateAsync()
        {
            await ReplyAsync(Utils.CurrentDate());
        }
    }
}