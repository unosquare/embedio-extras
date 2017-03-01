using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Unosquare.Labs.EmbedIO.BearerToken;
using Unosquare.Labs.EmbedIO.Extra.Tests.TestObjects;
using Unosquare.Labs.EmbedIO.Markdown;
using Unosquare.Swan.Formatters;
using System.Net;
#if !NET46
using Unosquare.Net;
#endif

namespace Unosquare.Labs.EmbedIO.Extra.Tests
{
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
            try
            {
                BasicProvider.ValidateClientAuthentication(new ValidateClientAuthenticationContext(null))
                    .RunSynchronously();
            }
            catch (Exception ex)
            {
                Assert.AreEqual(ex.GetType(), typeof(ArgumentNullException));
            }
        }
#endif

        [Test]
        public async Task GetInvalidToken()
        {
            var payload = System.Text.Encoding.UTF8.GetBytes("grant_type=nothing");

            var request = (HttpWebRequest) WebRequest.Create(WebServerUrl + "token");
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            using (var dataStream = await request.GetRequestStreamAsync())
            {
                dataStream.Write(payload, 0, payload.Length);
            }

            try
            {
                var response = (HttpWebResponse) await request.GetResponseAsync();
            }
            catch (WebException ex)
            {
                if (ex.Response == null || ex.Status != WebExceptionStatus.ProtocolError)
                    throw;

                var response = (HttpWebResponse) ex.Response;

                Assert.AreEqual(response.StatusCode, HttpStatusCode.Unauthorized, "Status Code Unauthorized");

            }
        }

        [Test]
        public async Task GetValidToken()
        {
            string token;
            var payload = System.Text.Encoding.UTF8.GetBytes("grant_type=password&username=test&password=test");

            var request = (HttpWebRequest) WebRequest.Create(WebServerUrl + "token");
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";

            using (var dataStream = await request.GetRequestStreamAsync())
            {
                dataStream.Write(payload, 0, payload.Length);
            }

            using (var response = (HttpWebResponse) await request.GetResponseAsync())
            {
                Assert.AreEqual(response.StatusCode, HttpStatusCode.OK, "Status Code OK");

                var jsonString = new StreamReader(response.GetResponseStream()).ReadToEnd();
                Assert.IsNotEmpty(jsonString);

                var json = Json.Deserialize<BearerToken.BearerToken>(jsonString);
                Assert.IsNotNull(json);
                Assert.IsNotEmpty(json.Token);
                Assert.IsNotEmpty(json.Username);

                token = json.Token;
            }

            var indexRequest = (HttpWebRequest) WebRequest.Create(WebServerUrl + "index.html");

            try
            {
                using (var response = (HttpWebResponse) await indexRequest.GetResponseAsync())
                {
                    Assert.AreEqual(response.StatusCode, HttpStatusCode.OK, "Status Code OK");
                }
            }
            catch (WebException ex)
            {
                if (ex.Response == null || ex.Status != WebExceptionStatus.ProtocolError)
                    throw;

                var response = (HttpWebResponse) ex.Response;

                Assert.AreEqual(response.StatusCode, HttpStatusCode.Unauthorized, "Status Code Unauthorized");

            }

            indexRequest = (HttpWebRequest) WebRequest.Create(WebServerUrl + "index.html");
            indexRequest.Headers["Authorization"] = "Bearer " + token;

            using (var response = (HttpWebResponse) await indexRequest.GetResponseAsync())
            {
                Assert.AreEqual(response.StatusCode, HttpStatusCode.OK, "Status Code OK");
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