using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using Amazon.DynamoDBv2.DataModel;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace fetchquote
{
    public class Function
    {
        
        /// <summary>
        /// A simple function that takes a string and does a ToUpper
        /// </summary>
        /// <param name="input"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public QuoteItem FunctionHandler(ILambdaContext context)
        {
            var mytask = Function.GetRandomQuote();
            mytask.Wait();
            QuoteItem q = mytask.Result;
            Console.WriteLine($"Author: {q.Author}\nQuote: {q.Quote}");
            return q;
        }

        public static async Task<QuoteItem> GetRandomQuote()
        {
            var client = new AmazonDynamoDBClient();
            DynamoDBContext dbcontext = new DynamoDBContext(client);
            Random rn = new Random();
            QuoteItem q = await dbcontext.LoadAsync<QuoteItem>(rn.Next(20));
            return q;        
            
        }
    }
    [DynamoDBTable("myquotes")]
    public class QuoteItem
    {
        [DynamoDBHashKey]   
        public int id { get; set; }

        [DynamoDBProperty("author")]    
        public string Author { get; set; }

        [DynamoDBProperty("category")]    
        public string Category { get; set; }

        [DynamoDBProperty("quote")]    
        public string Quote { get; set; }
   
    }
}
