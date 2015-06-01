namespace Unosquare.Labs.EmbedIO.ExtraTests
{
    using Newtonsoft.Json;
    using NUnit.Framework;
    using System;
    using System.IO;
    using System.Net;
    using System.Threading;
    using Unosquare.Labs.EmbedIO.BearerToken;
    using Unosquare.Labs.EmbedIO.ExtraTests.Properties;

    public class BearerTokenModuleTest
    {
        protected BasicAuthorizationServerProvider BasicProvider = new BasicAuthorizationServerProvider();
        protected WebServer WebServer;
        protected TestConsoleLog Logger = new TestConsoleLog();

        [SetUp]
        public void Init()
        {
            WebServer = new WebServer(Resources.ServerAddress, Logger);
            WebServer.RegisterModule(new BearerTokenModule(BasicProvider));
            WebServer.RunAsync();
        }

        [Test]
        public void TestBasicAuthorizationServerProvider()
        {
            Assert.AreEqual(BasicProvider.GetExpirationDate(), DateTime.UtcNow.AddHours(12).Ticks);

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

                var json = JsonConvert.DeserializeObject<BearerToken>(jsonString);
                Assert.IsNotNull(json);
                Assert.IsNotNullOrEmpty(json.Token);
                Assert.IsNotNullOrEmpty(json.Username);
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