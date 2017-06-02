using System.Threading.Tasks;
using Discord.Commands;
using Discord;
using System;
using YoutubeExplode.Models;
using System.Linq;
using System.Collections.Generic;
using YoutubeExplode;

public class AudioModule : ModuleBase<ICommandContext> {

    private readonly AudioService _service;

    static List<String> Queue = new List<string>();
    String Reply, NextSong, LeftInQueue;

    public AudioModule (AudioService service) {
        _service = service;
    }

    // You *MUST* mark these commands with 'RunMode.Async'
    // otherwise the bot will not respond until the Task times out.
    [Command("join", RunMode = RunMode.Async)]
    [Alias("j")]
    public async Task JoinCmd () {
        await Log(new LogMessage(LogSeverity.Info, "Queue", $"Bot is joining {Context.Channel}"));
        await _service.JoinAudio(Context.Guild, (Context.User as IVoiceState).VoiceChannel);
    }

    [Command("leave", RunMode = RunMode.Async)]
    [Alias("l")]
    public async Task LeaveCmd () {
        await _service.LeaveAudio(Context.Guild);
        await Log(new LogMessage(LogSeverity.Info, "Leave", $"Bot is leaving {Context.Channel}"));
    }

    [Command("play", RunMode = RunMode.Async)]
    [Alias("p")]
    public async Task PlayCmd ([Remainder] string LinkOrSearchTerm) {
        await _service.LeaveAudio(Context.Guild);
        await _service.JoinAudio(Context.Guild, (Context.User as IVoiceState).VoiceChannel);
        await _service.SendAudioAsync(Context.Guild, Context.Channel, LinkOrSearchTerm);
        await _service.LeaveAudio(Context.Guild);
        await Log(new LogMessage(LogSeverity.Info, "Play", $"Bot is leaving {Context.Channel}"));
    }

    [Command("playlist", RunMode = RunMode.Async)]
    [Alias("pl")]
    public async Task PlaylistCmd ([Remainder] string PlaylistLink) {
        await _service.LeaveAudio(Context.Guild);
        await _service.JoinAudio(Context.Guild, (Context.User as IVoiceState).VoiceChannel);

        YoutubeClient YTC = new YoutubeClient();

        PlaylistInfo PlayListInfo = await YTC.GetPlaylistInfoAsync(YoutubeClient.ParsePlaylistId(PlaylistLink));

        String[] IDArray = PlayListInfo.VideoIds.ToArray();

        foreach (String ID in IDArray) {
            await _service.SendAudioAsync(Context.Guild, Context.Channel, ID);
        }
        await _service.LeaveAudio(Context.Guild);
    }

    [Command("queue", RunMode = RunMode.Async)]
    [Alias("q")]
    public async Task QueueSong ([Remainder] string LinkOrSearchTerm) {
        Queue.Add(LinkOrSearchTerm);
        await Log(new LogMessage(LogSeverity.Info, "Queue", $"{LinkOrSearchTerm} added to queue"));
        Reply = Queue.Count == 1 ? "There is 1 song in the queue." : $"There are {Queue.Count} song in the queue.";
        await ReplyAsync($"{LinkOrSearchTerm} added.\n{Reply}");
    }

    [Command("clearque", RunMode = RunMode.Async)]
    [Alias("clearq")]
    public async Task ClearQue () {
        Queue.Clear();
        await ReplyAsync("Queue has been cleared");
    }

    [Command("playqueue", RunMode = RunMode.Async)]
    [Alias("playq")]
    public async Task PlayQueue () {

        while (Queue.Count > 0) {

            await _service.LeaveAudio(Context.Guild);
            await _service.JoinAudio(Context.Guild, (Context.User as IVoiceState).VoiceChannel);

            NextSong = Queue.Count != 1 ? $", next song {Queue.ElementAt(1)}" : "";
            LeftInQueue = Queue.Count == 1 ? "There is 1 song in the queue." : $"There are {Queue.Count} song in the queue.";
            await ReplyAsync($"Now Playing ID:{Queue.First()}{NextSong}.\n{LeftInQueue}");
            await Log(new LogMessage(LogSeverity.Info, "Queue", $"Now Playing: {Queue.First()}{NextSong}.\n{LeftInQueue}"));

            await _service.SendAudioAsync(Context.Guild, Context.Channel, Queue.First());
            Queue.RemoveAt(0);

        }

        await ReplyAsync("Sorry, the queue is empty, !queue (or !q) to add more!");
        await Log(new LogMessage(LogSeverity.Info, "Queue", "Que is empty"));

        await _service.LeaveAudio(Context.Guild);
        await Log(new LogMessage(LogSeverity.Info, "Queue", $"Bot is leaving {Context.Channel}"));
    }
    
    public static Task Log (LogMessage msg) {
        Console.WriteLine(msg.ToString());
        return Task.CompletedTask;
    }

}