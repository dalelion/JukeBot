using System;
using System.IO;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Discord.Commands;

namespace JukeBot {

    public class JukeBot {

        // Async main method
        public static void Main( string[] args ) =>
            new JukeBot().Start().GetAwaiter().GetResult();

        public static DiscordSocketClient DiscordClient;

        public async Task Start() {

            DiscordClient = new DiscordSocketClient( new DiscordSocketConfig() { LogLevel = LogSeverity.Info } );

            try {
                await DiscordClient.LoginAsync( TokenType.Bot, System.IO.File.ReadAllText( "Resources/Token.txt" ) );
            } catch ( Exception ) {
                await Log( new LogMessage( LogSeverity.Critical, "Token", "Invalid Token File" ) );
            }

            await DiscordClient.StartAsync();

            DependencyMap Map = new DependencyMap();
            Map.Add( DiscordClient );

            await new CommandHandler().Install( Map );

            DiscordClient.Log += Log;

            // Block this program until it is closed.
            await Task.Delay( -1 );
        }

        //TODO: Add extra functionality to the logger (Possible GUI)
        public static Task Log( LogMessage msg ) {
            Console.WriteLine( msg.ToString() );
            return Task.CompletedTask;
        }
    }
}