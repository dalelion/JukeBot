using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Runtime.InteropServices;
using System.Diagnostics;
using JukeBot.Services;

namespace JukeBot.Modules {

    public class BaseModule : ModuleBase {

        [Command( "say" )]
        [Alias( "echo" )]
        [Summary( "Echos the provided input" )]
        public async Task Say( [Remainder] string Input ) {
            await Log( new LogMessage( LogSeverity.Info, "Echo", $"Bot said {Input}" ) );
            await ReplyAsync( Input + " Test");
        }

        [Command( "info" )]
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
            await ReplyAsync( "**!play {Song name or link}** = Plays the song *[Also !p]*\n" +
                            "**!join** = Bot will join your channel *[Also !j]*\n" +
                            "**!leave** = Bot will leave its channel *[Also !l]*\n" +
                            "**!queue {Song name or link}** = Add a song to the queue *[Also !q]*\n" +
                            "**!playqueue** = Plays the queue *[Also !playq]*\n" +
                            "**!clearqueue** = Clears the queue *[Also !clearq]*\n" +
                            "**!playlist {PlaylistLink}** = Plays a whole playlist *[Also !pl]*\n" +
                            "**!info** = Displays bot info\n" +
                            "**!removeat {index}** = Removes the element at the index" );
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