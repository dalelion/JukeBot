using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using Discord;

namespace JukeBot.Services {
    public class ImageService {

        public async Task<String> GoogleImageSearch( String Query, int Num = 1 ) {

            String Key = "AIzaSyCimF2sJLwecmssiwDg8Jdb-f4NAgv-RBk", CX = "001762016596902704548:ugobeno8bsc", S;
            
            HttpResponseMessage Response = await new HttpClient().GetAsync( $"https://www.googleapis.com/customsearch/v1?q={Query.Replace(' ', '+')}&key={Key}&cx={CX}&searchtype=image&num={Num}" );

            S = Response.Content.ReadAsStringAsync().Result;

            return S.Substring( S.LastIndexOf( "\"src\":" ) ).Split( '\"' )[3];

        }

        public async Task<String> TenorSearch( String Query ) {
            
            HttpResponseMessage Response = await new HttpClient().GetAsync($"http://api.tenor.com/v1/search?q={Query}&key=12GNQGC43ETG" );
            
            String S = Response.Content.ReadAsStringAsync().Result;

            return S.Substring( S.IndexOf( "\"url\":" ) ).Split( '\"' )[3];

        }

    }
}
