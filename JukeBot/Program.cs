using System;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Discord.Commands;

//MzE4MTQ0MTQzMTE5NjEzOTU0.DAuG2g.AExOqFG35a-OKUM045o5KjDIz-k; // JukeBot
//MzE4NTQ1ODk4MjUyNjY0ODMz.DAz8fw.Xhw6I7pXWa4K2To5lfx1Ebklp_Y; // TestingBot

public class Program {
    // Convert our sync main to an async main.
    public static void Main (string[] args) =>
        new Program().Start().GetAwaiter().GetResult();

    public static DiscordSocketClient DiscordClient;
    private CommandHandler handler;

    public async Task Start () {

        // Define the DiscordSocketClient with a DiscordSocketConfig
        DiscordClient = new DiscordSocketClient(new DiscordSocketConfig() { LogLevel = LogSeverity.Info });

        String BotToken = "MzE4MTQ0MTQzMTE5NjEzOTU0.DAuG2g.AExOqFG35a-OKUM045o5KjDIz-k"; //JukeBot

        // Login and connect to Discord.
        await DiscordClient.LoginAsync(TokenType.Bot, BotToken);

        await DiscordClient.StartAsync();

        var map = new DependencyMap();
        map.Add(DiscordClient);

        handler = new CommandHandler();
        await handler.Install(map);

        // Add logger
        DiscordClient.Log += Log;
        
        // Block this program until it is closed.
        await Task.Delay(-1);
    }

    // Bare minimum Logging function for both DiscordSocketClient and CommandService
    public static Task Log (LogMessage msg) {
        Console.WriteLine(msg.ToString());
        return Task.CompletedTask;
    }
}