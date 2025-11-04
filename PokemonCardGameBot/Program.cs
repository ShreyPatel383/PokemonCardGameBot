using Discord;
using Discord.WebSocket;
using PokemonTcgSdk;
using PokemonTcgSdk.Standard.Infrastructure.HttpClients;
using System.Diagnostics;

public class Program
{

    public static void Main(string[] args) => new Program().MainAsync().GetAwaiter().GetResult();
    PokemonApiClient pokeClient = new PokemonApiClient();
    private DiscordSocketClient _client;
    private Commands commands;

    // public static void Main(string[] args) => new Program().MainAsync().GetAwaiter().GetResult();
    public async Task MainAsync()
    {

        var config = new DiscordSocketConfig()
        {
            GatewayIntents = GatewayIntents.Guilds |
                             GatewayIntents.GuildMessages |
                             GatewayIntents.MessageContent
        };
        _client = new DiscordSocketClient(config);
        _client.Log += Log;

        var token = File.ReadAllText("F:\\DiscordToken\\token.txt");
        await _client.LoginAsync(TokenType.Bot, token);
        await _client.StartAsync();
        commands = new Commands(_client);
        await Task.Delay(-1);


    }
    private Task Log(LogMessage msg)
    {
        Console.WriteLine("Task Logged!");
        Console.WriteLine(msg.ToString());
        return Task.CompletedTask;
    }



}
