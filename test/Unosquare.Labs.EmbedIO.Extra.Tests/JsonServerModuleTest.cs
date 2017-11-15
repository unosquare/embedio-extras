using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Unosquare.Labs.EmbedIO.Extra.Tests.TestObjects;
using Unosquare.Labs.EmbedIO.JsonServer;
using Unosquare.Swan.Formatters;
using Unosquare.Swan.Networking;

namespace Unosquare.Labs.EmbedIO.Extra.Tests
{
    [TestFixture]
    public class JsonServerModuleTest : FixtureBase
    {
        protected const string ApiPath = "api/";

        public JsonServerModuleTest()
            : base(ws => ws.RegisterModule(
                new JsonServerModule("/" + ApiPath, Path.Combine(TestHelper.SetupStaticFolder(), "database.json"))))
        {
            // placeholder
        }

        [Test]
        public async Task GetAllJson()
        {
            var jsonString = await JsonClient.GetString(WebServerUrl + ApiPath);
            Assert.IsNotEmpty(jsonString);
            var json = Json.Deserialize(jsonString);
            Assert.IsNotNull(json);
        }

        [Test]
        public async Task GetAllPostsJson()
        {
            var jsonString = await JsonClient.GetString(WebServerUrl + ApiPath + "/posts");
            Assert.IsNotEmpty(jsonString);
            var json = Json.Deserialize(jsonString);
            Assert.IsNotNull(json);
        }

        [Test]
        public async Task GetFirstPostsJson()
        {
            var jsonString = await JsonClient.GetString(WebServerUrl + ApiPath + "/posts/1");
            Assert.IsNotEmpty(jsonString);
            dynamic json = Json.Deserialize(jsonString);
            Assert.IsNotNull(json);
            Assert.AreEqual(json["id"].ToString(), "1");
        }

        [Test]
        public async Task AddPostJson()
        {
            var request = (HttpWebRequest) WebRequest.Create(WebServerUrl + ApiPath + "/posts");
            request.Method = "POST";

            using (var dataStream = await request.GetRequestStreamAsync())
            {
                var byteArray = Encoding.UTF8.GetBytes(@"{ ""id"": 4, ""title"": ""tubular2"", ""author"": ""unosquare"" }");
                dataStream.Write(byteArray, 0, byteArray.Length);
            }

            using (var response = (HttpWebResponse) await request.GetResponseAsync())
            {
               Assert.AreEqual(response.StatusCode, HttpStatusCode.OK, "Status Code OK");
            }

            var indexRequest = (HttpWebRequest)WebRequest.Create(WebServerUrl + ApiPath + "/posts");

            using (var response = (HttpWebResponse)await indexRequest.GetResponseAsync())
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
        public async Task PutPostJson()
        {
            var payload = new { id = 1, title = "replace", author = "unosquare" };
            var request = await JsonClient.Put(WebServerUrl + ApiPath + "/posts/1", payload);
            
            var jsonString = await JsonClient.GetString(WebServerUrl + ApiPath + "/posts/1");

            Assert.IsNotEmpty(jsonString);
            dynamic json = Json.Deserialize(jsonString);

            Assert.IsNotNull(json);
            Assert.AreEqual(json["title"].ToString(), "replace");
        }

        [Test]
        public async Task DeletePostJson()
        {
            var indexRequest = (HttpWebRequest)WebRequest.Create(WebServerUrl + ApiPath + "/posts");
            var posts = await JsonClient.GetString(WebServerUrl + ApiPath + "/posts");
            int total;

            var resp = Json.Deserialize<List<object>>(posts);
            total = resp.Count;

            var request = (HttpWebRequest)WebRequest.Create(WebServerUrl + ApiPath + "/posts/3");
            request.Method = "DELETE";

            using (var response = (HttpWebResponse) await request.GetResponseAsync())
            {
                Assert.AreEqual(response.StatusCode, HttpStatusCode.OK, "Status Code OK");
            }

            var jsonString = await JsonClient.GetString(WebServerUrl + ApiPath + "/posts");
            Assert.IsNotEmpty(jsonString);

            var json = Json.Deserialize<List<object>>(jsonString);
            Assert.AreEqual(total - 1, json.Count);
        }
    }
}