using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using Amazon.S3;
using Amazon.S3.Model;
using System.Globalization;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace googlecalendar
{
    public class Function
    {
        
        //static string ApplicationName = "Google Calendar API .NET Quickstart";
        /// <summary>
        /// A simple function that takes a string and does a ToUpper
        /// </summary>
        /// <param name="input"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public IList<Event> FunctionHandler(ILambdaContext context)
        {
            
            // Authentication code
            string key = "starthebandgooglecalendarapi";
            string credentialFile = Environment.GetEnvironmentVariable(key);
            AmazonS3Client client = new AmazonS3Client();
            string credentialFileContents = ReadS3CredentialFile(client, key, credentialFile);
            // Lambda is given 500 mb of temp storage
            string pathToCredentials = "/tmp/credentials.json";
            System.IO.File.WriteAllText(pathToCredentials, credentialFileContents);

            string[] scopes = {  
            CalendarService.Scope.Calendar,  
            CalendarService.Scope.CalendarEvents,  
            CalendarService.Scope.CalendarEventsReadonly  
            };  
            GoogleCredential credential;
            // Assume account using service account with domain deledation
            using (var stream = new FileStream(pathToCredentials, FileMode.Open, FileAccess.Read))
            {
                credential = GoogleCredential.FromStream(stream)
                                .CreateScoped(scopes).CreateWithUser("theband@startheband.com");
            }
            
            // Creating the service
            var service = new CalendarService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "Calendar Authentication Sample",
            });

             // Define parameters of request.
             // Getting revent 12 months out
            EventsResource.ListRequest request = service.Events.List("primary");
            request.TimeMin = DateTime.Now;
            request.TimeMax = DateTime.Now.AddMonths(12);
            request.ShowDeleted = false;
            request.SingleEvents = true;
            request.MaxResults = 2500;
            request.OrderBy = EventsResource.ListRequest.OrderByEnum.StartTime;

            // List events.
            Events events = request.Execute();
            Console.WriteLine("Upcoming events:");
            if (events.Items != null && events.Items.Count > 0)
            {
                foreach (var eventItem in events.Items)
                {
                    string when = eventItem.Start.DateTime.ToString();
                    if (String.IsNullOrEmpty(when))
                    {
                        when = eventItem.Start.Date;
                    }
                    Console.WriteLine("{0} ({1})", eventItem.Summary, when);
                }

                return events.Items;
            }
            else
            {
                Console.WriteLine("No upcoming events found.");
                return events.Items;
                
            }
            
        }

        public static string ReadS3CredentialFile(AmazonS3Client client, string bn, string S3Key)
        {
            GetObjectRequest request = new GetObjectRequest
            {
                BucketName = bn,
                Key = S3Key
            };
            var task = client.GetObjectAsync(request);
            task.Wait();
            GetObjectResponse response =  task.Result;

            StreamReader reader = new StreamReader(response.ResponseStream);

            String content = reader.ReadToEnd();

            Console.Out.WriteLine("Read S3 object with key " + S3Key + " in bucket " + bn + ". Content is: " + content);
            return content;
        }

        public static void CreateEvent(CalendarService service)
        {
            // https://developers.google.com/calendar/v3/reference/events/insert
            Event NewEvent = new Event();
            NewEvent.GuestsCanInviteOthers = true;
            NewEvent.Location = "In the middle of no where";
            EventDateTime StartTime = new EventDateTime();
            StartTime.TimeZone = "America/Chicago";
            StartTime.DateTimeRaw = ExtendedMethods.ToRfc3339String(new DateTime(2020, 07, 14, (10 + 5), 00, 00));
            NewEvent.Start = StartTime;
            EventDateTime EndTime = new EventDateTime();
            EndTime.DateTimeRaw = ExtendedMethods.ToRfc3339String(new DateTime(2020, 07, 14, (11 + 5), 00, 00));
            NewEvent.End = EndTime;
            NewEvent.Summary = "This is a super important meeting";
            NewEvent.Transparency = "transparent";
            List<EventAttendee> attendees = new List<EventAttendee>()
            {
                new EventAttendee{DisplayName = "Ben", Email = "belser@elsersmusings.com", Organizer = true},
                new EventAttendee{DisplayName = "Benjamin", Email = "benjamin.elser18@gmail.com", }

            };
            NewEvent.Attendees = attendees;
            EventsResource.InsertRequest InsertEvent = service.Events.Insert(NewEvent, "belser@elsersmusings.com");
            InsertEvent.Execute();


        }

        
    }


    public static class ExtendedMethods
    {
        public static string ToRfc3339String(this DateTime dateTime)
        {
            return dateTime.ToString("yyyy-MM-dd'T'HH:mm:ss.fffzzz", DateTimeFormatInfo.InvariantInfo);
        }
    }  
}

