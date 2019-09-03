using System;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using IMDBCore;
using MtgApiManager.Lib.Service;
// using PokeAPI;
using Ryuu.Handlers;
using Ryuu.Utilities;

// ReSharper disable UnusedMember.Global

namespace Ryuu.Modules
{
    [Name("fun")]
    public class FunModule : InteractiveBase<ShardedCommandContext>
    {
        // Initialize imdb to use in the imdb command
        private readonly Imdb _imdb = new Imdb(ConfigHandler.Config.OmdbKey);
        
        [Command("cleverbot", RunMode = RunMode.Async), Alias("cb", "clever", "talk")]
        [Summary("Talk with the bot")]
        [RequireBotPermission(ChannelPermission.SendMessages)]
        public async Task CleverbotAsync([Remainder, Name("query")] string args)
        {
            await Context.Channel.TriggerTypingAsync();
            
            var reply = await Utils.Cleverbot(args, $"enomoto_{Context.Message.Author.Id}");
            await ReplyAsync(reply);
        }
        
        [Command("imdb", RunMode = RunMode.Async)]
        [Summary("Shows info about the given movie")]
        [RequireBotPermission(ChannelPermission.SendMessages | ChannelPermission.EmbedLinks)]
        public async Task ImdbAsync([Remainder, Name("movie_name")] string args)
        {
            // TODO: Show more info about the movie in the embed

            try
            {
                var movie = await _imdb.GetMovieAsync(args);

                if (movie.Error != null)
                {
                    await ReplyAsync(movie.Error);
                    return;
                }
            
                var embed = new EmbedBuilder
                {
                    Color = new Color(ConfigHandler.Config.EmbedColor),
                    Title = movie.Title,
                    Description = $"{movie.ImdbId}\n{movie.Plot}",
                    ThumbnailUrl = movie.Poster
                }.Build();

                await ReplyAsync("", embed: embed);
            }
            catch (Exception e)
            {
                var embed = new EmbedBuilder
                {
                    Color = new Color(0xFF0000),
                    Description = e.Message
                };
                await ReplyAsync("", embed: embed.Build());
                throw;
            }
        }
        
        [Command("mtg", RunMode = RunMode.Async)]
        [Summary("Show magic the gathering cards")]
        [RequireBotPermission(ChannelPermission.SendMessages | ChannelPermission.EmbedLinks)]
        public async Task MtgAsync([Remainder, Name("card_name")] string args)
        {
            // TODO: Add more info about the card to the embed
            
            var service = new CardService();
            var result = await service.Where(x => x.Name, args).AllAsync();
            if (result.IsSuccess)
            {
                EmbedBuilder embed;
                
                var cards = result.Value;
                if (cards.Count < 1)
                {
                    embed = new EmbedBuilder
                    {
                        Color = new Color(0xFF0000),
                        Description = $"Could not find a card with the name **{args}**"
                    };
                
                    await ReplyAsync("", embed: embed.Build());
                    return;
                }

                embed = new EmbedBuilder
                {
                    Color = new Color(ConfigHandler.Config.EmbedColor),
                    Title = cards[0].Name,
                    Description = cards[0].Flavor,
                    ImageUrl = cards[0].ImageUrl.AbsoluteUri
                };
                
                await ReplyAsync("", embed: embed.Build());
            }
            else
            {
                var exception = result.Exception;
                
                var embed = new EmbedBuilder
                {
                    Color = new Color(0xFF0000),
                    Description = exception.Message
                };
                
                await ReplyAsync("", embed: embed.Build());
            }
        }
        
        /*
        [Command("pokemon", RunMode = RunMode.Async)]
        [Summary("Show pokemon info")]
        [RequireBotPermission(ChannelPermission.SendMessages | ChannelPermission.EmbedLinks)]
        public async Task PokemonAsync([Remainder, Name("pokemon_name")] string args)
        {
            // TODO: Add more pokemon info to the embed

            try
            {
                var pokemon = await DataFetcher.GetApiObject<Pokemon>(args)
                
                var embed = new EmbedBuilder
                {
                    Color = new Color(ConfigHandler.Config.EmbedColor),
                    Title = pokemon.Name,
                    ThumbnailUrl = pokemon.Sprites.FrontDefault
                };

                await ReplyAsync("", embed: embed.Build());
            }
            catch (Exception e)
            {
                var embed = new EmbedBuilder
                {
                    Color = new Color(0xFF0000),
                    Description = e.Message
                };
                await ReplyAsync("", embed: embed.Build());
                throw;
            }
        }
        */
    }
}