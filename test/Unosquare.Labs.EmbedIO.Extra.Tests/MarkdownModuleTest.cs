using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Unosquare.Labs.EmbedIO.Extra.Tests.TestObjects;
using Unosquare.Labs.EmbedIO.Markdown;

namespace Unosquare.Labs.EmbedIO.Extra.Tests
{
    [TestFixture]
    public class MarkdownModuleTest
    {
        protected string RootPath;
        protected WebServer WebServer;
        protected string WebServerUrl;

        [SetUp]
        public void Init()
        {
            RootPath = TestHelper.SetupStaticFolder();

            WebServerUrl = Resources.GetServerAddress();
            WebServer = new WebServer(WebServerUrl);
            WebServer.RegisterModule(new MarkdownStaticModule(RootPath));
            WebServer.RunAsync();
        }

        [Test]
        public async Task GetIndex()
        {
            var request = (HttpWebRequest) WebRequest.Create(WebServerUrl);

            using (var response = (HttpWebResponse) await request.GetResponseAsync())
            {
                Assert.AreEqual(response.StatusCode, HttpStatusCode.OK, "Status Code OK");

                var html = new StreamReader(response.GetResponseStream()).ReadToEnd();

                Assert.AreEqual(html.Replace("\r", "").Replace("\n", ""),
                    Resources.IndexHtml.Replace("\r", "").Replace("\n", ""), "Same content index.html");
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