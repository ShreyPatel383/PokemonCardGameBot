using Discord;
using Discord.WebSocket;

public class Program
{
    private static DiscordSocketClient _client;
    public static async Task Main()
    {
        _client = new DiscordSocketClient();
        _client.Log += Log;

        var token = File.ReadAllText("F:\\DiscordToken\\token.txt");

        await _client.LoginAsync(TokenType.Bot, token);
        await _client.StartAsync();
        await Task.Delay(-1);

    }
    private static Task Log(LogMessage msg)
    {
        Console.WriteLine(msg.ToString());
        return Task.CompletedTask;
    }


}
