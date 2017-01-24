namespace Unosquare.Labs.EmbedIO.ExtraTests
{
    using NUnit.Framework;
    using System;
    using System.IO;
    using System.Net;
    using System.Threading;
    using Unosquare.Labs.EmbedIO.ExtraTests.Properties;
    using Unosquare.Labs.EmbedIO.Markdown;

    public class MarkdownModuleTest
    {
        protected string RootPath;
        protected WebServer WebServer;

        [SetUp]
        public void Init()
        {
            RootPath = TestHelper.SetupStaticFolder();

            WebServer = new WebServer(Resources.ServerAddress);
            WebServer.RegisterModule(new MarkdownStaticModule(RootPath));
            WebServer.RunAsync();
        }

        [Test]
        public void GetIndex()
        {
            var request = (HttpWebRequest) WebRequest.Create(Resources.ServerAddress);

            using (var response = (HttpWebResponse) request.GetResponse())
            {
                Assert.AreEqual(response.StatusCode, HttpStatusCode.OK, "Status Code OK");

                var html = new StreamReader(response.GetResponseStream()).ReadToEnd();

                Assert.AreEqual(html.Replace("\r", "").Replace("\n", ""),
                    Resources.indexhtml.Replace("\r", "").Replace("\n", ""), "Same content index.html");
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