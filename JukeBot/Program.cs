using System;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Discord.Commands;

namespace JukeBot {

    public class Program {
        // Convert our sync main to an async main.
        public static void Main( string[] args ) =>
            new Program().Start().GetAwaiter().GetResult();

        public static DiscordSocketClient DiscordClient;
        private CommandHandler handler;

        public async Task Start() {

            // Define the DiscordSocketClient with a DiscordSocketConfig
            DiscordClient = new DiscordSocketClient( new DiscordSocketConfig() { LogLevel = LogSeverity.Info } );

            String BotToken = System.IO.File.ReadAllText( "Token.txt" );

            // Login and connect to Discord.
            await DiscordClient.LoginAsync( TokenType.Bot, BotToken );

            await DiscordClient.StartAsync();

            var map = new DependencyMap();
            map.Add( DiscordClient );

            handler = new CommandHandler();
            await handler.Install( map );

            // Add logger
            DiscordClient.Log += Log;

            // Block this program until it is closed.
            await Task.Delay( -1 );
        }

        // Bare minimum Logging function for both DiscordSocketClient and CommandService
        public static Task Log( LogMessage msg ) {
            Console.WriteLine( msg.ToString() );
            return Task.CompletedTask;
        }
    }
}