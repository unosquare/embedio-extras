using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using Unosquare.Labs.EmbedIO.ExtraTests.Properties;
using Unosquare.Labs.EmbedIO.JsonServer;
using Unosquare.Swan.Formatters;

namespace Unosquare.Labs.EmbedIO.ExtraTests
{
    public class JsonServerModuleTest
    {
        protected string RootPath;
        protected string ApiPath = "api/";
        protected WebServer WebServer;

        [SetUp]
        public void Init()
        {
            RootPath = TestHelper.SetupStaticFolder();

            WebServer = new WebServer(Resources.ServerAddress);
            WebServer.RegisterModule(new JsonServerModule("/" + ApiPath, Path.Combine(RootPath, "database.json")));
            WebServer.RunAsync();
        }

        [Test]
        public void GetAll()
        {
            var request = (HttpWebRequest) WebRequest.Create(Resources.ServerAddress + ApiPath);

            using (var response = (HttpWebResponse) request.GetResponse())
            {
                Assert.AreEqual(response.StatusCode, HttpStatusCode.OK, "Status Code OK");

                var jsonString = new StreamReader(response.GetResponseStream()).ReadToEnd();
                Assert.IsNotEmpty(jsonString);

                dynamic json = Json.Deserialize(jsonString);
                Assert.IsNotNull(json);
            }
        }

        [Test]
        public void GetAllPosts()
        {
            var request = (HttpWebRequest) WebRequest.Create(Resources.ServerAddress + ApiPath + "/posts");

            using (var response = (HttpWebResponse) request.GetResponse())
            {
                Assert.AreEqual(response.StatusCode, HttpStatusCode.OK, "Status Code OK");

                var jsonString = new StreamReader(response.GetResponseStream()).ReadToEnd();
                Assert.IsNotEmpty(jsonString);

                dynamic json = Json.Deserialize(jsonString);
                Assert.IsNotNull(json);
            }
        }

        [Test]
        public void GetFirstPosts()
        {
            var request = (HttpWebRequest) WebRequest.Create(Resources.ServerAddress + ApiPath + "/posts/1");

            using (var response = (HttpWebResponse) request.GetResponse())
            {
                Assert.AreEqual(response.StatusCode, HttpStatusCode.OK, "Status Code OK");

                var jsonString = new StreamReader(response.GetResponseStream()).ReadToEnd();
                Assert.IsNotEmpty(jsonString);

                dynamic json = Json.Deserialize(jsonString);

                Assert.IsNotNull(json);
                Assert.AreEqual(json.GetValue("id", StringComparison.InvariantCultureIgnoreCase).ToString(), "1");
            }
        }

        [Test]
        public void AddPost()
        {
            var request = (HttpWebRequest) WebRequest.Create(Resources.ServerAddress + ApiPath + "/posts");
            request.Method = "POST";

            using (var dataStream = request.GetRequestStream())
            {
                var byteArray = Encoding.UTF8.GetBytes("{ 'id': 4, 'title': 'tubular2', 'author': 'unosquare' }");
                dataStream.Write(byteArray, 0, byteArray.Length);
            }

            using (var response = (HttpWebResponse) request.GetResponse())
            {
                Assert.AreEqual(response.StatusCode, HttpStatusCode.OK, "Status Code OK");
            }

            var indexRequest = (HttpWebRequest)WebRequest.Create(Resources.ServerAddress + ApiPath + "/posts");

            using (var response = (HttpWebResponse)indexRequest.GetResponse())
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
        public void PutPost()
        {
            var request = (HttpWebRequest)WebRequest.Create(Resources.ServerAddress + ApiPath + "/posts/1");
            request.Method = "PUT";

            using (var dataStream = request.GetRequestStream())
            {
                var byteArray = Encoding.UTF8.GetBytes("{ 'id': 1, 'title': 'replace', 'author': 'unosquare' }");
                dataStream.Write(byteArray, 0, byteArray.Length);
            }

            using (var response = (HttpWebResponse)request.GetResponse())
            {
                Assert.AreEqual(response.StatusCode, HttpStatusCode.OK, "Status Code OK");
            }

            var retrieveRequest = (HttpWebRequest)WebRequest.Create(Resources.ServerAddress + ApiPath + "/posts/1");

            using (var response = (HttpWebResponse)retrieveRequest.GetResponse())
            {
                Assert.AreEqual(response.StatusCode, HttpStatusCode.OK, "Status Code OK");

                var jsonString = new StreamReader(response.GetResponseStream()).ReadToEnd();
                Assert.IsNotEmpty(jsonString);

                dynamic json = Json.Deserialize(jsonString);

                Assert.IsNotNull(json);
                Assert.AreEqual(json.GetValue("title", StringComparison.InvariantCultureIgnoreCase).ToString(), "replace");
            }
        }

        [Test]
        public void DeletePost()
        {
            var indexRequest = (HttpWebRequest)WebRequest.Create(Resources.ServerAddress + ApiPath + "/posts");
            var total = 0;

            using (var response = (HttpWebResponse)indexRequest.GetResponse())
            {
                Assert.AreEqual(response.StatusCode, HttpStatusCode.OK, "Status Code OK");

                var jsonString = new StreamReader(response.GetResponseStream()).ReadToEnd();
                Assert.IsNotEmpty(jsonString);

                var json = Json.Deserialize<List<object>>(jsonString);
                total = json.Count;
            }

            var request = (HttpWebRequest)WebRequest.Create(Resources.ServerAddress + ApiPath + "/posts/3");
            request.Method = "DELETE";

            using (var response = (HttpWebResponse)request.GetResponse())
            {
                Assert.AreEqual(response.StatusCode, HttpStatusCode.OK, "Status Code OK");
            }

            indexRequest = (HttpWebRequest)WebRequest.Create(Resources.ServerAddress + ApiPath + "/posts");

            using (var response = (HttpWebResponse)indexRequest.GetResponse())
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