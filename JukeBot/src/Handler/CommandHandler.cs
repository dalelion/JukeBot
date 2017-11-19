using System.Threading.Tasks;
using System.Reflection;
using Discord.Commands;
using Discord.WebSocket;
using JukeBot.Services;

namespace JukeBot {
    public class CommandHandler {
        private CommandService Commands;
        private DiscordSocketClient Client;
        private IDependencyMap Map;

        public async Task Install( IDependencyMap _map ) {
            // Create Command Service, inject it into Dependency Map
            Client = _map.Get<DiscordSocketClient>();
            Commands = new CommandService();
            Commands.Log += JukeBot.Log;

            Map = _map;

            Map.Add( new AudioService() );

            Map.Add( new ImageService() );

            await Commands.AddModulesAsync( Assembly.GetEntryAssembly() );

            Client.MessageReceived += PrefixCommandHandler;
            Client.MessageReceived += PostfixCommandHandler;
        }

        public async Task PrefixCommandHandler( SocketMessage ParameterMessage ) {
            // Don't handle the command if it is a system message
            SocketUserMessage Message = ParameterMessage as SocketUserMessage;
            if ( Message == null )
                return;

            int PrefixPosition = 0;
            // Determine if the message has a valid prefix, adjust argPos 
            if ( !( Message.HasMentionPrefix( Client.CurrentUser, ref PrefixPosition ) || Message.HasCharPrefix( '!', ref PrefixPosition ) ) )
                return;

            // Create a Command Context
            CommandContext Context = new CommandContext( Client, Message );

            // Execute the Command, store the result
            IResult Result = await Commands.ExecuteAsync( Context, PrefixPosition, Map );

            // If the command failed, notify the user
            if ( !Result.IsSuccess )
                await Message.Channel.SendMessageAsync( $"**Error:** {Result.ErrorReason}" );
        }

        public async Task PostfixCommandHandler( SocketMessage ParameterMessage ) {
            // Don't handle the command if it is a system message
            SocketUserMessage Message = ParameterMessage as SocketUserMessage;
            if ( Message == null || Message.Author.IsBot || Message.Author.Id.Equals( Client.CurrentUser.Id ) )
                return;

            string Content = Message.Content.ToLower();

            switch ( Content.Substring( Content.Length - 4 ) ) {
                case ".jpg":
                    Content = "jpg " + Content.Substring( 0, Content.Length - 4 );
                    break;
                case ".gif":
                    Content = "gif " + Content.Substring( 0, Content.Length - 4 );
                    break;
                default:
                    return;
            }

            // Create a Command Context
            CommandContext Context = new CommandContext( Client, Message );

            // Execute the Command, store the result
            IResult Result = await Commands.ExecuteAsync( Context, Content, Map );

            // If the command failed, notify the user
            if ( !Result.IsSuccess )
                await Message.Channel.SendMessageAsync( $"**Error:** {Result.ErrorReason}" );
        }
    }
}
