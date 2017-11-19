using System;
using System.IO;
using System.Linq;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using Discord;
using Discord.Audio;
using YoutubeExplode;
using YoutubeExplode.Models;
using YoutubeExplode.Models.MediaStreams;

namespace JukeBot.Services {
    public class AudioService {
        private readonly ConcurrentDictionary<ulong, IAudioClient> ConnectedChannels = new ConcurrentDictionary<ulong, IAudioClient>();

        public async Task JoinAudio( IGuild Guild, IVoiceChannel Target ) {
            IAudioClient Client;

            if ( ConnectedChannels.TryGetValue( Guild.Id, out Client ) ) {
                return;
            }
            if ( Target.Guild.Id != Guild.Id ) {
                return;
            }

            var AudioClient = await Target.ConnectAsync();

            if ( ConnectedChannels.TryAdd( Guild.Id, AudioClient ) ) { }
        }

        public async Task LeaveAudio( IGuild Guild ) {
            IAudioClient Client;
            if ( ConnectedChannels.TryRemove( Guild.Id, out Client ) ) {
                await Client.StopAsync();
            }
        }

        public async Task SendAudioAsync( IGuild Guild, String UserInput ) {
            YoutubeClient YTC = new YoutubeClient();

            if ( UserInput.ToLower().Contains( "youtube.com" ) ) {
                UserInput = YoutubeClient.ParseVideoId( UserInput );
            } else {
                IEnumerable<String> SearchList = await YTC.SearchAsync( UserInput );
                UserInput = SearchList.First();
            }

            VideoInfo VideoInfo = await YTC.GetVideoInfoAsync( UserInput );

            AudioStreamInfo ASI = VideoInfo.AudioStreams.OrderBy( x => x.Bitrate ).Last();

            String Title = VideoInfo.Title;

            Regex RGX = new Regex( "[^a-zA-Z0-9 -]" );
            Title = RGX.Replace( Title, "" );

            String Path = "Songs/" + $"{Title}.{ASI.Container.GetFileExtension()}";

            using ( var Input = await YTC.GetMediaStreamAsync( ASI ) )
                using ( var Out = File.Create( Path ) )
                    await Input.CopyToAsync( Out );

            IAudioClient AudioClient;

            await Program.DiscordClient.SetGameAsync( Title );

            if ( ConnectedChannels.TryGetValue( Guild.Id, out AudioClient ) ) {
                var Output = CreateStream( Path ).StandardOutput.BaseStream;
                var DiscordStream = AudioClient.CreatePCMStream( AudioApplication.Music, 2880 );
                await Output.CopyToAsync( DiscordStream );
                await DiscordStream.FlushAsync();
                await Program.DiscordClient.SetGameAsync( "" );
            }
        }

        private Process CreateStream( String Path ) {
            return Process.Start( new ProcessStartInfo {
                FileName = "/Resources/ffmpeg.exe",
                Arguments = $"-hide_banner -loglevel panic -i \"{Path}\" -ac 2 -f s16le -ar 48000 pipe:1",
                UseShellExecute = false,
                RedirectStandardOutput = true
            } );
        }

        public static Task Log( LogMessage Message ) {
            Console.WriteLine( Message.ToString() );
            return Task.CompletedTask;
        }
    }
}
