using Discord;
using Discord.Net;
using Discord.WebSocket;
using Newtonsoft.Json;
using PokemonTcgSdk.Standard.Features.FilterBuilder.Pokemon;
using PokemonTcgSdk.Standard.Infrastructure.HttpClients;
using PokemonTcgSdk.Standard.Infrastructure.HttpClients.Cards;
using PokemonTcgSdk.Standard.Infrastructure.HttpClients.Cards.Models;
using PokemonTcgSdk.Standard.Infrastructure.HttpClients.Set;
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Newtonsoft.Json.Linq;

public class Commands
{
    public string pokemonAPIKey = File.ReadAllText("F:\\DiscordToken\\pokemonAPIKey.txt");
    private readonly PokemonApiClient pokeClient;
    string setId = "swsh1";
    private readonly DiscordSocketClient client;
    public Commands(DiscordSocketClient _client)
    {
        client = _client;
        client.Ready += Client_Ready;
        client.SlashCommandExecuted += SlashCommandHandlerStartup;
        pokeClient = new PokemonApiClient(pokemonAPIKey);
    }

    public async Task Client_Ready()
    {
        //var guild = client.GetGuild(712014454098755584);
        //var guildCommand = new SlashCommandBuilder();
        //guildCommand.WithName("openpack1")
        //            .WithDescription("Open 1 Pack of Pack1?");

        //try {
        //    await guild.CreateApplicationCommandAsync(guildCommand.Build());
        //}
        //catch (ApplicationCommandException exception)
        //{
        //    var json = JsonConvert.SerializeObject(exception.Errors, Formatting.Indented);
        //    Console.WriteLine(json);
        //}
        PokemonApiClient pokeClient = new PokemonApiClient();
        pokeClient = new PokemonApiClient(pokemonAPIKey);
        var filter = PokemonFilterBuilder.CreatePokemonFilter().AddSetId(setId);
        var card = await pokeClient.GetApiResourceAsync<Card>(filter);
        Console.WriteLine(card.Results.Any(x => x.Set.Id == "swsh1"));
    }
    private Task SlashCommandHandlerStartup(SocketSlashCommand command)
    {
        Console.WriteLine("Command Stratup");
        _ = SlashCommandHandler(command);
        return Task.CompletedTask;
    }

    private async Task SlashCommandHandler(SocketSlashCommand command)
    {
        Console.WriteLine("COMMAND RECEIVED");
        switch (command.Data.Name)
        {
            case "openpack":
                //await command.RespondAsync($"A Pack has just been opened!");
                await Packhandler(command);
                break;
            case "test1":
                await command.RespondAsync("test");
                break;
        }
    }
    private async Task Packhandler(SocketSlashCommand command)
    {
        var imageUrl = await CardDataAsync();

        var embedBuiler = new EmbedBuilder()
            .WithTitle("Venusaur-EX")
            .WithImageUrl(imageUrl?? "https://images.pokemontcg.io/col1435/12_hires.png")
            .WithDescription("Basic\r\n      EX");
        var button = new ButtonBuilder()
            .WithLabel("next")
            .WithStyle(ButtonStyle.Primary)
            .WithCustomId("testbutton1");
        await command.RespondAsync(embed: embedBuiler.Build());
    }
    private async Task<String?> CardDataAsync(string setId = "swsh1", int take = 30, int skip = 0)
    {
        try
        {
            //  Console.WriteLine(pokeClient);
            var filter = PokemonFilterBuilder.CreatePokemonFilter().AddSetId(setId);
            var card = await pokeClient.GetApiResourceAsync<Card>(filter);
            var json = JsonConvert.SerializeObject(card, Formatting.Indented);
            Console.WriteLine("Returned JSON:\n" + json);
            var token = JToken.Parse(json);
            int randomcard = new Random().Next(0, take);
            var imageUrl =
            token["data"]?[randomcard]?["images"]?["large"]?.ToString()
            ?? token["Data"]?[randomcard]?["Images"]?["Large"]?.ToString();


            Console.WriteLine("Image URL: " + (imageUrl ?? "none found"));
            //Console.WriteLine(card.GetType().FullName);
            return imageUrl;
        }

        catch (Exception ex)
        {
            Console.WriteLine($"CardDataAsync error: {ex.Message}");
            return null;
        }
        }

    private async Task Buttons()
    {

    }




}
 
    

