namespace Unosquare.Labs.EmbedIOwin
{
    using Mezm.Owin.Razor;
    using Mezm.Owin.Razor.Routing;
    using Microsoft.Owin.Cors;
    using Microsoft.Owin.Hosting;
    using Owin;
    using System;
    using Unosquare.Labs.EmbedIO;
    using Unosquare.Labs.EmbedIO.Log;
    using Unosquare.Labs.EmbedIO.OwinMiddleware;

    /// <summary>
    /// Sample Owin-EmbedIO 
    /// </summary>
    public class Program
    {
        /// <summary>
        /// Entry point
        /// </summary>
        /// <param name="args"></param>
        private static void Main(string[] args)
        {
            try
            {
                OwinServerFactory.Log = new SimpleConsoleLog();

                Console.WriteLine("Do you want to run EmbedIO as OWIN Server?");
                var response = Console.ReadLine();

                if (response != null && response.ToLower() == "y")
                {
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
                else
                {
                    using (var webServer = WebServer
                        .CreateWithConsole("http://localhost:4578")
                        .WithWebApi(typeof (PeopleController).Assembly)
                        .UseOwin((owinApp) => 
                            owinApp
                            .UseDirectoryBrowser()
                            .UseRazor(Startup.InitRoutes)))
                    {
                        webServer.RunAsync();
                        Console.ReadKey();
                    }
                }
            }
            catch (Exception ex)
            {
                OwinServerFactory.Log.Error(ex);
            }
        }
    }

    /// <summary>
    /// Startup object
    /// </summary>
    public class Startup
    {
        /// <summary>
        /// Configure the OwinApp
        /// </summary>
        /// <param name="app"></param>
        public void Configuration(IAppBuilder app)
        {
            app.UseErrorPage();
            app.UseCors(CorsOptions.AllowAll);
            app.UseDirectoryBrowser();
            app.UseRazor(InitRoutes);
            app.UseWebApi(typeof (PeopleController).Assembly);
        }

        /// <summary>
        /// Initialize the Razor files
        /// </summary>
        /// <param name="table"></param>
        public static void InitRoutes(IRouteTable table)
        {
            table
                .AddFileRoute("/about/me", "Views/about.cshtml", new { Name = "EmbedIO Razor", Date = DateTime.UtcNow });
        }
    }
}