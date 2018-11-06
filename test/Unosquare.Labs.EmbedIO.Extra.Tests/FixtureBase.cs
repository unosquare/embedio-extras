namespace Unosquare.Labs.EmbedIO.Extra.Tests
{
    using NUnit.Framework;
    using System.Net.Http;
    using System;
    using System.Threading.Tasks;
    using TestObjects;
    using Unosquare.Labs.EmbedIO.Tests;

    public abstract class FixtureBase
    {
        private readonly Action<IWebServer> _builder;
        private readonly bool _useTestWebServer;

        protected FixtureBase(Action<IWebServer> builder, bool useTestWebServer = false)
        {
            Swan.Terminal.Settings.GlobalLoggingMessageType = Swan.LogMessageType.None;

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
            (WebServerInstance as IDisposable)?.Dispose();
        }

        public async Task<string> GetString(string partialUrl = "")
        {
            if (WebServerInstance is TestWebServer testWebServer)
                return await testWebServer.GetClient().GetAsync(partialUrl);

            using (var client = new HttpClient())
            {
                var uri = new Uri(new Uri(WebServerUrl), partialUrl);

                return await client.GetStringAsync(uri);
            }
        }

        public async Task<TestHttpResponse> SendAsync(TestHttpRequest request)
        {
            if (WebServerInstance is TestWebServer testWebServer)
                return await testWebServer.GetClient().SendAsync(request);

            using (var client = new HttpClient())
            {
                var response = await client.SendAsync(request.ToHttpRequestMessage());

                return response.ToTestHttpResponse();
            }
        }
    }

    internal static class TestExtensions
    {
        public static HttpRequestMessage ToHttpRequestMessage(this TestHttpRequest request) => new HttpRequestMessage();

        public static TestHttpResponse ToTestHttpResponse(this HttpResponseMessage response) => new TestHttpResponse();
    }
}
