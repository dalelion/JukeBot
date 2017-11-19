using System;
using System.IO;
using System.Linq;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using Discord;
using Discord.Audio;
using YoutubeExplode;
using YoutubeExplode.Models;

namespace JukeBot.Services {
    public class AudioService {
        private MemoryStream AudioData = new MemoryStream();
        private bool Pause;
        private readonly ConcurrentDictionary<ulong, IAudioClient> ConnectedChannels = new ConcurrentDictionary<ulong, IAudioClient>();

        public async Task JoinAudio( IGuild Guild, IVoiceChannel Target ) {
            IAudioClient Client;

            if ( this.ConnectedChannels.TryGetValue( Guild.Id, out Client ) ) return;
            if ( Target.Guild.Id != Guild.Id ) return;

            var AudioClient = await Target.ConnectAsync();

            if ( this.ConnectedChannels.TryAdd( Guild.Id, AudioClient ) ) { }
        }

        public async Task SeekAudio( double Percent ) {
            var val = this.Pause;
            this.Pause = true;
            Percent = Math.Max( 0, Math.Min( 100, Percent ) ) / 100;
            if ( this.AudioData.Length > 0 )
                await Task.Run( () => {
                    var Position = (long) ( Percent * this.AudioData.Length );
                    Position -= Position % 8192;
                    this.AudioData.Seek( Position, SeekOrigin.Begin );
                    this.Pause = val;
                } );
        }

        public async Task PauseAudio() => await Task.Run( () => { this.Pause = !this.Pause; } );

        public async Task LeaveAudio( IGuild Guild ) {
            IAudioClient Client;
            if ( this.ConnectedChannels.Count > 0 && this.ConnectedChannels.TryRemove( Guild.Id, out Client ) ) await Client.StopAsync();
        }

        public async Task SendAudioAsync( IGuild Guild, string UserInput ) {
            var YTC = new YoutubeClient();

            if ( UserInput.ToLower().Contains( "youtube.com" ) ) {
                UserInput = YoutubeClient.ParseVideoId( UserInput );
            } else {
                var SearchList = await YTC.SearchAsync( UserInput );
                UserInput = SearchList.First();
            }

            var VideoInfo = await YTC.GetVideoInfoAsync( UserInput );

            var ASI = VideoInfo.AudioStreams.OrderBy( x => x.Bitrate ).Last();

            var Title = VideoInfo.Title;

            var RGX = new Regex( "[^a-zA-Z0-9 -]" );
            Title = RGX.Replace( Title, "" );

            var Name = $"{Title}.{ASI.Container.GetFileExtension()}";
#if DEBUG
            var Path = "bin/Debug/netcoreapp1.1/Songs/";
#else
            String Path = "Songs/";
#endif
            if ( !File.Exists( Path + Name ) )
                using ( var Input = await YTC.GetMediaStreamAsync( ASI ) ) {
                    Directory.CreateDirectory( Path );
                    using ( var Out = File.Create( Path + Name ) ) {
                        await Input.CopyToAsync( Out );
                    }
                }

            IAudioClient AudioClient;

            await JukeBot.DiscordClient.SetGameAsync( Title );

            if ( this.ConnectedChannels.TryGetValue( Guild.Id, out AudioClient ) ) {
                var Output = this.CreateStream( Path + Name ).StandardOutput.BaseStream;
                await this.AudioData.FlushAsync();
                await Output.CopyToAsync( this.AudioData );
                await Output.FlushAsync();
                Output.Dispose();
                int read_length;
                var buffer = new byte[8192];
                this.AudioData.Seek( 0x0, SeekOrigin.Begin );
                var DiscordStream = AudioClient.CreatePCMStream( AudioApplication.Music, 2880 );
                while ( this.AudioData.Position < this.AudioData.Length )
                    if ( !this.Pause ) {
                        read_length = await this.AudioData.ReadAsync( buffer, 0, 8192 );
                        await DiscordStream.WriteAsync( buffer, 0, read_length );
                    } else {
                        await DiscordStream.WriteAsync( new byte[512], 0, 512 );
                    }
                await this.AudioData.FlushAsync();
                //await Output.CopyToAsync(DiscordStream);
                await DiscordStream.FlushAsync();
                await JukeBot.DiscordClient.SetGameAsync( "" );
            }
        }

        private Process CreateStream( string Path ) {
#if DEBUG
            return Process.Start( new ProcessStartInfo {
                FileName = @"bin/Debug/netcoreapp1.1/Resources/ffmpeg.exe",
                Arguments = $"-hide_banner -loglevel panic -i \"{Path}\" -ac 2 -f s16le -ar 48000 pipe:1",
                UseShellExecute = false,
                RedirectStandardOutput = true
            } );
#else
            return Process.Start( new ProcessStartInfo {
                FileName = "Resources/ffmpeg.exe",
                Arguments = $"-hide_banner -loglevel panic -i \"{Path}\" -ac 2 -f s16le -ar 48000 pipe:1",
                UseShellExecute = false,
                RedirectStandardOutput = true
            } );
#endif
        }

        public static Task Log( LogMessage Message ) {
            Console.WriteLine( Message.ToString() );
            return Task.CompletedTask;
        }
    }
}
