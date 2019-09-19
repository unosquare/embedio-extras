using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using EmbedIO.Extra.Tests.TestObjects;
using EmbedIO.LiteLibWebApi;
using NUnit.Framework;
using Swan.Formatters;
using Swan.Net;

namespace EmbedIO.Extra.Tests
{
    [TestFixture]
    public class LiteLibModuleTest : EndToEndFixtureBase
    {
        protected const string ApiPath = "dbapi";

        protected override void OnSetUp()
        {
            Server.WithModule(
                new LiteLibModule<TestDbContext>(new TestDbContext(), $"/{ApiPath}/"));
        }

        [Test]
        public async Task GetAllLiteLib()
        {
            var response = await Client.GetStringAsync($"{ApiPath}/order");

            Assert.IsNotNull(response);

            var orders = Json.Deserialize<List<Order>>(response);

            Assert.Greater(orders.Count, 0, "Orders count is greater than 0");
        }

        [Test]
        public async Task GetFirstItemLiteLib()
        {
            var response = await Client.GetStringAsync($"{ApiPath}/order/1");

            Assert.IsNotNull(response);

            var orders = Json.Deserialize<Order>(response);

            Assert.AreEqual(1, orders.RowId, "Order's RowId equals 1");
        }

        [Test]
        public async Task AddLiteLib()
        {
            var getAllResponse = await Client.GetStringAsync(ApiPath + "/order");
            var orders = Json.Deserialize<List<Order>>(getAllResponse).Count;

            var newOrder = new Order
            {
                CustomerName = "UnoLabs",
                ShipperCity = "GDL",
                ShippedDate = "2017-03-20",
                Amount = 100,
                IsShipped = false
            };

            Client.PostAsync(ApiPath + "/order", new StringContent(Json.Serialize(newOrder)));

            getAllResponse = await Client.GetStringAsync(ApiPath + "/order");
            var ordersPlusOne = Json.Deserialize<List<Order>>(getAllResponse).Count;

            Assert.AreEqual(orders + 1, ordersPlusOne);
        }

        [Test]
        public async Task PutLiteLib()
        {
            var order = new Order
            {
                CustomerName = "UnoLabs",
                ShipperCity = "Zapopan",
                ShippedDate = "2017-03-22",
                Amount = 200,
                IsShipped = true
            };
            
            Client.PutAsync(ApiPath + "/order/1", new StringContent(Json.Serialize(order)));

            var response = await Client.GetStringAsync(ApiPath + "/order/1");
            var result = Json.Deserialize<Order>(response);

            Assert.AreEqual(result.ShipperCity, "Zapopan");
            Assert.AreEqual(result.Amount, 200);
            Assert.AreEqual(result.ShippedDate, "2017-03-22");
            Assert.AreEqual(result.IsShipped, true);
        }

        [Test]
        public async Task DeleteLiteLib()
        {
            var response = await Client.GetStringAsync(ApiPath + "/order");

            Assert.IsNotNull(response);

            var order = Json.Deserialize<List<Order>>(response);
            var last = order.Last().RowId;
            var count = order.Count;

            var request = new HttpRequestMessage(HttpMethod.Delete, WebServerUrl + ApiPath + "/order/" + last);

            using (var webResponse = await Client.SendAsync(request))
            {
                Assert.AreEqual(webResponse.StatusCode, HttpStatusCode.OK, "Status Code OK");
            }

            response = await Client.GetStringAsync(ApiPath + "/order");
            var newCount = Json.Deserialize<List<Order>>(response).Count;

            Assert.AreEqual(newCount, count - 1);
        }
    }
}
