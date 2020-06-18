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

namespace writecontactinfo
{
    public class Function
    {
        
        /// <summary>
        /// A simple function that takes a string and does a ToUpper
        /// </summary>
        /// <param name="input"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public bool FunctionHandler(Contact input, ILambdaContext context)
        {
            var EmailCheck = input.LookUpEmail();
            EmailCheck.Wait();
            var C = EmailCheck.Result;
            if (C == null)
            {
                Console.WriteLine($"Email does not exist. Creating email: {input.Email}");
                var WriteContactTask = input.WriteContactInfo();
                WriteContactTask.Wait();
                bool result = WriteContactTask.Result;
                if (result)
                {
                    Console.WriteLine($"Save operation successful: {result}");
                    // Welcome email code goes here
                    return true;
                }
                else
                {
                    return false;
                }
                
            }
            else
            {
                Console.WriteLine($"{input.Email} already exists in database.");
                return false;
            }
            
            
        }
    }

        [DynamoDBTable("contacts")]
        public class Contact
        {
    
            [DynamoDBHashKey]
            public string Email { get; set; }

            [DynamoDBProperty("FirstName")]
            public string FirstName { get; set; }
            [DynamoDBProperty("LastName")]
            public string LastName { get; set; }
    
            [DynamoDBProperty("PhoneNumber")]
            public string PhoneNumber { get; set; }

            public string SayHello()
            {
                return $"Hello {this.FirstName}";
            }

            public async Task<bool> WriteContactInfo()
            {
                var client = new AmazonDynamoDBClient();
                DynamoDBContext dbcontext = new DynamoDBContext(client);
                await dbcontext.SaveAsync(this);
                return true;        
                
            }

            public async Task<Contact> LookUpEmail()
            {
                var client = new AmazonDynamoDBClient();
                DynamoDBContext dbcontext = new DynamoDBContext(client);
                try
                {
                    var result = await dbcontext.LoadAsync<Contact>(this.Email);
                    Console.WriteLine($"Result of lookup {result}"); 
                    return result;

                }
                catch (System.Exception)
                {
                    Contact C = new Contact();
                    return C;
                }
                    
            }
        }
}
