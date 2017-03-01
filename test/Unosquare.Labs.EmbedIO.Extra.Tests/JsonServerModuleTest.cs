using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Unosquare.Labs.EmbedIO.Extra.Tests.TestObjects;
using Unosquare.Labs.EmbedIO.JsonServer;
using Unosquare.Swan.Formatters;

namespace Unosquare.Labs.EmbedIO.Extra.Tests
{
    [TestFixture]
    public class JsonServerModuleTest
    {
        protected string RootPath;
        protected string ApiPath = "api/";
        protected string WebServerUrl;
        protected WebServer WebServer;

        [SetUp]
        public void Init()
        {
            RootPath = TestHelper.SetupStaticFolder();

            WebServerUrl = Resources.GetServerAddress();
            WebServer = new WebServer(WebServerUrl);
            WebServer.RegisterModule(new JsonServerModule("/" + ApiPath, Path.Combine(RootPath, "database.json")));
            WebServer.RunAsync();
        }

        [Test]
        public async Task GetAll()
        {
            var request = (HttpWebRequest) WebRequest.Create(WebServerUrl + ApiPath);

            using (var response = (HttpWebResponse) await request.GetResponseAsync())
            {
                Assert.AreEqual(response.StatusCode, HttpStatusCode.OK, "Status Code OK");

                var jsonString = new StreamReader(response.GetResponseStream()).ReadToEnd();
                Assert.IsNotEmpty(jsonString);

                dynamic json = Json.Deserialize(jsonString);
                Assert.IsNotNull(json);
            }
        }

        [Test]
        public async Task GetAllPosts()
        {
            var request = (HttpWebRequest) WebRequest.Create(WebServerUrl + ApiPath + "/posts");

            using (var response = (HttpWebResponse) await request.GetResponseAsync())
            {
                Assert.AreEqual(response.StatusCode, HttpStatusCode.OK, "Status Code OK");

                var jsonString = new StreamReader(response.GetResponseStream()).ReadToEnd();
                Assert.IsNotEmpty(jsonString);

                dynamic json = Json.Deserialize(jsonString);
                Assert.IsNotNull(json);
            }
        }

        [Test]
        public async Task GetFirstPosts()
        {
            var request = (HttpWebRequest) WebRequest.Create(WebServerUrl + ApiPath + "/posts/1");

            using (var response = (HttpWebResponse) await request.GetResponseAsync())
            {
                Assert.AreEqual(response.StatusCode, HttpStatusCode.OK, "Status Code OK");

                var jsonString = new StreamReader(response.GetResponseStream()).ReadToEnd();
                Assert.IsNotEmpty(jsonString);

                dynamic json = Json.Deserialize(jsonString);

                Assert.IsNotNull(json);
                Assert.AreEqual(json.GetValue("id", StringComparison.OrdinalIgnoreCase).ToString(), "1");
            }
        }

        [Test]
        public async Task AddPost()
        {
            var request = (HttpWebRequest) WebRequest.Create(WebServerUrl + ApiPath + "/posts");
            request.Method = "POST";

            using (var dataStream = await request.GetRequestStreamAsync())
            {
                var byteArray = Encoding.UTF8.GetBytes("{ 'id': 4, 'title': 'tubular2', 'author': 'unosquare' }");
                dataStream.Write(byteArray, 0, byteArray.Length);
            }

            using (var response = (HttpWebResponse) await request.GetResponseAsync())
            {
                Assert.AreEqual(response.StatusCode, HttpStatusCode.OK, "Status Code OK");
            }

            var indexRequest = (HttpWebRequest)WebRequest.Create(WebServerUrl + ApiPath + "/posts");

            using (var response = (HttpWebResponse) await indexRequest.GetResponseAsync())
            {
                Assert.AreEqual(response.StatusCode, HttpStatusCode.OK, "Status Code OK");

                var jsonString = new StreamReader(response.GetResponseStream()).ReadToEnd();
                Assert.IsNotEmpty(jsonString);

                var json = Json.Deserialize<List<object>>(jsonString);
                Assert.IsNotNull(json);
                Assert.AreEqual(json.Count, 4);
            }
        }

        [Test]
        public async Task PutPost()
        {
            var request = (HttpWebRequest)WebRequest.Create(WebServerUrl + ApiPath + "/posts/1");
            request.Method = "PUT";

            using (var dataStream = await request.GetRequestStreamAsync())
            {
                var byteArray = Encoding.UTF8.GetBytes("{ 'id': 1, 'title': 'replace', 'author': 'unosquare' }");
                dataStream.Write(byteArray, 0, byteArray.Length);
            }

            using (var response = (HttpWebResponse) await request.GetResponseAsync())
            {
                Assert.AreEqual(response.StatusCode, HttpStatusCode.OK, "Status Code OK");
            }

            var retrieveRequest = (HttpWebRequest)WebRequest.Create(WebServerUrl + ApiPath + "/posts/1");

            using (var response = (HttpWebResponse) await retrieveRequest.GetResponseAsync())
            {
                Assert.AreEqual(response.StatusCode, HttpStatusCode.OK, "Status Code OK");

                var jsonString = new StreamReader(response.GetResponseStream()).ReadToEnd();
                Assert.IsNotEmpty(jsonString);

                dynamic json = Json.Deserialize(jsonString);

                Assert.IsNotNull(json);
                Assert.AreEqual(json.GetValue("title", StringComparison.OrdinalIgnoreCase).ToString(), "replace");
            }
        }

        [Test]
        public async Task DeletePost()
        {
            var indexRequest = (HttpWebRequest)WebRequest.Create(WebServerUrl + ApiPath + "/posts");
            int total;

            using (var response = (HttpWebResponse) await indexRequest.GetResponseAsync())
            {
                Assert.AreEqual(response.StatusCode, HttpStatusCode.OK, "Status Code OK");

                var jsonString = new StreamReader(response.GetResponseStream()).ReadToEnd();
                Assert.IsNotEmpty(jsonString);

                var json = Json.Deserialize<List<object>>(jsonString);
                total = json.Count;
            }

            var request = (HttpWebRequest)WebRequest.Create(WebServerUrl + ApiPath + "/posts/3");
            request.Method = "DELETE";

            using (var response = (HttpWebResponse) await request.GetResponseAsync())
            {
                Assert.AreEqual(response.StatusCode, HttpStatusCode.OK, "Status Code OK");
            }

            indexRequest = (HttpWebRequest)WebRequest.Create(WebServerUrl + ApiPath + "/posts");

            using (var response = (HttpWebResponse) await indexRequest.GetResponseAsync())
            {
                Assert.AreEqual(response.StatusCode, HttpStatusCode.OK, "Status Code OK");

                var jsonString = new StreamReader(response.GetResponseStream()).ReadToEnd();
                Assert.IsNotEmpty(jsonString);

                var json = Json.Deserialize<List<object>>(jsonString);
                Assert.AreEqual(total - 1, json.Count);
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