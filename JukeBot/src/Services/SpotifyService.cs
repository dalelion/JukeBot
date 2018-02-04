using System;
using System.Collections.Generic;
using System.Text;
using FluentSpotifyApi.Core.Client;
using FluentSpotifyApi;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace JukeBot.src.Services {
    class SpotifyService {

        public async Task Spotify() {
            

            HttpClientWrapper hcw = new HttpClientWrapper( new HttpClient() );

            //hcw.SendAsync()

            FluentSpotifyApi.Client.SpotifyHttpClient c = new FluentSpotifyApi.Client.SpotifyHttpClient( hcw );
            
            //c.SendAsync()

            FluentSpotifyApi.Builder.Search.QueryFields query = new FluentSpotifyApi.Builder.Search.QueryFields();

            

        }
        
    }
}
