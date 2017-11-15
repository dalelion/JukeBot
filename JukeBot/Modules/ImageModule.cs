using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using Discord;
using Discord.Commands;
using JukeBot.Services;

namespace JukeBot.Modules {
    class ImageModule : ModuleBase<ICommandContext> {

        private readonly ImageService _Service;

        public ImageModule( ImageService Service ) {
            _Service = Service;
        }

        [Command( "jpg", RunMode = RunMode.Async )]
        [Summary( "Searches Google Images for the first image that matches" )]
        public async Task JPG( String SearchTerm ) {

            await _Service.GoogleImageSearch( Context.Guild, Context.Channel, SearchTerm );

        }

        [Command("gif", RunMode = RunMode.Async)]
        [Alias("g")]
        [Summary("Searches Tenor for the first gif that matches")]
        public async Task GIF( String SearchTerm ) {

            await _Service.TenorSearch( Context.Guild, Context.Channel, SearchTerm );
        }

    }
}