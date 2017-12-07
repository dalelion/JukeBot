using System;
using System.Text;
using System.Linq;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace JukeBot.Modules {
    public class StandardModule : ModuleBase {
        [Command( "say", RunMode = RunMode.Async )]
        [Alias( "echo" )]
        [Summary( "Echos the provided input" )]
        public async Task Say( [Remainder] string Input ) {
            await Log( new LogMessage( LogSeverity.Info, "Echo", $"Bot said {Input}" ) );
            await ReplyAsync( Input + " Test" );
        }

        [Command( "info", RunMode = RunMode.Async )]
        public async Task Info() {
            await Log( new LogMessage( LogSeverity.Info, "Info", "Bot reported info." ) );
            var Application = await Context.Client.GetApplicationInfoAsync();
            await ReplyAsync(
                $"{Format.Bold( "Info" )}\n" +
                $"- Author: {Application.Owner.Username} (ID {Application.Owner.Id})\n" +
                $"- Library: Discord.Net ({DiscordConfig.Version})\n" +
                $"- Runtime: {RuntimeInformation.FrameworkDescription} {RuntimeInformation.OSArchitecture}\n" +
                $"- Uptime: {GetUptime()}\n\n" +
                $"{Format.Bold( "Stats" )}\n" +
                $"- Heap Size: {GetHeapSize()} MB\n" +
                $"- Guilds: {( Context.Client as DiscordSocketClient ).Guilds.Count}\n" +
                $"- Channels: {( Context.Client as DiscordSocketClient ).Guilds.Sum( g => g.Channels.Count )}" +
                $"- Users: {( Context.Client as DiscordSocketClient ).Guilds.Sum( g => g.Users.Count )}"
            );
        }

        [Command( "help", RunMode = RunMode.Async )]
        [Alias( "h" )]
        public async Task Help() {

            StringBuilder SB = new StringBuilder();

            SB.AppendLine( "**Control Commands**" );
            SB.AppendLine( "**!info** = Displays bot info" );
            SB.AppendLine( "**!join** = Bot will join your channel *[Also !j]*" );
            SB.AppendLine( "**!leave** = Bot will leave its channel *[Also !l]*" );

            SB.AppendLine();

            SB.AppendLine( "**Audio Commands**" );
            SB.AppendLine( "**!play {Song name or link}** = Plays the song *[Also !p]*" );
            SB.AppendLine( "**!pause = Pauses/Unpauses the current song." );
            SB.AppendLine( "**!Seek {0-100}** = Seeks through the current song by percentages (ex. *!seek 50* skips to the middle)." );
            SB.AppendLine( "**!playlist {PlaylistLink}** = Plays a whole playlist *[Also !pl]*" );
            SB.AppendLine( "**!queue {Song name or link}** = Add a song to the queue *[Also !q]*" );
            SB.AppendLine( "**!playqueue** = Plays the queue *[Also !playq]*" );
            SB.AppendLine( "**!clearqueue** = Clears the queue *[Also !clearq]*" );
            SB.AppendLine( "**!removeat {index}** = Removes the element at the index." );

            SB.AppendLine();

            SB.AppendLine( "**Image Commands**" );
            SB.AppendLine( "**!jpg** {Search Query} [Also {Search Query}**.jpg**]" );
            SB.AppendLine( "**!gif** {Search Query} [Also {Search Query}**.gif**]" );

            await ReplyAsync( SB.ToString() );

            await Log( new LogMessage( LogSeverity.Info, "Help", "User asked for help." ) );
        }

        private static string GetUptime() => ( DateTime.Now - Process.GetCurrentProcess().StartTime ).ToString( @"dd\.hh\:mm\:ss" );

        private static string GetHeapSize() => Math.Round( GC.GetTotalMemory( true ) / ( 1024.0 * 1024.0 ), 2 ).ToString();

        public static Task Log( LogMessage msg ) {
            Console.WriteLine( msg.ToString() );
            return Task.CompletedTask;
        }
    }
}
