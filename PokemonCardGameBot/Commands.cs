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
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Newtonsoft.Json.Linq;

public class Commands
{
    private readonly string _pokemonAPIKey;
    private readonly PokemonApiClient pokeClient;
    string setId = "sv3pt5";
    private readonly DiscordSocketClient client;

    private const int MaxOpens = 10;
    private readonly ConcurrentDictionary<ulong, int> _userOpenCounts = new();
    private readonly ConcurrentDictionary<ulong, object> _userLocks = new();

    public Commands(DiscordSocketClient _client, string pokemonAPiKey)
    {
        client = _client;
        client.Ready += Client_Ready;
        client.SlashCommandExecuted += SlashCommandHandlerStartup;
        _pokemonAPIKey = pokemonAPiKey ?? throw new ArgumentNullException(nameof(pokemonAPiKey));
        pokeClient = new PokemonApiClient(_pokemonAPIKey);
        client.InteractionCreated += OnInteractionCreatedAsync;
    }

    //For use in CardDataAsync to get random cards.
    private static readonly Random _rng = Random.Shared;

    private object GetUserLock(ulong userId) => _userLocks.GetOrAdd(userId, _ => new object());

    public async Task Client_Ready()
    {
        var filter = PokemonFilterBuilder.CreatePokemonFilter().AddSetId(setId);
        var cards = await pokeClient.GetApiResourceAsync<Card>(filter);
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
                await Packhandler(command);
                break;
            case "test1":
                await command.RespondAsync("test");
                break;
        }
    }

    private async Task Packhandler(SocketSlashCommand command)
    {
        var userId = command.User.Id;
        int newCount = 0;

        // update count under per-user lock (no awaits while locked)
        var userLock = GetUserLock(userId);
        lock (userLock)
        {
            if (_userOpenCounts.TryGetValue(userId, out var currentCount) && currentCount >= MaxOpens)
            {
                //Reset count once reaching max opens (10)
                _userOpenCounts[userId] = 1;
            }
            else
            {
                var cur = _userOpenCounts.GetOrAdd(userId, 0);
                newCount = cur + 1;
                _userOpenCounts[userId] = newCount;
            }

        }

        if (newCount == -1)
        {
            await command.RespondAsync($"You have already opened {MaxOpens} packs.", ephemeral: true);
            return;
        }

        var imageUrl = await CardDataAsync();

        var embedBuiler = new EmbedBuilder()
            .WithTitle("Venusaur-EX")
            .WithImageUrl(imageUrl ?? "https://images.pokemontcg.io/col1435/12_hires.png")
            .WithDescription("Basic\r\n      EX")
            .WithFooter($"Open {newCount}/{MaxOpens}");

        var components = new ComponentBuilder();
        if (newCount < MaxOpens)
            components.WithButton("Next", "pack_next", ButtonStyle.Primary);
        else
            components.WithButton("Next (max)", "pack_disabled", ButtonStyle.Secondary, disabled: true);

        await command.RespondAsync(embed: embedBuiler.Build(), components: components.Build());
    }

    private async Task<String?> CardDataAsync(string setId = "sv3pt5")
    {
        try
        {
            var filter = PokemonFilterBuilder.CreatePokemonFilter().AddSetId(setId);
            var cards = await pokeClient.GetApiResourceAsync<Card>(filter);
            if (cards?.Results == null || cards.Results.Count == 0)
            {
                Console.WriteLine("CardDataAsync: no results returned from API.");
                return null;
            }

            int randomcard = _rng.Next(cards.Results.Count);
            var imageUrl = cards.Results[randomcard].Images.Large?.ToString() ?? cards.Results[randomcard].Images.Small?.ToString();

            Console.WriteLine("Image URL: " + (imageUrl ?? "none found"));
            return imageUrl;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"CardDataAsync error: {ex.Message}");
            return null;
        }
    }

    private async Task OnInteractionCreatedAsync(SocketInteraction interaction)
    {
        if (!(interaction is SocketMessageComponent comp) || comp.Data.CustomId != "pack_next")
            return;

        var userId = comp.User.Id;
        int newCount;

        // increment under lock
        var userLock = GetUserLock(userId);
        lock (userLock)
        {
            var cur = _userOpenCounts.GetOrAdd(userId, 0);
            if (cur >= MaxOpens)
            {
                newCount = -1; // already at cap
            }
            else
            {
                newCount = cur + 1;
                _userOpenCounts[userId] = newCount;
            }
        }

        // remove the button from the previous message immediately
        try
        {
            await comp.UpdateAsync(msg => msg.Components = null);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to remove components from original message: {ex.Message}");
            try
            {
                var disabled = new ComponentBuilder()
                    .WithButton("Next", "pack_next", ButtonStyle.Primary, disabled: true)
                    .Build();
                await comp.UpdateAsync(msg => msg.Components = disabled);
            }
            catch (Exception ex2)
            {
                Console.WriteLine($"Fallback disable failed: {ex2.Message}");
            }
        }

        if (newCount == -1)
        {
            await comp.FollowupAsync($"You have already opened {MaxOpens} packs.", ephemeral: true);
            return;
        }

        var newImage = await CardDataAsync();
        var newEmbed = new EmbedBuilder()
            .WithTitle("Venusaur-EX")
            .WithImageUrl(newImage ?? "https://images.pokemontcg.io/col1435/12_hires.png")
            .WithDescription("Basic\r\n      EX")
            .WithFooter($"Open {newCount}/{MaxOpens}")
            .Build();

        var components = new ComponentBuilder();
        if (newCount < MaxOpens)
            components.WithButton("Next", "pack_next", ButtonStyle.Primary);
        else
            components.WithButton("Next (max)", "pack_disabled", ButtonStyle.Secondary, disabled: true);

        try
        {
            await comp.FollowupAsync(embed: newEmbed, components: components.Build());
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to send followup embed: {ex.Message}");
            try
            {
                await comp.FollowupAsync("Could not send the new embed right now.", ephemeral: true);
            }
            catch (Exception inner)
            {
                Console.WriteLine($"Fallback followup failed: {inner.Message}");
            }
        }
    }
}
 
    

