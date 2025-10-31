using System;
using Discord;

public class Commands
{
	public Commands()
	{

	}
	public async Task Client_Ready()
	{
        var guild = client.GetGuild(guildId);
        var guildCommand = new SlashCommandBuilder();
        guildCommand.WithName("test1");
        guildCommand.WithDescription("TESTING COMPLETE HELLO BITCH");
    }
}
