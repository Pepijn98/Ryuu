using System.Globalization;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using Ryuu.Handlers;

// ReSharper disable UnusedMember.Global

namespace Ryuu.Modules
{
    public class UserModule : InteractiveBase<ShardedCommandContext>
    {
        [Group("user")]
        [Summary("Everything user related")]
        [RequireBotPermission(ChannelPermission.SendMessages | ChannelPermission.EmbedLinks)]
        public class UserGroupModule : ModuleBase
        {
            [Command("blacklisted", RunMode = RunMode.Async), Alias("checkbl")]
            [Summary("Shows the blacklisted users")]
            [RequireOwner]
            [RequireBotPermission(ChannelPermission.SendMessages | ChannelPermission.EmbedLinks)]
            public async Task ShowBlacklistAsync(SocketUser user)
            {
                var embed = new EmbedBuilder
                {
                    ThumbnailUrl = user.GetAvatarUrl(),
                    Description = "**Is that user blacklisted?**"
                };
                embed.AddField("Name", user.Username + "#" + user.Discriminator, true);
                embed.AddField("ID", user.Id, true);

                if (BlacklistHandler.BlacklistConfigs.ContainsKey(user.Id))
                {
                    embed.Color = Color.Green;
                    embed.AddField("Status", "Yes", true);
                }
                else
                {
                    embed.Color = Color.Red;
                    embed.AddField("Status", "No", true);
                }

                await ReplyAsync("", embed: embed.Build());
            }
        
            [Command("info", RunMode = RunMode.Async)]
            [Summary("Returns info about the current user, or the user parameter, if one passed")]
            [RequireBotPermission(ChannelPermission.SendMessages | ChannelPermission.EmbedLinks)]
            public async Task UserInfoAsync(SocketUser user = null)
            {
                var u = user ?? Context.User;

                const string format = "dddd, dd-MM-yyyy HH:mm:ss z";
                var tag = $"{u.Username}#{u.Discriminator}";
                var id = $"{u.Id}";
                var avatar = u.GetAvatarUrl(ImageFormat.Png, 2048);
                var createdAt = u.CreatedAt.ToString(format, CultureInfo.InvariantCulture);

                var embed = new EmbedBuilder
                {
                    ThumbnailUrl = avatar,
                    Color = new Color(ConfigHandler.Config.EmbedColor),
                    Title = $"User info of {tag}",
                    Description = $"ID: **{id}**"
                };
                embed.AddField("Created On", createdAt, true);

                await ReplyAsync("", embed: embed.Build());
            }
            
            // TODO: Add a user avatar command
        }
    }
}