using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using Ryuu.Utilities;

// ReSharper disable UnusedType.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global

namespace Ryuu.Modules
{
    [Name("owner")]
    public class OwnerModule : InteractiveBase<ShardedCommandContext>
    {
        private readonly IEnumerable<string> _dependencies = new[] {"Discord", "Discord.Net", "Discord.Commands", "Discord.WebSocket", "System", "System.Linq", "System.Collections.Generic", "System.Text", "System.Threading.Tasks"};
        
        // For my personal use owo
        // Makes it easy to announce stuff to certain roles without them always being mentionable
        [Command("release", RunMode = RunMode.Async)]
        [Summary("Send a release announcement")]
        [RequireOwner]
        [RequireBotPermission(ChannelPermission.SendMessages)]
        public async Task ReleaseNotesAsync(string bot, ITextChannel channel, [Remainder] string notes)
        {
            ulong roleId;
            switch (bot)
            {
                    case "jeanne":
                        roleId = 417489087998722058;
                        break;
                    case "yuzu":
                        roleId = 417489696894091274;
                        break;
                    case "saber":
                        roleId = 417489595467431956;
                        break;
                    default:
                        await ReplyAsync("No");
                        return;
            }
            
            var role = Context.Guild.GetRole(roleId);
            await role.ModifyAsync(x => x.Mentionable = true);
            await channel.SendMessageAsync($"{role.Mention}\n{notes}");
            await role.ModifyAsync(x => x.Mentionable = false);
        }
        
        [Command("exec", RunMode = RunMode.Async)]
        [Summary("Execute shell command")]
        [RequireOwner]
        [RequireBotPermission(ChannelPermission.SendMessages)]
        public async Task ExecAsync([Remainder] string cmd)
        {
            var escapedArgs = cmd.Replace("\"", "\\\"");
            
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "/bin/bash",
                    Arguments = $"-c \"{escapedArgs}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };
            process.Start();
            var stdOut = await process.StandardOutput.ReadToEndAsync();
            var stdErr = await process.StandardError.ReadToEndAsync();
            process.WaitForExit();
            
            if (stdErr != "")
            {
                await ReplyAsync($"```glsl\n{stdErr}```");
            }
            else
            {
                await ReplyAsync($"```glsl\n{stdOut}```");
            }
        }
        
        [Command("eval", RunMode = RunMode.Async)]
        [Summary("Evaluates some code and returns the result")]
        [RequireOwner]
        [RequireBotPermission(ChannelPermission.SendMessages)]
        public async Task EvaluateAsync([Remainder] string code)
        {
            const string codeBlockRegex = @"\`\`\`(?:(\S+?)[\n ])?\n*(?s:(.+?))\n*\`\`\`";
            if (Regex.IsMatch(code, codeBlockRegex, RegexOptions.Compiled | RegexOptions.Multiline))
            {
                code = $"{Regex.Match(code, codeBlockRegex, RegexOptions.Compiled | RegexOptions.Multiline).Groups[2]}";
            }
            var assemblies = Assembly.GetEntryAssembly()?.GetReferencedAssemblies().Select(Assembly.Load).ToList();
            assemblies?.Add(Assembly.GetEntryAssembly());
            
            var scriptOptions = ScriptOptions.Default
                .WithReferences(assemblies?.Select(x => MetadataReference.CreateFromFile(x.Location)))
                .WithImports(Assembly.GetEntryAssembly()?.GetTypes().Select(x => x.Namespace).Distinct());
            
            var embed = new EmbedBuilder()
                .WithTitle("Evaluate Code")
                .WithDescription("Debugging...");
                
            var message = await ReplyAsync(embed: embed.Build());
            try
            {
                var result = await CSharpScript.EvaluateAsync($"{string.Join("\n", _dependencies.Select(x => $"using {x};"))}\n{code}", scriptOptions, new EvaluateObject
                {
                    Client = Context.Client,
                    Context = Context
                }, typeof(EvaluateObject));
                embed.WithTitle("Completed").WithDescription($"Result: {result ?? "none"}");
                await message.ModifyAsync(x => x.Embed = embed.Build());
            }
            catch (Exception e)
            {
                embed.WithTitle("Failure").WithDescription($"Reason: {e.Message}");
                await message.ModifyAsync(x => x.Embed = embed.Build());
            }
            finally
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
        }
        
        [Command("setgame", RunMode = RunMode.Async)]
        [Summary("Set the current playing game")]
        [RequireOwner]
        [RequireBotPermission(ChannelPermission.SendMessages)]
        public async Task ChangeGameAsync([Remainder] string args = null)
        {
            if (args == null)
            {
                foreach( var shard in Context.Client.Shards )
                {
                    await shard.SetGameAsync(Program.PlayingStatus.Replace("{username}", shard.CurrentUser.Username));
                }

                await ReplyAsync("Reset the playing game");
            }
            else
            {
                foreach( var shard in Context.Client.Shards )
                {
                    await shard.SetGameAsync(args);
                }

                await ReplyAsync($"Changed playing game to: **{args}**");
            }
        }
        
        [Command("setstatus", RunMode = RunMode.Async)]
        [Summary("Set the current online status")]
        [RequireOwner]
        [RequireBotPermission(ChannelPermission.SendMessages)]
        public async Task ChangeStatusAsync([Remainder] string args)
        {
            UserStatus status;
            
            switch (args)
            {
                case "online":
                    status = UserStatus.Online;
                    break;
                case "offline":
                    status = UserStatus.Offline;
                    break;
                case "dnd":
                    status = UserStatus.DoNotDisturb;
                    break;
                case "idle":
                    status = UserStatus.Idle;
                    break;
                case "invisible":
                    status = UserStatus.Invisible;
                    break;
                case "afk":
                    status = UserStatus.AFK;
                    break;
                default:
                    status = UserStatus.Online;
                    break;
            }

            foreach( var shard in Context.Client.Shards )
            {
                await shard.SetStatusAsync(status);
            }

            await ReplyAsync($"Changed status to: **{args}**");
        }
        
        [Command("setname", RunMode = RunMode.Async)]
        [Summary("Set a new username for the bot")]
        [RequireOwner]
        [RequireBotPermission(ChannelPermission.SendMessages)]
        public async Task ChangeNameAsync([Remainder] string args)
        {
            await Context.Client.CurrentUser.ModifyAsync(x => x.Username = args);
            await ReplyAsync($"Changed my username to {args}");
        }
        
        [Command("setavatar", RunMode = RunMode.Async)]
        [Summary("Set a new avatar for the bot")]
        [RequireOwner]
        [RequireBotPermission(ChannelPermission.SendMessages)]
        public async Task ChangeAvatarAsync([Remainder] string args)
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("image/*"));
            client.DefaultRequestHeaders.Add("User-Agent", Utils.UserAgent);

            var stream = await client.GetStreamAsync(args);
            var img = new System.IO.BufferedStream(stream);

            await Context.Client.CurrentUser.ModifyAsync(x => x.Avatar = new Image(img));
            await ReplyAsync("Success changing my avatar, does it look good?");
        }

        public class EvaluateObject
        {
            public ShardedCommandContext Context { get; set; }
            public DiscordShardedClient Client { get; set; }
        }
    }
}