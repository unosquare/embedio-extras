namespace Unosquare.Labs.EmbedIO.ExtraSample
{
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
        private static void Main(string[] args)
        {
            var url = args.Length > 0 ? args[0] : "http://localhost:9696/";

            // Create basic authentication provider
            var basicAuthProvider = new BasicAuthorizationServerProvider();

            // Create Webserver with console logger and attach LocalSession and Static
            // files module
            using (var server = new WebServer(url))
            {
                server
                    .EnableCors()
                    .UseBearerToken(basicAuthProvider, new[] {"/secure.html"});

                server.RegisterModule(new JsonServerModule(jsonPath: Path.Combine(WebRootPath, "database.json")));
                server.RegisterModule(new MarkdownStaticModule(WebRootPath));
                server.RegisterModule(new LiteLibModule<TestDbContext>(new TestDbContext(), "/dbapi/"));
                server.RunAsync();

                // Fire up the browser to show the content if we are debugging!
                var browser = new System.Diagnostics.Process
                {
                    StartInfo = new System.Diagnostics.ProcessStartInfo(url) {UseShellExecute = true}
                };
                browser.Start();
                Terminal.ReadKey(true, true);
            }
        }
    }
}