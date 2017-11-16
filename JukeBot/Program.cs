using System;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Discord.Commands;

namespace JukeBot {

    public class Program {
        // Asyncronouse main method
        public static void Main( string[] args ) =>
            new Program().Start().GetAwaiter().GetResult();

        public static DiscordSocketClient DiscordClient;
        private CommandHandler Handler;

        /// <summary>
        /// 
        /// </summary>
        public async Task Start() {

            // Define the DiscordSocketClient with a DiscordSocketConfig
            DiscordClient = new DiscordSocketClient( new DiscordSocketConfig() { LogLevel = LogSeverity.Info } );
            
            try {
            await DiscordClient.LoginAsync( TokenType.Bot, System.IO.File.ReadAllText( "Token.txt" ) );
            } catch (Exception E) {
                await Log( new LogMessage( LogSeverity.Critical, "Token", "Invalid Token File" ) );
            }

            await DiscordClient.StartAsync();

            DependencyMap Map = new DependencyMap();
            Map.Add( DiscordClient );

            CommandHandler Handler = new CommandHandler();
            await Handler.Install( Map );
            
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