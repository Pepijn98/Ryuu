using System;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Kitsu.Anime;
using Kitsu.Manga;
using Ryuu.Handlers;
using Ryuu.Utilities;

// ReSharper disable UnusedType.Global
// ReSharper disable UnusedMember.Global

namespace Ryuu.Modules
{
    [Name("weeb")]
    public class WeebModule : InteractiveBase<ShardedCommandContext>
    {
        [Command("catgirl", RunMode = RunMode.Async), Alias("neko")]
        [Summary("Send a cute catgirl image")]
        [RequireBotPermission(ChannelPermission.SendMessages | ChannelPermission.EmbedLinks)]
        public async Task CatgirlAsync([Remainder, Name("nsfw/sfw **(optional)**")] string args = "")
        {
            args = args.ToLower();

            string catgirlId;

            switch (args)
            {
                case "nsfw":
                    if (Context.Channel is ITextChannel chan && chan.IsNsfw)
                    {
                        catgirlId = await Utils.Catgirl(true);
                    }
                    else
                    {
                        await ReplyAsync("To get nsfw catgirls make sure to enable nsfw in the channel options");
                        return;
                    }
                    break;
                case "sfw":
                case "":
                    catgirlId = await Utils.Catgirl(false);
                    break;
                default:
                    await ReplyAsync("You can only use `nsfw`, `sfw` or nothing");
                    return;
            }

            var embed = new EmbedBuilder
            {
                ImageUrl = $"https://nekos.moe/image/{catgirlId}",
                Color = new Color(ConfigHandler.Config.EmbedColor)
            }.Build();
            
            await ReplyAsync("", embed: embed);
        }

        [Command("anime", RunMode = RunMode.Async)]
        [Summary("Shows info about the given anime by its name")]
        [RequireBotPermission(ChannelPermission.SendMessages | ChannelPermission.EmbedLinks)]
        public async Task AnimeAsync([Remainder, Name("anime_name")] string args)
        {
            // TODO: Show more info about the anime in the embed

            try
            {
                // var animes = new List<string>();
                var res = await Anime.GetAnimeAsync(args);
                var anime = res.Data[0];
                /*foreach (var anime in res.Data)
                {
                    var str = $"**{anime.Attributes.CanonicalTitle ?? anime.Attributes.Titles.JaJp}**\n" +
                              $"{anime.Attributes.Synopsis ?? "-"}\n\n" +
                              $"{anime.Attributes.PosterImage.Medium}";
                    animes.Add(str);
                }*/
                
                var embed = new EmbedBuilder
                {
                    Color = new Color(ConfigHandler.Config.EmbedColor),
                    Title = anime.Attributes.CanonicalTitle ?? anime.Attributes.Titles.JaJp,
                    ThumbnailUrl = anime.Attributes.PosterImage.Medium,
                    Description = anime.Attributes.Synopsis ?? "-"
                }.Build();

                await ReplyAsync("", embed: embed);
                // await PagedReplyAsync(animes);
            }
            catch (Exception e)
            {
                var embed = new EmbedBuilder
                {
                    Color = new Color(0xFF0000),
                    Description = e.Message
                }.Build();
                await ReplyAsync("", embed: embed);
                throw;
            }
        }
        [Command("anime", RunMode = RunMode.Async)]
        [Summary("Shows info about the given anime by its id")]
        [RequireBotPermission(ChannelPermission.SendMessages | ChannelPermission.EmbedLinks)]
        public async Task AnimeAsync([Remainder, Name("anime_id")] int args)
        {
            // TODO: Show more info about the anime in the embed

            var anime = await Anime.GetAnimeAsync(args);

            if (anime.Errors.Length >= 1)
            {
                await ReplyAsync($"```glsl\nError: {anime.Errors[0].Code}\n\n{anime.Errors[0].Title}\n{anime.Errors[0].Detail}```");
                return;
            }
            
            var embed = new EmbedBuilder
            {
                Color = new Color(ConfigHandler.Config.EmbedColor),
                Title = anime.Data.Attributes.CanonicalTitle ?? anime.Data.Attributes.Titles.JaJp,
                Description = anime.Data.Attributes.Synopsis,
                ThumbnailUrl = anime.Data.Attributes.PosterImage.Medium
            }.Build();
            
            await ReplyAsync("", embed: embed);
        }

        [Command("manga", RunMode = RunMode.Async)]
        [Summary("Shows info about the given manga by its name")]
        [RequireBotPermission(ChannelPermission.SendMessages | ChannelPermission.EmbedLinks)]
        public async Task MangaAsync([Remainder, Name("manga_name")] string args)
        {
            // TODO: Show more info about the manga in the embed

            var manga = await Manga.GetMangaAsync(args);
            
            var embed = new EmbedBuilder
            {
                Color = new Color(ConfigHandler.Config.EmbedColor),
                Title = manga.Data[0].Attributes.CanonicalTitle ?? manga.Data[0].Attributes.Titles.JaJp,
                Description = manga.Data[0].Attributes.Synopsis,
                ThumbnailUrl = manga.Data[0].Attributes.PosterImage.Medium
            }.Build();
            
            await ReplyAsync("", embed: embed);
        }
        [Command("manga", RunMode = RunMode.Async)]
        [Summary("Shows info about the given manga by its id")]
        [RequireBotPermission(ChannelPermission.SendMessages | ChannelPermission.EmbedLinks)]
        public async Task MangaAsync([Remainder, Name("manga_id")] int args)
        {
            // TODO: Show more info about the manga in the embed

            var manga = await Manga.GetMangaAsync(args);
            
            if (manga.Errors.Length >= 1)
            {
                await ReplyAsync($"```glsl\nError: {manga.Errors[0].Code}\n\n{manga.Errors[0].Title}\n{manga.Errors[0].Detail}```");
                return;
            }
            
            var embed = new EmbedBuilder
            {
                Color = new Color(ConfigHandler.Config.EmbedColor),
                Title = manga.Data.Attributes.CanonicalTitle ?? manga.Data.Attributes.Titles.JaJp,
                Description = manga.Data.Attributes.Synopsis,
                ThumbnailUrl = manga.Data.Attributes.PosterImage.Medium
            }.Build();
            
            await ReplyAsync("", embed: embed);
        }
    }
}