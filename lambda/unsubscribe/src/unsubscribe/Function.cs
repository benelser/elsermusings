using System;
using System.Text;
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

namespace unsubscribe
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
            
            try
            {
                Contact currentContact = Contact.GetContact(input.Email);
                if (currentContact.CheckGUIDMatch(input))
                {
                    currentContact.DeleteContact();
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (System.Exception)
            {
                
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
        [DynamoDBProperty("GUID")]
        public string GUID { get; set; }
        public string Message { get; set; }

        private static async Task<Contact> GetContactData(Contact C)
        {
            var client = new AmazonDynamoDBClient();
            DynamoDBContext dbcontext = new DynamoDBContext(client);
            try
            {
                var result = await dbcontext.LoadAsync<Contact>(C.Email);
                // this.Name = result;
                // this.Domain = currentOrg.Domain;
                Console.WriteLine($"Contact {result}"); 
                return result;

            }
            catch (System.Exception)
            {
                Console.WriteLine("Exception thrown inside GetContactData");
                return new Contact();
            }
        }

        public static Contact GetContact(string email){
            Contact C = new Contact();
            C.Email = email;
            var result = GetContactData(C);
            result.Wait();
            return result.Result;
        }

        public bool CheckGUIDMatch(Contact fromUser)
        {
            if (fromUser.Email.ToLower() == this.Email.ToLower() && fromUser.GUID == this.GUID)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public async void DeleteContact()
        {
            var client = new AmazonDynamoDBClient();
            DynamoDBContext dbcontext = new DynamoDBContext(client);
            try
            {
                await dbcontext.DeleteAsync<Contact>(this.Email);
                Console.WriteLine($"Deleted {this.Email} successfully."); 

            }
            catch (System.Exception)
            {
                Console.WriteLine($"Exception thrown while attempting to delete {this.Email}");
            }
        }
        
    }
}
