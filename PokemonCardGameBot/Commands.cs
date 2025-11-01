using Discord;
using Discord.Net;
using Discord.WebSocket;
using Newtonsoft.Json;
using System;
using System.Diagnostics;

public class Commands
{
    
    private readonly DiscordSocketClient client;
    public Commands(DiscordSocketClient _client)
	{
        client = _client;
        client.Ready += Client_Ready;
        client.SlashCommandExecuted += SlashCommandHandler;
    }

	public async Task Client_Ready()
	{

        var guild = client.GetGuild(712014454098755584);
        var guildCommand = new SlashCommandBuilder();
        guildCommand.WithName("test1");
        guildCommand.WithDescription("first test command");
        

        try {
            await guild.CreateApplicationCommandAsync(guildCommand.Build());
        }
        catch (ApplicationCommandException exception)
        {
            var json = JsonConvert.SerializeObject(exception.Errors, Formatting.Indented);
            Console.WriteLine(json);
        }
    }

    private async Task SlashCommandHandler(SocketSlashCommand command)
    {
        await command.RespondAsync($"TESTING HELLO HELLO HELLO BITCH");
    }
}
