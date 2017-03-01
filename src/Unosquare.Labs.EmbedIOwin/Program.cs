namespace Unosquare.Labs.EmbedIOwin
{
    using Mezm.Owin.Razor;
    using Mezm.Owin.Razor.Routing;
    using Microsoft.Owin.Hosting;
    using Owin;
    using Swan;
    using System;
    using Unosquare.Labs.EmbedIO;
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
                Console.WriteLine("Do you want to run EmbedIO as OWIN Server? (y/n)");
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
                        "Running a http server on port {options.Port}".Info(nameof(Program));
                        Console.ReadKey();
                    }
                }
                else
                {
                    using (var webServer = WebServer
                        .Create("http://localhost:4578")
                        .WithWebApi(typeof(PeopleController).Assembly)
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
                ex.Log(nameof(Program));
                Console.ReadKey();
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
            //app.UseCors(CorsOptions.AllowAll);
            app.UseDirectoryBrowser();
            app.UseRazor(InitRoutes);
            app.UseWebApi(typeof(PeopleController).Assembly);
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