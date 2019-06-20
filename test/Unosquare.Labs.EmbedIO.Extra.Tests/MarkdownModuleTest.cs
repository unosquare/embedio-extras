using System.Threading.Tasks;
using EmbedIO.Extra.Tests.TestObjects;
using EmbedIO.Markdown;
using NUnit.Framework;

namespace EmbedIO.Extra.Tests
{
    [TestFixture]
    public class MarkdownModuleTest : FixtureBase
    {
        public MarkdownModuleTest() 
            : base(ws => ws.WithModule(new MarkdownStaticModule("/", TestHelper.SetupStaticFolder())),
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