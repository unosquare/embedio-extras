using Newtonsoft.Json;
using NUnit.Framework;
using System.IO;
using System.Net;
using Unosquare.Labs.EmbedIO.BearerToken;
using Unosquare.Labs.EmbedIO.ExtraTests.Properties;

namespace Unosquare.Labs.EmbedIO.ExtraTests
{
    public class BearerTokenModuleTest
    {
        protected WebServer WebServer;
        protected TestConsoleLog Logger = new TestConsoleLog();

        [SetUp]
        public void Init()
        {
            WebServer = new WebServer(Resources.ServerAddress, Logger);
            WebServer.RegisterModule(new BearerTokenModule(new BasicAuthorizationServerProvider()));
            WebServer.RunAsync();
        }

        [Test]
        public void GetValidToken()
        {
            var payload = System.Text.Encoding.UTF8.GetBytes("grant_type=password&username=test&password=test");

            var request = (HttpWebRequest) WebRequest.Create(Resources.ServerAddress + "token");
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = payload.Length;

            using (var dataStream = request.GetRequestStream())
            {
                dataStream.Write(payload, 0, payload.Length);
            }

            using (var response = (HttpWebResponse) request.GetResponse())
            {
                Assert.AreEqual(response.StatusCode, HttpStatusCode.OK, "Status Code OK");

                var jsonString = new StreamReader(response.GetResponseStream()).ReadToEnd();
                Assert.IsNotNullOrEmpty(jsonString);

                var json = JsonConvert.DeserializeObject<BearerToken.BearerToken>(jsonString);
                Assert.IsNotNull(json);
                Assert.IsNotNullOrEmpty(json.Token);
            }
        }
    }
}