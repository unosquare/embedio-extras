// TODO: Complete
//using System;
//using System.Net;
//using Unosquare.Labs.EmbedIO.Modules;

//namespace Unosquare.Labs.EmbedIO.Extra.Tests.TestObjects
//{
//    public class TestController : WebApiController
//    {
//        [WebApiHandler(HttpVerbs.Get, "/people/*")]
//        public bool GetPeople(WebServer server, HttpListenerContext context)
//        {
//            try
//            {
//                return context.JsonResponse(new { Name = "Hola" });
//            }
//            catch (Exception ex)
//            {
//                context.Response.StatusCode = (int)System.Net.HttpStatusCode.InternalServerError;
//                return context.JsonResponse(ex);
//            }
//        }
//    }
//}