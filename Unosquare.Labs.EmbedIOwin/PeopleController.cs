using System;
using System.Collections.Generic;
using System.Linq;
using Unosquare.Labs.EmbedIO;
using Unosquare.Labs.EmbedIO.Modules;
using Unosquare.Net;
using Unosquare.Swan;

namespace Unosquare.Labs.EmbedIOwin
{
    public class PeopleController : WebApiController
    {

        /// <summary>
        /// A simple model representing a person
        /// </summary>
        public class Person
        {
            public int Key { get; set; }
            public string Name { get; set; }
            public int Age { get; set; }
            public string EmailAddress { get; set; }
            public string PhotoUrl { get; set; }
        }

        public static List<Person> People = new List<Person>
        {
            new Person() {Key = 1, Name = "Mario Di Vece", Age = 31, EmailAddress = "mario@unosquare.com"},
            new Person() {Key = 2, Name = "Geovanni Perez", Age = 32, EmailAddress = "geovanni.perez@unosquare.com"},
            new Person() {Key = 3, Name = "Luis Gonzalez", Age = 29, EmailAddress = "luis.gonzalez@unosquare.com"},
        };

        /// <summary>
        /// Gets the people.
        /// This will respond to 
        ///     GET http://localhost:9696/api/people/
        ///     GET http://localhost:9696/api/people/1
        ///     GET http://localhost:9696/api/people/{n}
        /// 
        /// Notice the wildcard is important
        /// </summary>
        /// <param name="server">The server.</param>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        /// <exception cref="KeyNotFoundException">Key Not Found:  + lastSegment</exception>
        [WebApiHandler(HttpVerbs.Get, "/api/people/*")]
        public bool GetPeople(WebServer server, HttpListenerContext context)
        {
            try
            {
                // read the last segment
                var lastSegment = context.Request.Url.Segments.Last();

                // if it ends with a / means we need to list people
                if (lastSegment.EndsWith("/"))
                    return context.JsonResponse(People);

                // otherwise, we need to parse the key and respond with the entity accordingly
                int key = 0;
                if (int.TryParse(lastSegment, out key) && People.Any(p => p.Key == key))
                {
                    return context.JsonResponse(People.FirstOrDefault(p => p.Key == key));
                }

                throw new KeyNotFoundException("Key Not Found: " + lastSegment);
            }
            catch (Exception ex)
            {
                // here the error handler will respond with a generic 500 HTTP code a JSON-encoded object
                // with error info. You will need to handle HTTP status codes correctly depending on the situation.
                // For example, for keys that are not found, ou will need to respond with a 404 status code.
                var errorResponse = new
                {
                    Title = "Unexpected Error",
                    ErrorCode = ex.GetType().Name,
                    Description = ex.ExceptionMessage(),
                };

                context.Response.StatusCode = (int)System.Net.HttpStatusCode.InternalServerError;
                return context.JsonResponse(errorResponse);
            }
        }
    }
}