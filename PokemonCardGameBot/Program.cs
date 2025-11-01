using Discord;
using Discord.WebSocket;
using System.Diagnostics;

public class Program
{
    public static void Main(string[] args) => new Program().MainAsync().GetAwaiter().GetResult();

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
        Console.WriteLine("1");
        await _client.LoginAsync(TokenType.Bot, token);
        Console.WriteLine("2");
        await _client.StartAsync();
        commands = new Commands(_client);
        await commands.Client_Ready();
        await Task.Delay(-1);


    }
    private Task Log(LogMessage msg)
    {
        Console.WriteLine("3");
        Console.WriteLine(msg.ToString());
        return Task.CompletedTask;
    }



}
