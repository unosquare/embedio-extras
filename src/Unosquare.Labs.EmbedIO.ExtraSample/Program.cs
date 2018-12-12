using System;

namespace Unosquare.Labs.EmbedIO.ExtraSample
{
    using System.Threading.Tasks;
    using System.Threading;
    using System.IO;
    using BearerToken;
    using JsonServer;
    using LiteLibWebApi;
    using Markdown;
    using Swan;

    internal class Program
    {
        /// <summary>
        /// Gets the web root path.
        /// </summary>
        /// <value>
        /// The HTML root path.
        /// </value>
        public static string WebRootPath => Path.Combine(Runtime.EntryAssemblyDirectory, "web");

        /// <summary>
        /// Defines the entry point of the application.
        /// </summary>
        /// <param name="args">The arguments.</param>
        private static async Task Main(string[] args)
        {
            var url = args.Length > 0 ? args[0] : "http://localhost:9696/";
            
            var ctSource = new CancellationTokenSource();
            ctSource.Token.Register(() => "Shutting down".Info());

            // Create basic authentication provider
            var authServer = new SampleAuthorizationServerProvider();
            
            // Set a task waiting for press key to exit
#pragma warning disable 4014
            Task.Run(() =>
#pragma warning restore 4014
            {
                // Wait for any key to be pressed before disposing of our web server.
                Console.ReadLine();

                ctSource.Cancel();
            }, ctSource.Token);

            // Our web server is disposable. 
            using (var server = new WebServer(url))
            {
                server
                    .EnableCors()
                    .UseBearerToken(authServer, new[] {"/secure.html"});

                server.RegisterModule(new JsonServerModule(jsonPath: Path.Combine(WebRootPath, "database.json")));
                server.RegisterModule(new MarkdownStaticModule(WebRootPath));
                server.RegisterModule(new LiteLibModule<TestDbContext>(new TestDbContext(), "/dbapi/"));
                
                // Fire up the browser to show the content!
                var browser = new System.Diagnostics.Process
                {
                    StartInfo = new System.Diagnostics.ProcessStartInfo(url.Replace("*", "localhost"))
                    {
                        UseShellExecute = true,
                    },
                };

                browser.Start();

                // Once we've registered our modules and configured them, we call the RunAsync() method.
                if (!ctSource.IsCancellationRequested)
                    await server.RunAsync(ctSource.Token);

                "Bye".Info();

                Terminal.Flush();
            }
        }
    }
}