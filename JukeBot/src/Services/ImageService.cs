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
            //TODO: Num has no purpose right now, add implementation to get the X number image result
            //Can jus tbe A loop that catches errors and tries again with the next index
            //OR
            //User can input a parameter for the number image they want (ex. feelsbadman.jpg #4)

            String Key = "AIzaSyCimF2sJLwecmssiwDg8Jdb-f4NAgv-RBk", CX = "001762016596902704548:ugobeno8bsc", Content;

            HttpResponseMessage Response = await new HttpClient().GetAsync( $"https://www.googleapis.com/customsearch/v1?q={Query.Replace( ' ', '+' )}&key={Key}&cx={CX}&searchtype=image&num={Num}" );

            //HttpResponseMessage R = await new HttpClient().GetAsync( "https://api.cognitive.microsoft.com/bing/v5.0/images/search" );

            //await new HttpMessageInvoker().SendAsync(new HttpRequestMessage().Headers.Add("Ocp-Apim-Subscription-Key", "93d947f2eeab4734a60a9d23ee5df4c0" ) )

            Content = Response.Content.ReadAsStringAsync().Result;

            return Content.Substring( Content.LastIndexOf( "\"src\":" ) ).Split( '\"' )[3];
        }

        public async Task<String> TenorSearch( String Query ) {
            HttpResponseMessage Response = await new HttpClient().GetAsync( $"http://api.tenor.com/v1/search?q={Query}&key=12GNQGC43ETG" );

            String Content = Response.Content.ReadAsStringAsync().Result;

            return Content.Substring( Content.IndexOf( "\"url\":" ) ).Split( '\"' )[3];
        }
    }
}
