using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Discord;
using Discord.Audio;
using YoutubeExplode;
using YoutubeExplode.Models;
using YoutubeExplode.Models.MediaStreams;
using System.Linq;
using System;
using System.Text.RegularExpressions;

public class AudioService {

    public static Task SongPlaying;

    private readonly ConcurrentDictionary<ulong, IAudioClient> ConnectedChannels = new ConcurrentDictionary<ulong, IAudioClient>();

    public async Task JoinAudio (IGuild guild, IVoiceChannel target) {
        IAudioClient client;
        if (ConnectedChannels.TryGetValue(guild.Id, out client)) {
            return;
        }
        if (target.Guild.Id != guild.Id) {
            return;
        }

        var audioClient = await target.ConnectAsync();

        if (ConnectedChannels.TryAdd(guild.Id, audioClient)) {
        }
    }

    public async Task LeaveAudio (IGuild guild) {
        IAudioClient client;
        if (ConnectedChannels.TryRemove(guild.Id, out client)) {
            await client.StopAsync();
        }
    }

    public async Task SendAudioAsync (IGuild guild, IMessageChannel channel, string UserInput) {

        YoutubeClient YTC = new YoutubeClient();
        
        if (!UserInput.Contains("youtube.com")) {
            var IDList = await YTC.SearchAsync(UserInput);
            UserInput = IDList.First();
        }

        VideoInfo VideoInfo = await YTC.GetVideoInfoAsync(UserInput);

        AudioStreamInfo ASI = VideoInfo.AudioStreams.OrderBy(x => x.Bitrate).Last();

        String Title = VideoInfo.Title;

        Regex RGX = new Regex("[^a-zA-Z0-9 -]");
        Title = RGX.Replace(Title, "");

        String Path = "E:/JukeBot/Songs/" + $"{Title}.{ASI.Container.GetFileExtension()}";

        using (var Input = await YTC.GetMediaStreamAsync(ASI))
        using (var Out = File.Create(Path))
            await Input.CopyToAsync(Out);

        IAudioClient client;

        await Program.DiscordClient.SetGameAsync(Title);

        if (ConnectedChannels.TryGetValue(guild.Id, out client)) {

            var Output = CreateStream(Path).StandardOutput.BaseStream;
            var Stream = client.CreatePCMStream(AudioApplication.Music, 2880);
            await Output.CopyToAsync(Stream);
            await Stream.FlushAsync();
            await Program.DiscordClient.SetGameAsync("");
        }
    }

    private Process CreateStream (string path) {
        return Process.Start(new ProcessStartInfo {
            FileName = "ffmpeg.exe",
            Arguments = $"-hide_banner -loglevel panic -i \"{path}\" -ac 2 -f s16le -ar 48000 pipe:1",
            UseShellExecute = false,
            RedirectStandardOutput = true
        });
    }
}