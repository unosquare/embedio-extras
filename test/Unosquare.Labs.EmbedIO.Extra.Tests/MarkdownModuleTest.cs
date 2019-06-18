using EmbedIO.Markdown;

namespace Unosquare.Labs.EmbedIO.Extra.Tests
{
    using System.Threading.Tasks;
    using NUnit.Framework;
    using TestObjects;

    [TestFixture]
    public class MarkdownModuleTest : FixtureBase
    {
        public MarkdownModuleTest() 
            : base(ws => ws.RegisterModule(new MarkdownStaticModule(TestHelper.SetupStaticFolder())),
                true)
        {
            // placeholder
        }

        [Test]
        public async Task GetIndex()
        {
            var html = await GetString();

            Assert.AreEqual(html.Replace("\r", string.Empty).Replace("\n", string.Empty),
                Resources.IndexHtml.Replace("\r", string.Empty).Replace("\n", string.Empty));
        }
    }
}