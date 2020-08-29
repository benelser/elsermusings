using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Net;
using System.Net.Mail;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using Amazon.DynamoDBv2.DataModel;

// https://docs.aws.amazon.com/ses/latest/DeveloperGuide/send-using-smtp-net.html
// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace RequestApptBataviaAuto
{
    public class Function
    {
        
        /// <summary>
        /// A simple function that takes a string and does a ToUpper
        /// </summary>
        /// <param name="input"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public bool FunctionHandler(AppointmentRequestModel input, ILambdaContext context)
        {
            
            var assembly = Assembly.GetExecutingAssembly();
            string resourceName = assembly.GetManifestResourceNames().Single(str => str.EndsWith("email.html"));
            StringBuilder html = new StringBuilder();
            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            using (StreamReader reader = new StreamReader(stream))
            {
                html.Append(reader.ReadToEnd());
                
            }
            html.Replace("{FirstName}", $"{input.FirstName}");
            html.Replace("{LastName}", $"{input.LastName}");
            html.Replace("{Email}", $"{input.Email}");
            html.Replace("{PhoneNumber}", $"{input.PhoneNumber}");
            html.Replace("{DesiredDate}", $"{input.DesiredDate}");
            html.Replace("{ContactMethod}", $"{input.ContactMethod}");
            html.Replace("{AppointmentType}", $"{input.AppointmentType}");
            html.Replace("{Year}", $"{input.Year}");
            html.Replace("{Make}", $"{input.Make}");
            html.Replace("{Model}", $"{input.Model}");
            html.Replace("{Message}", $"{input.Message}");

            Organization currentOrg = Organization.GetOrganization(input.OrgId);
    
            if (string.IsNullOrEmpty(currentOrg.Name))
            {
                Console.WriteLine($"Something went wrong while fetching Org with id: {input.OrgId}");
                Environment.Exit(Environment.ExitCode);
            }

            string[] recips = { currentOrg.Email };
            // string unsubscribeLink = currentContact.GetUnsubscribeLink(currentOrg);
            // String BODY =
            // "<h1>New Song Request</h1>" +
            // $"<h2>Artist: {input.Artist}</h2>" +
            // $"<h2>Song Title: {input.Title}</h2>";
            EmailService es = new EmailService("noreply@myradiantsolution.com","Appointment Request Service", recips, "New Appointment Request", html.ToString());
            System.Console.WriteLine(es);
            bool success = es.SendMessage();
            if (success == true)
            {   
                return true;
            }
            else
            {
                return false;
            }
           
        }
    }



    public class AppointmentRequestModel
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string DesiredDate { get; set; }
        public string ContactMethod { get; set; }
        public string AppointmentType { get; set; }
        public string Year { get; set; }
        public string Make { get; set; }
        public string Model { get; set; }
        public string Message { get; set; }
        public string OrgId { get; set; }
        
    }

    public class EmailService
    {
        private string SMTP_USERNAME { get; set; }
        private string SMTP_PASSWORD { get; set; }
        private string HOST { get; set; }
        public int PORT { get; set; }
        public string From { get; set; }
        public string FromName { get; set; }
        public string Subject { get; set; }
        public string  Body { get; set; }
        public MailMessage message = new MailMessage();
        public EmailService(string From, String FromName, string[] To, string Subject, string Body)
        {
            
            this.SMTP_USERNAME = Environment.GetEnvironmentVariable("SMTP_USERNAME");;
            this.SMTP_PASSWORD = Environment.GetEnvironmentVariable("SMTP_PASSWORD");;
            this.HOST = "email-smtp.us-east-1.amazonaws.com";
            this.PORT = 587;
            this.From = From;
            this.FromName = FromName;
            this.Subject = Subject;
            this.Body = Body;
            this.CreateToList(To);

        }

        public bool SendMessage()
        {
            // Create and build a new MailMessage object
            this.message.IsBodyHtml = true;
            this.message.From = new MailAddress(this.From, this.FromName);
            this.message.Subject = this.Subject;
            this.message.Body = this.Body;
            using (var client = new System.Net.Mail.SmtpClient(this.HOST, this.PORT))
            {
                // Pass SMTP credentials
                client.Credentials =
                    new NetworkCredential(this.SMTP_USERNAME, this.SMTP_PASSWORD);

                // Enable SSL encryption
                client.EnableSsl = true;

                // Try to send the message. Show status in console.
                try
                {
                    Console.WriteLine("Attempting to send email...");
                    client.Send(this.message);
                    Console.WriteLine("Email sent!");
                    return true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("The email was not sent.");
                    Console.WriteLine("Error message: " + ex.Message);
                }
                return false;
            }
        }   

        private void CreateToList(string[] To)
        {
            foreach (string name in To)
            {
                this.message.To.Add(new MailAddress(name));
            }
        }

        public override string ToString()
        {
            return $"From: {this.From}\nFromName: {this.FromName}\nBody {this.Body}";
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
        [DynamoDBProperty("Email")]
        public string Email { get; set; }

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