using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using Discord;

namespace JukeBot.Services {
    class ImageService {

        public async Task GoogleImageSearch( IGuild guild, IMessageChannel channel, String Query, int Num = 1 ) {

            String Key = "AIzaSyCimF2sJLwecmssiwDg8Jdb-f4NAgv-RBk", CX = "001762016596902704548:ugobeno8bsc", S = "";
            
            HttpResponseMessage Response = await new HttpClient().GetAsync( $"www.googleapis.com/customsearch/v1?q={Query.Replace(' ', '+')}&key={Key}&cx={CX}&searchtype=image&num={Num}" );
            
            S = Response.Content.ToString().Substring( S.LastIndexOf( "\"scr:\"" ) ).Split( '\"' )[3];

        }

        public async Task TenorSearch( IGuild Guild, IMessageChannel Channel, String Query ) {

            String S = "";

            HttpResponseMessage Response = await new HttpClient().GetAsync($"api.tenor.com/v1/search?q={Query}&key=12GNQGC43ETG" );
            
            S = Response.Content.ToString().Substring( S.IndexOf( "\"url\":" ) ).Split( '\"' )[3];

        }

    }
}
