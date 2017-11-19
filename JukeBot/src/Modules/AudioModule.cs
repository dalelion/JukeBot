using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using JukeBot.Services;
using Discord;
using Discord.Commands;
using YoutubeExplode;
using YoutubeExplode.Models;

namespace JukeBot.Modules {
    public class AudioModule : ModuleBase<ICommandContext> {
        private readonly AudioService _Service;

        public AudioModule( AudioService Service ) {
            _Service = Service;
        }

        static List<String> Queue = new List<string>();
        String Reply, NextSong, LeftInQueue;

        // You *MUST* mark these commands with 'RunMode.Async'
        // otherwise the bot will not respond until the Task times out.
        [Command( "join", RunMode = RunMode.Async )]
        [Alias( "j" )]
        public async Task JoinCmd() {
            await Log( new LogMessage( LogSeverity.Info, "Queue", $"Bot is joining {Context.Channel}" ) );
            await _Service.JoinAudio( Context.Guild, ( Context.User as IVoiceState ).VoiceChannel );
        }

        [Command( "leave", RunMode = RunMode.Async )]
        [Alias( "l" )]
        public async Task LeaveCmd() {
            await _Service.LeaveAudio( Context.Guild );
            await Log( new LogMessage( LogSeverity.Info, "Leave", $"Bot is leaving {Context.Channel}" ) );
        }

        [Command( "play", RunMode = RunMode.Async )]
        [Alias( "p" )]
        public async Task PlayCmd( [Remainder] string LinkOrSearchTerm ) {
            await _Service.LeaveAudio( Context.Guild );
            await _Service.JoinAudio( Context.Guild, ( Context.User as IVoiceState ).VoiceChannel );
            await _Service.SendAudioAsync( Context.Guild, LinkOrSearchTerm );
            await _Service.LeaveAudio( Context.Guild );
            await Log( new LogMessage( LogSeverity.Info, "Play", $"Bot is leaving {Context.Channel}" ) );
        }

        [Command( "playlist", RunMode = RunMode.Async )]
        [Alias( "pl" )]
        public async Task PlaylistCmd( [Remainder] string PlaylistLink ) {
            await _Service.LeaveAudio( Context.Guild );
            await _Service.JoinAudio( Context.Guild, ( Context.User as IVoiceState ).VoiceChannel );

            YoutubeClient YTC = new YoutubeClient();

            PlaylistInfo PlayListInfo = await YTC.GetPlaylistInfoAsync( YoutubeClient.ParsePlaylistId( PlaylistLink ) );

            String[] IDArray = PlayListInfo.VideoIds.ToArray();

            foreach ( String ID in IDArray ) {
                await _Service.SendAudioAsync( Context.Guild, ID );
            }
            await _Service.LeaveAudio( Context.Guild );
        }

        [Command( "queue", RunMode = RunMode.Async )]
        [Alias( "q" )]
        public async Task QueueSong( [Remainder] string LinkOrSearchTerm ) {
            Queue.Add( LinkOrSearchTerm );
            await Log( new LogMessage( LogSeverity.Info, "Queue", $"{LinkOrSearchTerm} added to queue" ) );
            Reply = Queue.Count == 1 ? "There is 1 song in the queue." : $"There are {Queue.Count} song in the queue.";
            await ReplyAsync( $"{LinkOrSearchTerm} added.\n{Reply}" );
        }

        [Command( "clearque", RunMode = RunMode.Async )]
        [Alias( "clearq" )]
        public async Task ClearQue() {
            Queue.Clear();
            await ReplyAsync( "Queue has been cleared" );
        }

        [Command( "playqueue", RunMode = RunMode.Async )]
        [Alias( "playq" )]
        public async Task PlayQueue() {
            while ( Queue.Count > 0 ) {
                await _Service.LeaveAudio( Context.Guild );
                await _Service.JoinAudio( Context.Guild, ( Context.User as IVoiceState ).VoiceChannel );

                NextSong = Queue.Count != 1 ? $", next song {Queue.ElementAt( 1 )}" : "";
                LeftInQueue = Queue.Count == 1 ? "There is 1 song in the queue." : $"There are {Queue.Count} song in the queue.";
                await ReplyAsync( $"Now Playing ID:{Queue.First()}{NextSong}.\n{LeftInQueue}" );
                await Log( new LogMessage( LogSeverity.Info, "Queue", $"Now Playing: {Queue.First()}{NextSong}.\n{LeftInQueue}" ) );

                await _Service.SendAudioAsync( Context.Guild, Queue.First() );
                Queue.RemoveAt( 0 );
            }

            await ReplyAsync( "Sorry, the queue is empty, !queue (or !q) to add more!" );
            await Log( new LogMessage( LogSeverity.Info, "Queue", "Queue is empty" ) );

            await _Service.LeaveAudio( Context.Guild );
            await Log( new LogMessage( LogSeverity.Info, "Queue", $"Bot is leaving {Context.Channel}" ) );
        }

        [Command( "removeat", RunMode = RunMode.Async )] // (add range ability?)
        public async Task RemoveNext( [Remainder] int Index ) {
            Queue.RemoveAt( Index );
            await ReplyAsync( $"Removed item {Queue.ElementAt( Index )} at index {Index}" );
            await Log( new LogMessage( LogSeverity.Info, "Queue", $"Removed item {Queue.ElementAt( Index )} at index {Index}" ) );
        }

        [Command( "stop", RunMode = RunMode.Async )] // (add range ability?)
        public async Task StopSong() {
            await Log( new LogMessage( LogSeverity.Error, "StopMethod", "Not Implemented", new NotImplementedException() ) );
            //await _service
        }

        //TODO: ****************
        //Insert, List queue, view at index (range?)
        //Play next song
        //Song length at start of song

        public static Task Log( LogMessage msg ) {
            Console.WriteLine( msg.ToString() );
            return Task.CompletedTask;
        }
    }
}
