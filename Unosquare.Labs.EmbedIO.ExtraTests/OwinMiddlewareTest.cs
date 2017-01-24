namespace Unosquare.Labs.EmbedIO.ExtraTests
{
    using NUnit.Framework;
    using Owin;
    using System;
    using System.IO;
    using System.Net;
    using System.Threading;
    using Unosquare.Labs.EmbedIO.ExtraTests.Properties;
    using Unosquare.Labs.EmbedIO.OwinMiddleware;

    public class OwinMiddlewareTest
    {
        protected WebServer WebServer;

        [SetUp]
        public void Init()
        {
            WebServer = new WebServer(Resources.ServerAddress)
                .UseOwin((owinApp) => owinApp.UseDirectoryBrowser());
            WebServer.RunAsync();
        }

        [Test]
        public void GetIndex()
        {
            var request = (HttpWebRequest)WebRequest.Create(Resources.ServerAddress);

            using (var response = (HttpWebResponse)request.GetResponse())
            {
                Assert.AreEqual(response.StatusCode, HttpStatusCode.OK, "Status Code OK");

                var html = new StreamReader(response.GetResponseStream()).ReadToEnd();

                Assert.IsNotEmpty(html, "Directory Browser Index page is not null");
                Assert.IsTrue(html.Contains("<title>Index of /</title>"), "Index page has correct title");
            }
        }

        [Test]
        public void GetErrorPage()
        {
            var request = (HttpWebRequest)WebRequest.Create(Resources.ServerAddress + "/error");

            try
            {
                // By design GetResponse throws exception with NotModified status, weird
                request.GetResponse();
            }
            catch (WebException ex)
            {
                if (ex.Response == null || ex.Status != WebExceptionStatus.ProtocolError)
                    throw;

                var response = (HttpWebResponse)ex.Response;

                Assert.AreEqual(response.StatusCode, HttpStatusCode.NotFound, "Status Code NotFound");
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