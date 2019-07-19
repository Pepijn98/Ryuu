using System.Threading.Tasks;
using Discord;
using Discord.Webhook;
using Discord.WebSocket;
using Ryuu.Handlers;
using Ryuu.Models;
using Ryuu.Utilities;

namespace Ryuu.Controllers
{
    public static class Events
    {
        private static readonly ConfigModel Config = ConfigHandler.Config;
        
        internal static async Task UserJoinedAsync(SocketGuildUser user)
        {
            var config = GuildHandler.GuildConfigs[user.Guild.Id];

            var joinChannel = user.Guild.GetChannel(config.JoinLeave);

            var account = user.IsBot ? "Bot" : "User";

            if (joinChannel != null)
            {
                var channel = joinChannel as ITextChannel;
                var embed = new EmbedBuilder
                {
                    Color = Color.Green,
                    ThumbnailUrl = user.GetAvatarUrl(),
                    Description = "**Member Joined!**"
                };
                embed.AddField("Name", user.Username, true);
                embed.AddField("Discriminator", user.Discriminator, true);
                embed.AddField("ID", user.Id, true);
                embed.AddField("Account Type", account, true);
                if (channel != null) await channel.SendMessageAsync("", embed: embed.Build());
            }
        }

        internal static async Task UserLeftAsync(SocketGuildUser user)
        {
            var config = GuildHandler.GuildConfigs[user.Guild.Id];

            var leaveChannel = user.Guild.GetChannel(config.JoinLeave);

            var account = user.IsBot ? "Bot" : "User";

            if (leaveChannel != null)
            {
                var channel = leaveChannel as ITextChannel;
                var embed = new EmbedBuilder
                {
                    Color = Color.Red,
                    ThumbnailUrl = user.GetAvatarUrl(),
                    Description = "**Member Left!**"
                };
                embed.AddField("Name", user.Username, true);
                embed.AddField("Discriminator", user.Discriminator, true);
                embed.AddField("ID", user.Id, true);
                embed.AddField("Account Type", account, true);
                if (channel != null) await channel.SendMessageAsync("", embed: embed.Build());
            }
        }

        internal static async Task LeftGuildAsync(SocketGuild guild)
        {
            await Utils.DeleteGuildConfigAsync(guild);
            
            var webhookClient = new DiscordWebhookClient(Config.JoinLeaveId, Config.JoinLeaveToken);
                
            var webhookEmbed = new EmbedBuilder
            {
                Color = Color.Red,
                ThumbnailUrl = guild.IconUrl,
                Title = "Left guild:",
                Description = $"**{guild.Name} - ({guild.Id})**"
            };
            webhookEmbed.AddField("Members:", guild.MemberCount, true);
            webhookEmbed.AddField("Owner:", $"{guild.Owner.Username} - ({guild.OwnerId})", true);
                
            var webhookEmbedArray = new Embed[1];
            webhookEmbedArray[0] = webhookEmbed.Build();
                
            await webhookClient.SendMessageAsync("", false, webhookEmbedArray, guild.CurrentUser.Username, guild.CurrentUser.GetAvatarUrl(ImageFormat.Auto, 256));
        }

        internal static async Task HandleGuildConfigAsync(SocketGuild guild)
        {
            var createConfig = new GuildModel();
            if (!GuildHandler.GuildConfigs.ContainsKey(guild.Id))
            {
                GuildHandler.GuildConfigs.Add(guild.Id, createConfig);
            }
            await GuildHandler.SaveAsync(GuildHandler.GuildConfigs);
        }

        internal static async Task JoinedGuildAsync(SocketGuild guild)
        {
            var embed = new EmbedBuilder
            {
                Color = new Color(Config.EmbedColor),
                ThumbnailUrl = guild.IconUrl,
                Title = "Hello there!",
                Description = "Thanks for inviting me to your server!\n" +
                              $"The default prefix currently is **{Config.Prefix}**\n" +
                              $"Use `{Config.Prefix}prefix <new_prefix>` to change the guilds prefix\n" +
                              "To have spaces in the prefix wrap it in quotation marks (\"\")\n" +
                              $"For more commands you can use {Config.Prefix}help"
            }.Build();
            await guild.DefaultChannel.SendMessageAsync("", embed: embed);

            await Utils.CreateGuildConfigAsync(guild);
            
            var webhookClient = new DiscordWebhookClient(Config.JoinLeaveId, Config.JoinLeaveToken);
                
            var webhookEmbed = new EmbedBuilder
            {
                Color = new Color(Config.EmbedColor),
                ThumbnailUrl = guild.IconUrl,
                Title = "Joined guild:",
                Description = $"**{guild.Name} - ({guild.Id})**"
            };
            webhookEmbed.AddField("Members:", guild.MemberCount, true);
            webhookEmbed.AddField("Owner:", $"{guild.Owner.Username} - ({guild.OwnerId})", true);
                
            var webhookEmbedArray = new Embed[1];
            webhookEmbedArray[0] = webhookEmbed.Build();
                
            await webhookClient.SendMessageAsync("", false, webhookEmbedArray, guild.CurrentUser.Username, guild.CurrentUser.GetAvatarUrl(ImageFormat.Auto, 256));
        }

        internal static async Task BannedUserAsync(SocketUser user, SocketGuild guild)
        {
            var config = GuildHandler.GuildConfigs[guild.Id];

            var logChannel = guild.GetChannel(config.ModLog);
            if (logChannel != null)
            {
                var channel = logChannel as ITextChannel;
                var embed = new EmbedBuilder
                {
                    ThumbnailUrl = user.GetAvatarUrl(),
                    Color = new Color(Config.EmbedColor)
                };
                embed.AddField("Name", user.Username, true);
                embed.AddField("Type", "Ban", true);
                embed.AddField("ID", user.Id, true);
                embed.AddField("Discriminator", user.Discriminator, true);
                if (channel != null) await channel.SendMessageAsync("", embed: embed.Build());
            }
        }
    }
}