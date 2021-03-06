using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using JukeBot.Services;
using Discord;
using Discord.Commands;
using YoutubeExplode;

namespace JukeBot.Modules {
    public class AudioModule : ModuleBase<ICommandContext> {
        private readonly AudioService _Service;

        public AudioModule( AudioService Service ) {
            this._Service = Service;
        }

        private static List<string> Queue = new List<string>();
        private string Reply, NextSong, LeftInQueue;

        // You *MUST* mark these commands with 'RunMode.Async'
        // otherwise the bot will not respond until the Task times out.
        [Command( "join", RunMode = RunMode.Async )]
        [Alias( "j" )]
        public async Task JoinCmd() {
            await Log( new LogMessage( LogSeverity.Info, "Queue", $"Bot is joining {this.Context.Channel}" ) );
            await this._Service.JoinAudio( this.Context.Guild, ( this.Context.User as IVoiceState ).VoiceChannel );
        }

        [Command( "leave", RunMode = RunMode.Async )]
        [Alias( "l" )]
        public async Task LeaveCmd() {
            await this._Service.LeaveAudio( this.Context.Guild );
            await Log( new LogMessage( LogSeverity.Info, "Leave", $"Bot is leaving {this.Context.Channel}" ) );
        }

        [Command( "play", RunMode = RunMode.Async )]
        [Alias( "p" )]
        public async Task PlayCmd( [Remainder] string LinkOrSearchTerm ) {
            await this._Service.LeaveAudio( this.Context.Guild );
            await this._Service.JoinAudio( this.Context.Guild, ( this.Context.User as IVoiceState ).VoiceChannel );
            await this._Service.SendAudioAsync( this.Context.Guild, LinkOrSearchTerm );
            await this._Service.LeaveAudio( this.Context.Guild );
            await Log( new LogMessage( LogSeverity.Info, "Play", $"Bot is leaving {this.Context.Channel}" ) );
        }

        [Command( "play4chan", RunMode = RunMode.Async )]
        [Alias( "p4" )]
        public async Task Play4chanCmd( [Remainder] string UserInput ) {
            await this._Service.LeaveAudio( this.Context.Guild );
            await this._Service.JoinAudio( this.Context.Guild, ( this.Context.User as IVoiceState ).VoiceChannel );
            await this._Service.SendWEBMAudioAsync( this.Context.Guild, UserInput );
            await this._Service.LeaveAudio( this.Context.Guild );
            await Log( new LogMessage( LogSeverity.Info, "Play", $"Bot is leaving {this.Context.Channel}" ) );
        }

        [Command( "seek", RunMode = RunMode.Async )]
        public async Task SeekCmd( [Remainder] string PercentDuration ) {
            await Log( new LogMessage( LogSeverity.Info, "seek", $"Seeking {PercentDuration}%" ) );
            double p = 0;
            if ( double.TryParse( PercentDuration, out p ) )
                await this._Service.SeekAudio( p );
        }

        [Command( "pause", RunMode = RunMode.Async )]
        public async Task PauseCmd() => await this._Service.PauseAudio();

        
        [Command( "playlist", RunMode = RunMode.Async )]
        [Alias( "pl" )]
        public async Task PlaylistCmd( [Remainder] string PlaylistLink ) {
            await this._Service.LeaveAudio( this.Context.Guild );
            await this._Service.JoinAudio( this.Context.Guild, ( this.Context.User as IVoiceState ).VoiceChannel );

            var YTC = new YoutubeClient();

            var PlayListInfo = await YTC.GetPlaylistAsync( YoutubeClient.ParsePlaylistId( PlaylistLink ) );

            var IDArray = PlayListInfo.Videos.ToArray();

            foreach ( var ID in IDArray ) await this._Service.SendAudioAsync( this.Context.Guild, ID );
            await this._Service.LeaveAudio( this.Context.Guild );
        }

        [Command( "queue", RunMode = RunMode.Async )]
        [Alias( "q" )]
        public async Task QueueSong( [Remainder] string LinkOrSearchTerm ) {
            Queue.Add( LinkOrSearchTerm );
            await Log( new LogMessage( LogSeverity.Info, "Queue", $"{LinkOrSearchTerm} added to queue" ) );
            this.Reply = Queue.Count == 1 ? "There is 1 song in the queue." : $"There are {Queue.Count} song in the queue.";
            await this.ReplyAsync( $"{LinkOrSearchTerm} added.\n{this.Reply}" );
        }

        [Command( "clearque", RunMode = RunMode.Async )]
        [Alias( "clearq" )]
        public async Task ClearQue() {
            Queue.Clear();
            await this.ReplyAsync( "Queue has been cleared" );
        }

        [Command( "playqueue", RunMode = RunMode.Async )]
        [Alias( "playq" )]
        public async Task PlayQueue() {
            await this._Service.LeaveAudio( this.Context.Guild );
            await this._Service.JoinAudio( this.Context.Guild, ( this.Context.User as IVoiceState ).VoiceChannel );
            while ( Queue.Count > 0 ) {

                this.NextSong = Queue.Count != 1 ? $", next song {Queue.ElementAt( 1 )}" : "";
                this.LeftInQueue = Queue.Count == 1 ? "There is 1 song in the queue." : $"There are {Queue.Count} song in the queue.";
                await this.ReplyAsync( $"Now Playing ID:{Queue.First()}{this.NextSong}.\n{this.LeftInQueue}" );
                await Log( new LogMessage( LogSeverity.Info, "Queue", $"Now Playing: {Queue.First()}{this.NextSong}.\n{this.LeftInQueue}" ) );

                await this._Service.SendAudioAsync( this.Context.Guild, Queue.First() );
                Queue.RemoveAt( 0 );
            }

            await this.ReplyAsync( "Sorry, the queue is empty, !queue (or !q) to add more!" );
            await Log( new LogMessage( LogSeverity.Info, "Queue", "Queue is empty" ) );

            await this._Service.LeaveAudio( this.Context.Guild );
            await Log( new LogMessage( LogSeverity.Info, "Queue", $"Bot is leaving {this.Context.Channel}" ) );
        }

        [Command( "removeat", RunMode = RunMode.Async )] //TODO: (add range ability?)
        public async Task RemoveNext( [Remainder] int Index ) {
            String Msg = $"Removed item {Queue.ElementAt( Index )} at index {Index}";
            Queue.RemoveAt( Index );
            await this.ReplyAsync( Msg );
            await Log( new LogMessage( LogSeverity.Info, "Queue", Msg ) );
        }

        [Command( "skip", RunMode = RunMode.Async )]
        [Alias("stop")]
        public async Task Skip( ) {
            await SeekCmd("100");
            await this.ReplyAsync( "Skipped Song" );
            await Log( new LogMessage( LogSeverity.Info, "Queue", "Skipped Song" ) );
        }
        

        //*********************************************************************************************************
        //TODO: Spotify support

        [Command( "Spotify", RunMode = RunMode.Async )]
        [Alias( "spot" )]
        public async Task Spotify() {
            await this.ReplyAsync( "Spotify command triggered." );
        }




        //TODO: Overhaul Queue - Insert, List queue, view at index (range?), Play next song
        //TODO: Display song length at start of song

        public static Task Log( LogMessage msg ) {
            Console.WriteLine( msg.ToString() );
            return Task.CompletedTask;
        }
    }
}
