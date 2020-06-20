using System;
using System.Net;
using System.Net.Mail;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Amazon.Lambda.Core;
// https://docs.aws.amazon.com/ses/latest/DeveloperGuide/send-using-smtp-net.html
// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace emailservice
{
    public class Function
    {
        
        /// <summary>
        /// A simple function that takes a string and does a ToUpper
        /// </summary>
        /// <param name="input"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public string FunctionHandler(Requests input, ILambdaContext context)
        {
            
            String FROM = "belser@elsersmusings.com";
            String FROMNAME = "Ben Elser";
            String TO = "benjamin.elser18@gmail.com";
            String SMTP_USERNAME = "AKIARPD22HKLVQM2BDWX";
            String SMTP_PASSWORD = "BD8wCl2JTZmMibYKIp2NA6++JGPtw0+EnzqLSB5XWcVw";
            String HOST = "email-smtp.us-east-1.amazonaws.com";
            String SUBJECT = "NEW BLOG REQUEST";
            int PORT = 587;
            String BODY =
            "<h1>New Blog Request</h1>" +
            $"<h2>Topic: {input.Topic}</h2>" +
            $"<p>{input.Message}</p>";
            
            // Create and build a new MailMessage object
            MailMessage message = new MailMessage();
            message.IsBodyHtml = true;
            message.From = new MailAddress(FROM, FROMNAME);
            message.To.Add(new MailAddress(TO));
            message.Subject = SUBJECT;
            message.Body = BODY;
            using (var client = new System.Net.Mail.SmtpClient(HOST, PORT))
            {
                // Pass SMTP credentials
                client.Credentials =
                    new NetworkCredential(SMTP_USERNAME, SMTP_PASSWORD);

                // Enable SSL encryption
                client.EnableSsl = true;

                // Try to send the message. Show status in console.
                try
                {
                    Console.WriteLine("Attempting to send email...");
                    client.Send(message);
                    Console.WriteLine("Email sent!");
                    System.Console.WriteLine(input);
                    return "true";
                }
                catch (Exception ex)
                {
                    Console.WriteLine("The email was not sent.");
                    Console.WriteLine("Error message: " + ex.Message);
                    return "false";
                }
            }
        }
    }

    public class Requests
    {
        public string Topic { get; set; }
        public string Message { get; set; }
        
    }
}
