using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http.Json;
using System.Net.Http;
using System.Web;
using System.Net.Http.Headers;
using Amazon.Lambda.Core;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace spotify
{
    public class Function
    {
        
        /// <summary>
        /// A simple function that takes a string and does a ToUpper
        /// </summary>
        /// <param name="input"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public void FunctionHandler(ILambdaContext context)
        {
            
            var artists = GetArtists("Tupac");
            foreach (var artist in artists)
            {
                System.Console.WriteLine(artist.name);
            }
  

        }

        public static KeyValuePair<string, string> GetSpotifyToken()
        {
            string Client_ID = Environment.GetEnvironmentVariable("clientid");
            string Client_Secret = Environment.GetEnvironmentVariable("clientsecret");
            String encoded = System.Convert.ToBase64String(System.Text.Encoding.GetEncoding("ISO-8859-1").GetBytes(Client_ID + ":" + Client_Secret));
            HttpClient client = new HttpClient();
            var url = new Uri("https://accounts.spotify.com/api/token");
            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("grant_type", "client_credentials")
            });
            client.DefaultRequestHeaders.Add("Authorization", "Basic " + encoded);
            HttpResponseMessage response = client.PostAsync(url, content).Result;
            ResponseToken token = response.Content.ReadFromJsonAsync<ResponseToken>().Result;
            return new KeyValuePair<string, string>("Authorization", $"Bearer {token.Access_Token}");
        }

        public static IList<Item> GetArtists(string artist)
        {
            KeyValuePair<string, string> TokenHeader = GetSpotifyToken();
            byte[] artistbytes = Encoding.ASCII.GetBytes(artist);
            var artistencoded = HttpUtility.UrlEncode(artistbytes);
            var url = new Uri($"https://api.spotify.com/v1/search?q={artistencoded}&type=artist");
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add(TokenHeader.Key, TokenHeader.Value);
            HttpResponseMessage response = client.GetAsync(url).Result;
            ArtistsResponse res = response.Content.ReadFromJsonAsync<ArtistsResponse>().Result;
            return res.artists.items;
            
        }
    }

    class ResponseToken
    {
        public string Access_Token { get; set; }
        public string Token_Type { get; set; }
        public int Expires_In { get; set; }
        public string Scope { get; set; }
    }

    public class ExternalUrls
    {
        public string spotify { get; set; }
    }

    public class Followers
    {
        public object href { get; set; }
        public int total { get; set; }
    }

    public class Image
    {
        public int height { get; set; }
        public string url { get; set; }
        public int width { get; set; }
    }

    public class Item
    {
        public ExternalUrls external_urls { get; set; }
        public Followers followers { get; set; }
        public IList<string> genres { get; set; }
        public string href { get; set; }
        public string id { get; set; }
        public IList<Image> images { get; set; }
        public string name { get; set; }
        public int popularity { get; set; }
        public string type { get; set; }
        public string uri { get; set; }
    }

    public class Artists
    {
        public string href { get; set; }
        public IList<Item> items { get; set; }
        public int limit { get; set; }
        public object next { get; set; }
        public int offset { get; set; }
        public object previous { get; set; }
        public int total { get; set; }
    }

    public class ArtistsResponse
    {
        public Artists artists { get; set; }
    }

    


}
