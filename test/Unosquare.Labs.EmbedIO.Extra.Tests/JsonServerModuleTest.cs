using EmbedIO.Extra.Tests.TestObjects;
using EmbedIO.JsonServer;
using NUnit.Framework;
using Swan.Formatters;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace EmbedIO.Extra.Tests
{
    [TestFixture]
    public class JsonServerModuleTest : EndToEndFixtureBase
    {
        protected const string ApiPath = "api/";

        protected override void OnSetUp()
        {
            Server.WithModule(new JsonServerModule($"/{ApiPath}", Path.Combine(TestHelper.SetupStaticFolder(), "database.json")));
        }

        [Test]
        public async Task GetAllJson()
        {
            var jsonString = await Client.GetStringAsync(ApiPath);
            Assert.IsNotEmpty(jsonString);
            var json = Json.Deserialize(jsonString);
            Assert.IsNotNull(json);
        }

        [Test]
        public async Task GetAllPostsJson()
        {
            var jsonString = await Client.GetStringAsync(ApiPath + "posts");
            Assert.IsNotEmpty(jsonString);
            var json = Json.Deserialize(jsonString);
            Assert.IsNotNull(json);
        }

        [Test]
        public async Task GetFirstPostsJson()
        {
            var jsonString = await Client.GetStringAsync(ApiPath + "posts/1");
            Assert.IsNotEmpty(jsonString);
            dynamic json = Json.Deserialize(jsonString);
            Assert.IsNotNull(json);
            Assert.AreEqual(json["id"].ToString(), "1");
        }

        [Test]
        public async Task AddPostJson()
        {
            var request = new HttpRequestMessage(HttpMethod.Post, $"{WebServerUrl}{ApiPath}/posts")
            {
                Content = new StringContent(@"{ ""id"": 4, ""title"": ""tubular2"", ""author"": ""unosquare"" }"),
            };

            using var response = await Client.SendAsync(request);
            Assert.AreEqual(response.StatusCode, HttpStatusCode.OK, "Status Code OK");
        }

        [Test]
        public async Task PutPostJson()
        {
            var request = new HttpRequestMessage(HttpMethod.Put, WebServerUrl + ApiPath + "/posts/1")
            {
                Content = new StringContent(@"{ ""id"": 1, ""title"": ""replace"", ""author"": ""unosquare"" }"),
            };

            using var response = await Client.SendAsync(request);
            Assert.AreEqual(response.StatusCode, HttpStatusCode.OK, "Status Code OK");

            var jsonString = await Client.GetStringAsync($"{ApiPath}/posts/1");

            var json = Json.Deserialize<Dictionary<string, string>>(jsonString);

            Assert.IsNotNull(json);
            Assert.AreEqual(json["title"], "replace");
        }

        [Test]
        public async Task DeletePostJson()
        {
            await Client.PostAsync($"{ApiPath}/posts",
                new StringContent(@"{ ""id"": 123, ""title"": ""tubular2"", ""author"": ""unosquare"" }"));

            var response = await Client.DeleteAsync($"{ApiPath}/posts/123");
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, "Status Code OK");
        }
    }
}