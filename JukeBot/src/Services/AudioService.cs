using System;
using System.IO;
using System.Net;
using System.Linq;
using System.Net.Http;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using Discord;
using Discord.Audio;
using YoutubeExplode;
using YoutubeExplode.Models;
using System.Xml.Linq;
using Newtonsoft.Json;

namespace JukeBot.Services {
    public class AudioService {
        private MemoryStream AudioData;
        private bool Pause;
        private readonly ConcurrentDictionary<ulong, IAudioClient> ConnectedChannels = new ConcurrentDictionary<ulong, IAudioClient>();

        public async Task JoinAudio( IGuild Guild, IVoiceChannel Target ) {
            IAudioClient Client;

            if ( this.ConnectedChannels.TryGetValue( Guild.Id, out Client ) )
                return;
            if ( Target.Guild.Id != Guild.Id )
                return;

            var AudioClient = await Target.ConnectAsync();

            if ( this.ConnectedChannels.TryAdd( Guild.Id, AudioClient ) ) { }
        }

        public async Task SeekAudio( double Percent ) {
            var val = this.Pause;
            this.Pause = true;
            Percent = Math.Max( 0, Math.Min( 100, Percent ) ) / 100;
            if ( this.AudioData.Length > 0 )
                await Task.Run( () => {
                    var Position = ( long )( Percent * this.AudioData.Length );
                    Position -= Position % 8192;
                    this.AudioData.Seek( Position, SeekOrigin.Begin );
                    this.Pause = val;
                } );
        }

        public async Task PauseAudio() => await Task.Run( () => { this.Pause = !this.Pause; } );

        public async Task LeaveAudio( IGuild Guild ) {
            IAudioClient Client;
            if ( this.ConnectedChannels.Count > 0 && this.ConnectedChannels.TryRemove( Guild.Id, out Client ) )
                await Client.StopAsync();
        }

