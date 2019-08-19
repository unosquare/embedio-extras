using System;
using System.Net.Http;
using System.Threading.Tasks;
using EmbedIO.Extra.Tests.TestObjects;
using EmbedIO.Testing;
using NUnit.Framework;
using Swan.Logging;

namespace EmbedIO.Extra.Tests
{
    public abstract class FixtureBase
    {
        private readonly Action<IWebServer> _builder;
        private readonly bool _useTestWebServer;

        protected FixtureBase(Action<IWebServer> builder, bool useTestWebServer = false)
        {
            Logger.UnregisterLogger<ConsoleLogger>();

            _builder = builder;
            _useTestWebServer = useTestWebServer;
        }

        public string WebServerUrl { get; private set; }

        public IWebServer WebServerInstance { get; private set; }

        [SetUp]
        public void Init()
        {
            WebServerUrl = Resources.GetServerAddress();
            WebServerInstance = _useTestWebServer
                ? (IWebServer)new TestWebServer()
                : new WebServer(WebServerUrl);

            _builder(WebServerInstance);
            WebServerInstance.RunAsync();
        }

        [TearDown]
        public void Kill()
        {
            Task.Delay(500).Wait();
            WebServerInstance?.Dispose();
        }

        public async Task<string> GetString(string partialUrl = "")
        {
            if (WebServerInstance is TestWebServer testWebServer)
                return await testWebServer.Client.GetStringAsync(partialUrl);

            using (var client = new HttpClient())
            {
                var uri = new Uri(new Uri(WebServerUrl), partialUrl);

                return await client.GetStringAsync(uri);
            }
        }
    }
}
