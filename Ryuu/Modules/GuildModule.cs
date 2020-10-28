using System.Globalization;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using Ryuu.Handlers;

// ReSharper disable UnusedType.Global
// ReSharper disable UnusedMember.Global

namespace Ryuu.Modules
{
    public class GuildModule : InteractiveBase<ShardedCommandContext>
    {
        [Group("guild"), Alias("server")]
        [Summary("Everything guild related")]
        public class GuildGroupModule : ModuleBase
        {
            [Command("settings", RunMode = RunMode.Async)]
            [Summary("Shows all setting on your guild!")]
            [RequireBotPermission(ChannelPermission.SendMessages | ChannelPermission.EmbedLinks)]
            public async Task SettingsAsync()
            {
                var config = GuildHandler.GuildConfigs[Context.Guild.Id];
    
                ulong joinLeave = 0;
                if (config.JoinLeave > 0)
                {
                    joinLeave = config.JoinLeave;
                }
    
                ulong modLog = 0;
                if (config.ModLog > 0)
                {
                    modLog = config.ModLog;
                }
    
                ulong modRole = 0;
                if (config.ModRole > 0)
                {
                    modRole = config.ModRole;
                }
    
                var prefix = config.CommandPrefix ?? $"No specified prefix for this guild. Using default prefix: {ConfigHandler.Config.Prefix}";
    
                var embed = new EmbedBuilder
                {
                    ThumbnailUrl = Context.Guild.IconUrl,
                    Title = $"Guild Setting for {Context.Guild.Name}",
                    Color = new Color(ConfigHandler.Config.EmbedColor)
                };
                embed.AddField("Join/Leave Channel", joinLeave, true);
                embed.AddField("Mod Log Channel", modLog, true);
                embed.AddField("Mod Role ID", modRole, true);
                embed.AddField("Command Prefix", prefix, true);
                
                await ReplyAsync("", embed: embed.Build());
            }

            [Command("prefix", RunMode = RunMode.Async)]
            [Summary("Set your own prefix")]
            [RequireUserPermission(GuildPermission.ManageGuild)]
            [RequireBotPermission(ChannelPermission.SendMessages)]
            public async Task SetPrefixAsync([Name("new_prefix")] string newprefix)
            {
                if (Context.Guild is SocketGuild guild)
                {
                    var config = GuildHandler.GuildConfigs[guild.Id];
                    config.CommandPrefix = newprefix;
                    GuildHandler.GuildConfigs[guild.Id] = config;
                }
                await GuildHandler.SaveAsync(GuildHandler.GuildConfigs);
                
                await ReplyAsync($"Okay, your guild command prefix has been set to **{newprefix}**");
            }
    
            [Command("modlog", RunMode = RunMode.Async)]
            [Summary("Set your mod log channel")]
            [RequireUserPermission(ChannelPermission.ManageChannels)]
            [RequireBotPermission(ChannelPermission.SendMessages)]
            public async Task SetModLogAsync(SocketGuildChannel channel)
            {
                var config = GuildHandler.GuildConfigs[Context.Guild.Id];
                config.ModLog = channel.Id;
                GuildHandler.GuildConfigs[Context.Guild.Id] = config;
                await GuildHandler.SaveAsync(GuildHandler.GuildConfigs);
                
                await ReplyAsync($"Okay, your guild mod log has been set to **{channel.Name}**");
            }
    
            [Command("joinleave", RunMode = RunMode.Async)]
            [Summary("Set your join/leave channel")]
            [RequireUserPermission(ChannelPermission.ManageChannels)]
            [RequireBotPermission(ChannelPermission.SendMessages)]
            public async Task SetJoinLeaveAsync(SocketGuildChannel channel)
            {
                var config = GuildHandler.GuildConfigs[Context.Guild.Id];
                config.JoinLeave = channel.Id;
                GuildHandler.GuildConfigs[Context.Guild.Id] = config;
                await GuildHandler.SaveAsync(GuildHandler.GuildConfigs);
                
                await ReplyAsync($"Okay, your guild mod log has been set to **{channel.Name}**");
            }
    
            [Command("modrole", RunMode = RunMode.Async)]
            [Summary("Set your mod role channel")]
            [RequireUserPermission(ChannelPermission.ManageRoles)]
            [RequireBotPermission(ChannelPermission.SendMessages)]
            public async Task SetModRoleAsync([Name("role_id")] ulong role)
            {
                var config = GuildHandler.GuildConfigs[Context.Guild.Id];
                config.ModRole = role;
                GuildHandler.GuildConfigs[Context.Guild.Id] = config;
                await GuildHandler.SaveAsync(GuildHandler.GuildConfigs);
                
                await ReplyAsync($"Okay, your guild Mod Role has been set to **{role}**");
            }
            
            [Command("info", RunMode = RunMode.Async)]
            [Summary("Show info about the guild")]
            [RequireBotPermission(ChannelPermission.SendMessages | ChannelPermission.EmbedLinks)]
            public async Task GuildInfoAsync()
            {
                const string format = "dddd, dd-MM-yyyy HH:mm:ss z";
                if (Context.Guild is SocketGuild guild)
                {
                    var name = guild.Name;
                    var serverid = guild.Id;
                    var created = guild.CreatedAt.ToString(format, CultureInfo.InvariantCulture);
                    var owner = $"{guild.Owner.Username}#{guild.Owner.Discriminator}";
                    var ownerid = guild.OwnerId;
                    var users = guild.Users.Count;
                    var roles = guild.Roles.Count;
                    var channels = guild.Channels.Count + guild.VoiceChannels.Count;
                    var verification = guild.VerificationLevel;
                    var timeout = guild.AFKTimeout;
                    var afkchannel = guild.AFKChannel;

                    created = created.Replace("+0", "UTC");
                    
                    var embed = new EmbedBuilder
                    {
                        ThumbnailUrl = guild.IconUrl,
                        Color = new Color(ConfigHandler.Config.EmbedColor),
                        Title = $"Server Information ({name})",
                        Description = $"ID: **{serverid}**"
                    };
                    embed.AddField("Users Count", $"{users}", true);
                    embed.AddField("Channels Count", $"{channels}", true);
                    embed.AddField("Roles Count", $"{roles}", true);
                    embed.AddField("Verification Level", $"{verification}", true);
                    embed.AddField("AFK Channel", $"{afkchannel}", true);
                    embed.AddField("AFK Timeout", $"{timeout}", true);
                    embed.AddField("Created At", $"{created}", true);
                    embed.AddField("Owner", $"{owner} ({ownerid})", true);

                    await ReplyAsync("", embed: embed.Build());
                }
            }
            
            // TODO: Add a guild icon command
        }
    }
}