        public async Task SendAudioAsync( IGuild Guild, string UserInput ) {
            var YTC = new YoutubeClient();

            if ( UserInput.ToLower().Contains( "youtube.com" ) ) {
                UserInput = YoutubeClient.ParseVideoId( UserInput );
            } else {
                //var SearchList = await YTC.SearchAsync( UserInput );
                
                HttpClient _httpClient = new HttpClient();

                string EncodedSearchQuery = WebUtility.UrlEncode( UserInput );

                string Request = $"https://www.youtube.com/search_ajax?style=xml&search_query={EncodedSearchQuery}";

                var Response = await _httpClient.GetStringAsync( Request ).ConfigureAwait( false );

                var SearchResultsXml = XElement.Parse( Response ).StripNamespaces();

                var VideoIds = SearchResultsXml.Descendants( "encrypted_id" ).Select( e => ( string )e );

                UserInput = VideoIds.First();
            }
            
            var MediaInfo = await YTC.GetVideoMediaStreamInfosAsync( UserInput );
            
            var ASI = MediaInfo.Audio.OrderBy( x => x.Bitrate ).Last();

            var VideoInfo = await YTC.GetVideoAsync( UserInput );

            var Title = VideoInfo.Title; //VideoInfo.ToString();            ;
            
            var RGX = new Regex( "[^a-zA-Z0-9 -]" );
            Title = RGX.Replace( Title, "" );

            var Name = $"{Title}.{ASI.AudioEncoding.ToString()}";
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
                this.AudioData = new MemoryStream();
                await Output.CopyToAsync( this.AudioData );
                await Output.FlushAsync();
                Output.Dispose();
                int read_length = 0;
                bool flipflop = false;
                int buffer_size = 2048;
                var buffer = new[] { new byte[buffer_size], new byte[buffer_size] };
                this.AudioData.Seek( 0x0, SeekOrigin.Begin );
                var DiscordStream = AudioClient.CreatePCMStream( AudioApplication.Music, 2880 );
                Task writer;
                Task<int> reader;
                while ( this.AudioData.Position < this.AudioData.Length )
                    if ( !this.Pause ) {
                        writer = DiscordStream.WriteAsync( buffer[flipflop ? 0 : 1], 0, read_length );
                        flipflop = !flipflop;
                        reader = this.AudioData.ReadAsync( buffer[flipflop ? 0 : 1], 0, buffer_size );
                        read_length = await reader;
                        await writer;
                    } else {
                        await DiscordStream.WriteAsync( new byte[512], 0, 512 );
                        read_length = 0;
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

        private async Task StreamAudio(IGuild Guild, String Filename) {

            Console.WriteLine("Streaming");

            IAudioClient AudioClient;

            await JukeBot.DiscordClient.SetGameAsync( Filename );

            if ( this.ConnectedChannels.TryGetValue( Guild.Id, out AudioClient ) ) {
                var Output = this.CreateStream( "4chan/" + Filename ).StandardOutput.BaseStream;
                this.AudioData = new MemoryStream();
                await Output.CopyToAsync( this.AudioData );
                await Output.FlushAsync();
                Output.Dispose();
                int read_length = 0;
                bool flipflop = false;
                int buffer_size = 2048;
                var buffer = new[] { new byte[buffer_size], new byte[buffer_size] };
                this.AudioData.Seek( 0x0, SeekOrigin.Begin );
                var DiscordStream = AudioClient.CreatePCMStream( AudioApplication.Music, 2880 );
                Task writer;
                Task<int> reader;
                while ( this.AudioData.Position < this.AudioData.Length )
                    if ( !this.Pause ) {
                        writer = DiscordStream.WriteAsync( buffer[flipflop ? 0 : 1], 0, read_length );
                        flipflop = !flipflop;
                        reader = this.AudioData.ReadAsync( buffer[flipflop ? 0 : 1], 0, buffer_size );
                        read_length = await reader;
                        await writer;
                    } else {
                        await DiscordStream.WriteAsync( new byte[512], 0, 512 );
                        read_length = 0;
                    }
                await this.AudioData.FlushAsync();
                //await Output.CopyToAsync(DiscordStream);
                await DiscordStream.FlushAsync();
                await JukeBot.DiscordClient.SetGameAsync( "" );
            }


        }

        
        public async Task SendWEBMAudioAsync( IGuild Guild, string UserInput ) {

            //http://boards.4chan.org/{BOARD}/thread/{THREAD#}
            //http://a.4cdn.org/{BOARD}/thread/{THREAD#}.json

            Regex RGX = new Regex( @"[a-zA-Z]+/[a-zA-Z]+/[0-9]+" );

            String Board = "" + RGX.Match( UserInput );
            Board = Board.Split('/' )[0];

            String JSONLink = "http://a.4cdn.org/" + RGX.Match(UserInput) + ".json", FileName;
            
            HttpClient HTTPC = new HttpClient();

            var Response = await HTTPC.GetAsync( JSONLink );
            var JSON = JsonConvert.DeserializeObject<nig>( await Response.Content.ReadAsStringAsync() );
            foreach ( var Post in JSON.Posts ) {
                if ( Post.ext == ".webm" ) {
                    
                    FileName = Post.filename + Post.ext;

                    DownloadFile( $"http://i.4cdn.org/{Board}/{Post.tim}.webm", "4chan/" + FileName ).WaitForExit();

                    await StreamAudio( Guild, FileName );

                }
            }

        }

        private Process DownloadFile(String url, String FileName) {

            return Process.Start( new ProcessStartInfo {
                FileName = "FileDownloader.exe",
                Arguments = $"{url} {FileName}"
            } );

        }


        public static Task Log( LogMessage Message ) {
            Console.WriteLine( Message.ToString() );
            return Task.CompletedTask;
        }
    }


    public static class Extensions {
        public static XElement StripNamespaces( this XElement element ) {
            // Original code credit: http://stackoverflow.com/a/1147012

            var result = new XElement( element );
            foreach ( var e in result.DescendantsAndSelf() ) {
                e.Name = XNamespace.None.GetName( e.Name.LocalName );
                var attributes = e.Attributes()
                    .Where( a => !a.IsNamespaceDeclaration )
                    .Where( a => a.Name.Namespace != XNamespace.Xml && a.Name.Namespace != XNamespace.Xmlns )
                    .Select( a => new XAttribute( XNamespace.None.GetName( a.Name.LocalName ), a.Value ) );
                e.ReplaceAttributes( attributes );
            }

            return result;
        }
    }

    class nig {
        [JsonProperty( "posts" )]
        public System.Collections.Generic.List<nog> Posts { get; set; }
    }
    class nog {
        public Int64 tim { get; set; }
        public string ext { get; set; }
        public string filename { get; set; }
    }


}
