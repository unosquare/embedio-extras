using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Unosquare.Labs.EmbedIO.Extra.Tests.TestObjects;
using Unosquare.Labs.EmbedIO.LiteLibWebApi;
using Unosquare.Swan.Formatters;
using Unosquare.Swan.Networking;

namespace Unosquare.Labs.EmbedIO.Extra.Tests
{
    [TestFixture]
    public class LiteLibModuleTest
    {
        protected string RootPath;
        protected string ApiPath = "dbapi";
        protected string WebServerUrl;
        protected WebServer WebServer;

        [SetUp]
        public void Init()
        {
            RootPath = TestHelper.SetupStaticFolder();

            WebServerUrl = Resources.GetServerAddress();
            WebServer = new WebServer(WebServerUrl);
            WebServer.RegisterModule(new LiteLibModule<TestDbContext>(new TestDbContext(), ApiPath));
            WebServer.RunAsync();
        }

        //Get All Orders
        [Test]
        public async Task GetAll()
        {
            var response = await JsonClient.GetString(WebServerUrl + ApiPath + "/order");
            Assert.AreEqual(response, HttpStatusCode.OK, "Status Code OK");
        }

        [Test]
        public async Task GetFirstItem()
        {
            var request = (HttpWebRequest)WebRequest.Create(WebServerUrl + ApiPath + "/order/1");

            using (var response = (HttpWebResponse)await request.GetResponseAsync())
            {
                Assert.AreEqual(response.StatusCode, HttpStatusCode.OK, "Status Code OK");

                var jsonString = new StreamReader(response.GetResponseStream()).ReadToEnd();
                Assert.IsNotEmpty(jsonString);

                dynamic json = Json.Deserialize(jsonString);
                Assert.IsNotNull(json);
            }
        }

        [Test]
        public async Task Post()
        {

        }
    }
}
