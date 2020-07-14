using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
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
            
            Organization currentOrg;
            Contact currentContact;
            try
            {
                currentOrg = Organization.GetOrganization(input.OrgId);
                currentContact = Contact.GetContact(input.Email, currentOrg.Id);
                var EmailCheck = currentContact.LookUpEmail();
                EmailCheck.Wait();
                Console.WriteLine($"{currentContact.Email} already exists in database.");
                return false;
                
            }
            catch (NullReferenceException)
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
            
        }
    }

        public class Contact
        {
    
            [DynamoDBHashKey("email")]
            public string Email { get; set; }

            [DynamoDBProperty("FirstName")]
            public string FirstName { get; set; }
            [DynamoDBProperty("LastName")]
            public string LastName { get; set; }
    
            [DynamoDBProperty("PhoneNumber")]
            public string PhoneNumber { get; set; }
            [DynamoDBProperty("GUID")]
            public string GUID = Guid.NewGuid().ToString();
            [DynamoDBProperty("OrgId")]
            public string OrgId { get; set; }
            public string Message { get; set; }

            public string SayHello()
            {
                return $"Hello {this.FirstName}";
            }
            private static async Task<Contact> GetContactData(Contact C, string OrgId)
            {
                var client = new AmazonDynamoDBClient();
                var dbcontextconfig = new DynamoDBContextConfig();
                dbcontextconfig.TableNamePrefix = $"{OrgId}-";
                DynamoDBContext dbcontext = new DynamoDBContext(client, dbcontextconfig);
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

            public static Contact GetContact(string email, string OrgId){
                Contact C = new Contact();
                C.Email = email;
                var result = GetContactData(C, OrgId);
                result.Wait();
                return result.Result;
            }

            public async Task<bool> WriteContactInfo()
            {
                var client = new AmazonDynamoDBClient();
                this.Email = this.Email.ToLower();
                var dbcontextconfig = new DynamoDBContextConfig();
                dbcontextconfig.TableNamePrefix = $"{this.OrgId}-";
                DynamoDBContext dbcontext = new DynamoDBContext(client, dbcontextconfig);
                await dbcontext.SaveAsync(this);
                return true;        
                
            }

            public async Task<Contact> LookUpEmail()
            {
                var client = new AmazonDynamoDBClient();
                var dbcontextconfig = new DynamoDBContextConfig();
                dbcontextconfig.TableNamePrefix = $"{this.OrgId}-";
                DynamoDBContext dbcontext = new DynamoDBContext(client, dbcontextconfig);
                try
                {
                    var result = await dbcontext.LoadAsync<Contact>(this.Email.ToLower());
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

        [DynamoDBTable("organizations")]
        public class Organization
        {

            [DynamoDBHashKey]
            public string Id { get; set; }
            [DynamoDBProperty("Domain")]
            public string Domain { get; set; }
            [DynamoDBProperty("Name")]
            public string Name { get; set; }

            private static async Task<Organization> GetOrganizationData(Organization org)
            {
                var client = new AmazonDynamoDBClient();
                DynamoDBContext dbcontext = new DynamoDBContext(client);
                try
                {
                    var result = await dbcontext.LoadAsync<Organization>(org.Id);
                    // this.Name = result;
                    // this.Domain = currentOrg.Domain;
                    Console.WriteLine($"Organization {result}"); 
                    return result;

                }
                catch (System.Exception)
                {
                    Console.WriteLine("Exception thrown inside GetORg");
                    return new Organization();
                }
            }

            public static Organization GetOrganization(string id){
                Organization org = new Organization();
                org.Id = id;
                var result = GetOrganizationData(org);
                result.Wait();
                return result.Result;
            }
        }
}
