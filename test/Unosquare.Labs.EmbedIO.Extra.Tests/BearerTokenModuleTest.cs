namespace Unosquare.Labs.EmbedIO.Extra.Tests
{
    using System;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
    using NUnit.Framework;
    using BearerToken;
    using TestObjects;
    using Markdown;
    using Swan.Formatters;
    using System.Net.Http;
    using System.Collections.Generic;
    using System.Net;
#if NET46
    using Unosquare.Net;
#else
    using System.Net;
#endif

    [TestFixture]
    public class BearerTokenModuleTest : FixtureBase
    {
        protected BasicAuthorizationServerProvider BasicProvider = new BasicAuthorizationServerProvider();

        public BearerTokenModuleTest()
            : base((ws) => 
            {
                ws.RegisterModule(new BearerTokenModule(new BasicAuthorizationServerProvider()));
                ws.RegisterModule(new MarkdownStaticModule(TestHelper.SetupStaticFolder()));
            })
        {
        }
        
#if !NET46
        [Test]
        public void TestBasicAuthorizationServerProvider()
        {
            Assert.Throws<ArgumentNullException>(() => BasicProvider
                .ValidateClientAuthentication(new ValidateClientAuthenticationContext(null))
                .RunSynchronously());
        }
#endif

        [Test]
        public async Task GetInvalidToken()
        {
            var payload = System.Text.Encoding.UTF8.GetBytes("grant_type=nothing");
            using (var client = new HttpClient())
            {
                var req = new HttpRequestMessage(HttpMethod.Post, WebServerUrl + "token") { Content = new ByteArrayContent(payload) };
                var res = await client.SendAsync(req);

                Assert.AreEqual(res.StatusCode, HttpStatusCode.Unauthorized);
            }                
        }

        [Test]
        public async Task GetValidToken()
        {
            string token;
            var payload = System.Text.Encoding.UTF8.GetBytes("grant_type=password&username=test&password=test");
            using (var client = new HttpClient())
            {
                var req = new HttpRequestMessage(HttpMethod.Post, WebServerUrl + "token") { Content = new ByteArrayContent(payload) };
                using (var res = await client.SendAsync(req))
                {
                    Assert.AreEqual(res.StatusCode, HttpStatusCode.OK);
                    var jsonString = await res.Content.ReadAsStringAsync();
                    var json = Json.Deserialize<BearerToken>(jsonString);
                    Assert.IsNotNull(json);
                    Assert.IsNotEmpty(json.Token);
                    Assert.IsNotEmpty(json.Username);
                    token = json.Token;
                }

                var indexRequest = new HttpRequestMessage(HttpMethod.Post, WebServerUrl + "index.html");

                using (var indexResponse = await client.SendAsync(indexRequest))
                {
                    Assert.AreEqual(indexResponse.StatusCode, System.Net.HttpStatusCode.Unauthorized);
                }

                indexRequest = new HttpRequestMessage(HttpMethod.Post, WebServerUrl + "index.html");
                indexRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                using (var indexResponse = await client.SendAsync(indexRequest))
                {
                    Assert.AreEqual(indexResponse.StatusCode, System.Net.HttpStatusCode.OK);
                }
            }
        }
    }
}