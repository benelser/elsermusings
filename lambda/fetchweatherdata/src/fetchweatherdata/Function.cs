using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Amazon.Lambda.Core;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using Amazon.DynamoDBv2.DataModel;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace fetchweatherdata
{
    public class Function
    {
        
        /// <summary>
        /// Function fetches data from dynamodb
        /// </summary>
        /// <param name="input"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public WeatherRecord[] FunctionHandler(ILambdaContext context)
        {
            var mytask = Function.ReadWeatherTable();
            mytask.Wait();
            
            Console.WriteLine($"Count: {mytask.Result.Length}");
            foreach (WeatherRecord r in mytask.Result)
            {
                Console.WriteLine($"Date: {r.Date}\nTemp. (F): {r.TemperatureF}");
            }
            
            
            WeatherRecord[] records = (WeatherRecord[])mytask.Result;
            // LambdaLogger.Log("EVENT: " + );
            return records;

        }

        public static async Task<WeatherRecord[]> ReadWeatherTable()
        {
            var client = new AmazonDynamoDBClient();
            DynamoDBContext dbcontext = new DynamoDBContext(client);
            var search = dbcontext.FromScanAsync<WeatherRecord>(new Amazon.DynamoDBv2.DocumentModel.ScanOperationConfig() {
            ConsistentRead = true
            });
            
            var searchResponse = await search.GetRemainingAsync();
            // Console.WriteLine($"Count: {searchResponse.ToArray().Length}");
            // foreach (WeatherRecord r in searchResponse.ToArray())
            // {
            //     Console.WriteLine($"Date: {r.Date}\nSummary: {r.Summary}");
            // }
            
            return searchResponse.ToArray();
        }

    }

    

    [DynamoDBTable("weatherdata")]
    public class WeatherRecord
    {
        [DynamoDBHashKey]   
        public int entry { get; set; }

        [DynamoDBProperty("date")]    
        public string Date { get; set; }

        [DynamoDBProperty("summary")]    
        public string Summary { get; set; }

        [DynamoDBProperty("temperatureC")]    
        public int TemperatureC { get; set; }
        public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
   
    }
}
