﻿using System;
using System.IO;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using Microsoft.Owin.Hosting;
using NUnit.Framework;
using Owin;
using Unosquare.Labs.EmbedIO.OwinMiddleware;
using System.Threading.Tasks;

namespace Unosquare.Labs.EmbedIO.Owin.Tests
{
    public class EmbedioOwinTest
    {
        protected IDisposable WebServer;
        protected string WebServerUrl;

        [SetUp]
        public void Init()
        {
            var options = new StartOptions
            {
                ServerFactory = OwinServerFactory.ServerFactoryName,
                Port = 7777
            };

            WebServer = WebApp.Start(options, (app) => app
                .UseDirectoryBrowser()
                .UseWebApi(typeof(TestController).Assembly)
                .UseEmbedIOCors());
        }

        [Test]
        public async Task TestWebSocket()
        {
            var webSocketClient = new ClientWebSocket();
            var cts = new CancellationTokenSource();

            await webSocketClient.ConnectAsync(new Uri("ws://localhost:7777/chat"), cts.Token);

            var encoded = Encoding.UTF8.GetBytes("HELO");
            var bufferSend = new ArraySegment<byte>(encoded, 0, encoded.Length);
            await webSocketClient.SendAsync(bufferSend, WebSocketMessageType.Text, true, cts.Token);

            if (webSocketClient.State != WebSocketState.Open) return;

            using (var ms = new MemoryStream())
            {
                var buffer = new ArraySegment<byte>(new byte[8192]);
                WebSocketReceiveResult result;

                do
                {
                    result = await webSocketClient.ReceiveAsync(buffer, CancellationToken.None);
                    ms.Write(buffer.Array, buffer.Offset, result.Count);
                } while (!result.EndOfMessage);

                ms.Seek(0, SeekOrigin.Begin);

                if (result.MessageType != WebSocketMessageType.Text) return;

                using (var reader = new StreamReader(ms, Encoding.UTF8))
                {
                    var message = reader.ReadToEnd();
                    Assert.AreEqual(message, "Welcome to the chat room!");
                }
            }
        }

        [Test]
        public void GetIndex()
        {
            var request = (HttpWebRequest) WebRequest.Create("http://localhost:7777");

            using (var response = (HttpWebResponse) request.GetResponse())
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
            var request = (HttpWebRequest) WebRequest.Create("http://localhost:7777/error");

            try
            {
                // By design GetResponse throws exception with NotModified status, weird
                request.GetResponse();
            }
            catch (WebException ex)
            {
                if (ex.Response == null || ex.Status != WebExceptionStatus.ProtocolError)
                    throw;

                var response = (HttpWebResponse) ex.Response;

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