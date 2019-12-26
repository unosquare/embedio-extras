namespace EmbedIO.ExtraSample
{
    using Swan.Logging;
    using System;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
    using BearerToken;
    using JsonServer;
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
        public static string WebRootPath => Path.Combine(SwanRuntime.EntryAssemblyDirectory, "web");

        /// <summary>
        /// Defines the entry point of the application.
        /// </summary>
        /// <param name="args">The arguments.</param>
        private static async Task Main(string[] args)
        {
            var url = args.Length > 0 ? args[0] : "http://localhost:9696/";

            using var tokenSource = new CancellationTokenSource();
            tokenSource.Token.Register(() => "Shutting down".Info());

            // Create basic authentication provider
            var authServer = new SampleAuthorizationServerProvider();

            // Set a task waiting for press key to exit
#pragma warning disable 4014
            Task.Run(() =>
#pragma warning restore 4014
            {
                // Wait for any key to be pressed before disposing of our web server.
                Console.ReadLine();

                tokenSource.Cancel();
            }, tokenSource.Token);

            var bearerTokenModule = new BearerTokenModule(
                "/api",
                authServer,
                "0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9eyJjbGF")
            {
                OnSuccessTransformation = dict => { dict.Add("logged", true); },
            };

            // Our web server is disposable. 
            using var server = new WebServer(url)
                .WithModule(bearerTokenModule)
                .WithModule(new JsonServerModule(jsonPath: Path.Combine(WebRootPath, "database.json")))
                .WithModule(new MarkdownStaticModule("/", WebRootPath));

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
            if (!tokenSource.IsCancellationRequested)
                await server.RunAsync(tokenSource.Token);

            "Bye".Info();

            Terminal.Flush();
        }
    }
}