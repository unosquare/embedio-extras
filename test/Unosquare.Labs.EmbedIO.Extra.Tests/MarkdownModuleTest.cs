using System.Threading.Tasks;
using EmbedIO.Extra.Tests.TestObjects;
using EmbedIO.Markdown;
using NUnit.Framework;

namespace EmbedIO.Extra.Tests
{
    [TestFixture]
    public class MarkdownModuleTest : EndToEndFixtureBase
    {
        protected override void OnSetUp() => Server.WithModule(new MarkdownStaticModule("/", TestHelper.SetupStaticFolder()));

        [Test]
        public async Task GetIndex()
        {
            var html = await Client.GetStringAsync("/");

            Assert.AreEqual(html.Replace("\r", string.Empty).Replace("\n", string.Empty),
                Resources.IndexHtml.Replace("\r", string.Empty).Replace("\n", string.Empty));
        }
    }
}