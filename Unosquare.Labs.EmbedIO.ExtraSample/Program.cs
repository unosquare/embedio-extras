using System;
using System.IO;
using Unosquare.Labs.EmbedIO.BearerToken;
using Unosquare.Labs.EmbedIO.JsonServer;
using Unosquare.Labs.EmbedIO.Markdown;

namespace Unosquare.Labs.EmbedIO.ExtraSample
{
    internal class Program
    {
        /// <summary>
        /// Gets the web root path.
        /// </summary>
        /// <value>
        /// The HTML root path.
        /// </value>
        public static string WebRootPath
        {
            get
            {
                var assemblyPath = Path.GetDirectoryName(typeof (Program).Assembly.Location);
#if DEBUG
                // This lets you edit the files without restarting the server.
                return Path.GetFullPath(Path.Combine(assemblyPath, "..\\..\\web"));
#else
    // This is when you have deployed ythe server.
                return Path.Combine(assemblyPath, "html");
#endif
            }
        }

        /// <summary>
        /// Defines the entry point of the application.
        /// </summary>
        /// <param name="args">The arguments.</param>
        private static void Main(string[] args)
        {
            var url = "http://localhost:9696/";
            if (args.Length > 0)
                url = args[0];

            // Create basic authentication provider
            var basicAuthProvider = new BasicAuthorizationServerProvider();

            // Create Webserver with console logger and attach LocalSession and Static
            // files module
            var server = WebServer.CreateWithConsole(url).EnableCors();
            server.RegisterModule(new BearerTokenModule(basicAuthProvider, new[] {"/secure.html"}));
            server.RegisterModule(new JsonServerModule(jsonPath: Path.Combine(WebRootPath, "database.json")));
            server.RegisterModule(new MarkdownStaticModule(WebRootPath));
            server.RunAsync();

            // Fire up the browser to show the content if we are debugging!
#if DEBUG
            var browser = new System.Diagnostics.Process()
            {
                StartInfo = new System.Diagnostics.ProcessStartInfo(url) {UseShellExecute = true}
            };
            browser.Start();
#endif
            // Wait for any key to be pressed before disposing of our web server.
            // In a service we'd manage the lifecycle of of our web server using
            // something like a BackgroundWorker or a ManualResetEvent.
            Console.ReadKey(true);
            server.Dispose();
        }
    }
}