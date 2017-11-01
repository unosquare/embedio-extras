namespace Unosquare.Labs.EmbedIO.Extra.Tests
{
    using System;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
    using NUnit.Framework;
    using BearerToken;
    using TestObjects;
    using Markdown;
    using Swan.Formatters;
#if NET46
    using Unosquare.Net;
#else
    using System.Net;
#endif

    [TestFixture]
    public class BearerTokenModuleTest
    {
        protected string RootPath;
        protected BasicAuthorizationServerProvider BasicProvider = new BasicAuthorizationServerProvider();
        protected WebServer WebServer;
        protected string WebServerUrl;

        [SetUp]
        public void Init()
        {
            RootPath = TestHelper.SetupStaticFolder();

            WebServerUrl = Resources.GetServerAddress();
            WebServer = new WebServer(WebServerUrl);
            WebServer.RegisterModule(new BearerTokenModule(BasicProvider));
            WebServer.RegisterModule(new MarkdownStaticModule(RootPath));
            WebServer.RunAsync();
        }

#if !NET46
        [Test]
        public void TestBasicAuthorizationServerProvider()
        {
            Assert.Throws<ArgumentNullException>(() => BasicProvider
                .ValidateClientAuthentication(new ValidateClientAuthenticationContext(null))
                .RunSynchronously());
        }
#endif

        [Test]
        public async Task GetInvalidToken()
        {
            var payload = System.Text.Encoding.UTF8.GetBytes("grant_type=nothing");

            var request = (System.Net.HttpWebRequest) System.Net.WebRequest.Create(WebServerUrl + "token");
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            using (var dataStream = await request.GetRequestStreamAsync())
            {
                dataStream.Write(payload, 0, payload.Length);
            }

            try
            {
                var response = (System.Net.HttpWebResponse) await request.GetResponseAsync();
            }
            catch (System.Net.WebException ex)
            {
                if (ex.Response == null || ex.Status != System.Net.WebExceptionStatus.ProtocolError)
                    throw;

                var response = (System.Net.HttpWebResponse) ex.Response;

                Assert.AreEqual(response.StatusCode, System.Net.HttpStatusCode.Unauthorized);
            }
        }

        [Test]
        public async Task GetValidToken()
        {
            string token;
            var payload = System.Text.Encoding.UTF8.GetBytes("grant_type=password&username=test&password=test");

            var request = (System.Net.HttpWebRequest) System.Net.WebRequest.Create(WebServerUrl + "token");
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";

            using (var dataStream = await request.GetRequestStreamAsync())
            {
                dataStream.Write(payload, 0, payload.Length);
            }

            using (var response = (System.Net.HttpWebResponse) await request.GetResponseAsync())
            {
                Assert.AreEqual(response.StatusCode, System.Net.HttpStatusCode.OK, "Status Code OK");

                var jsonString = new StreamReader(response.GetResponseStream()).ReadToEnd();
                Assert.IsNotEmpty(jsonString);

                var json = Json.Deserialize<BearerToken>(jsonString);
                Assert.IsNotNull(json);
                Assert.IsNotEmpty(json.Token);
                Assert.IsNotEmpty(json.Username);

                token = json.Token;
            }

            var indexRequest = (System.Net.HttpWebRequest) System.Net.WebRequest.Create(WebServerUrl + "index.html");

            try
            {
                using (var response = (System.Net.HttpWebResponse) await indexRequest.GetResponseAsync())
                {
                    Assert.AreEqual(response.StatusCode, System.Net.HttpStatusCode.OK, "Status Code OK");
                }
            }
            catch (System.Net.WebException ex)
            {
                if (ex.Response == null || ex.Status != System.Net.WebExceptionStatus.ProtocolError)
                    throw;

                var response = (System.Net.HttpWebResponse) ex.Response;

                Assert.AreEqual(response.StatusCode, System.Net.HttpStatusCode.Unauthorized);

            }

            indexRequest = (System.Net.HttpWebRequest) System.Net.WebRequest.Create(WebServerUrl + "index.html");
            indexRequest.Headers["Authorization"] = "Bearer " + token;

            using (var response = (System.Net.HttpWebResponse) await indexRequest.GetResponseAsync())
            {
                Assert.AreEqual(response.StatusCode, System.Net.HttpStatusCode.OK, "Status Code OK");
            }
        }

        [TearDown]
        public void Kill()
        {
            Thread.Sleep(TimeSpan.FromSeconds(1));
            WebServer.Dispose();
        }
    }
}