﻿namespace Unosquare.Labs.EmbedIO.Extra.Tests
{
    using NUnit.Framework;
    using System.IO;
    using System.Net;
    using System.Net.Http;
    using System.Text;
    using System.Threading.Tasks;
    using TestObjects;
    using JsonServer;
    using Swan.Formatters;
    using Swan.Networking;

    [TestFixture]
    public class JsonServerModuleTest : FixtureBase
    {
        protected const string ApiPath = "api/";

        public JsonServerModuleTest()
            : base(ws => ws.RegisterModule(
                new JsonServerModule("/" + ApiPath, Path.Combine(TestHelper.SetupStaticFolder(), "database.json"))),
                false)
        {
            // placeholder
        }

        [Test]
        public async Task GetAllJson()
        {
            var jsonString = await GetString(ApiPath);
            Assert.IsNotEmpty(jsonString);
            var json = Json.Deserialize(jsonString);
            Assert.IsNotNull(json);
        }

        [Test]
        public async Task GetAllPostsJson()
        {
            var jsonString = await GetString(ApiPath + "posts");
            Assert.IsNotEmpty(jsonString);
            var json = Json.Deserialize(jsonString);
            Assert.IsNotNull(json);
        }

        [Test]
        public async Task GetFirstPostsJson()
        {
            var jsonString = await GetString(ApiPath + "posts/1");
            Assert.IsNotEmpty(jsonString);
            dynamic json = Json.Deserialize(jsonString);
            Assert.IsNotNull(json);
            Assert.AreEqual(json["id"].ToString(), "1");
        }

        [Test]
        public async Task AddPostJson()
        {
            using (var client = new HttpClient())
            {
                var byteArray = Encoding.UTF8.GetBytes(@"{ ""id"": 4, ""title"": ""tubular2"", ""author"": ""unosquare"" }");
                var request = new HttpRequestMessage(HttpMethod.Post, WebServerUrl + ApiPath + "/posts")
                {
                    Content = new ByteArrayContent(byteArray)
                };

                using (var response = await client.SendAsync(request))
                {
                    Assert.AreEqual(response.StatusCode, HttpStatusCode.OK, "Status Code OK");
                }
            }
        }

        [Test]
        public async Task PutPostJson()
        {
            var payload = new { id = 1, title = "replace", author = "unosquare" };
            await JsonClient.Put(WebServerUrl + ApiPath + "/posts/1", payload);

            var jsonString = await GetString(ApiPath + "/posts/1");

            Assert.IsNotEmpty(jsonString);
            dynamic json = Json.Deserialize(jsonString);

            Assert.IsNotNull(json);
            Assert.AreEqual(json["title"].ToString(), "replace");
        }

        [Test]
        public async Task DeletePostJson()
        {
            using (var client = new HttpClient())
            {
                var byteArray = Encoding.UTF8.GetBytes(@"{ ""id"": 123, ""title"": ""tubular2"", ""author"": ""unosquare"" }");
                var request =
                    new HttpRequestMessage(HttpMethod.Post, WebServerUrl + ApiPath + "/posts") { Content = new ByteArrayContent(byteArray) };

                using (var response = await client.SendAsync(request))
                {
                    Assert.AreEqual(response.StatusCode, HttpStatusCode.OK, "Status Code OK");
                }

                var deleteRequest = new HttpRequestMessage(HttpMethod.Delete, WebServerUrl + ApiPath + "/posts/123");

                using (var response = await client.SendAsync(deleteRequest))
                {
                    Assert.AreEqual(response.StatusCode, HttpStatusCode.OK, "Status Code OK");
                }
            }
        }
    }
}