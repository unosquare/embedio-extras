using Mezm.Owin.Razor;
using Mezm.Owin.Razor.Routing;
using Microsoft.Owin.Cors;
using Microsoft.Owin.Hosting;
using Owin;
using System;
using Unosquare.Labs.EmbedIO.Log;
using Unosquare.Labs.EmbedIO.OwinMiddleware;

namespace Unosquare.Labs.EmbedIOwin
{
    public class Program
    {
        private static void Main(string[] args)
        {
            try
            {
                OwinServerFactory.Log = new SimpleConsoleLog();

                var options = new StartOptions
                {
                    ServerFactory = OwinServerFactory.ServerFactoryName,
                    Port = 4578
                };

                using (WebApp.Start<Startup>(options))
                {
                    OwinServerFactory.Log.DebugFormat("Running a http server on port {0}", options.Port);
                    Console.ReadKey();
                }
            }
            catch (Exception ex)
            {
                OwinServerFactory.Log.Error(ex);
            }
        }
    }

    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.UseErrorPage();
            app.UseCors(CorsOptions.AllowAll);
            app.UseDirectoryBrowser();
            app.UseRazor(InitRoutes);
            app.UseWebApi(typeof (PeopleController).Assembly);
        }

        private static void InitRoutes(IRouteTable table)
        {
            table
                .AddFileRoute("/about/me", "Views/about.cshtml", new { Name = "EmbedIO Razor", Date = DateTime.UtcNow });
        }
    }
}