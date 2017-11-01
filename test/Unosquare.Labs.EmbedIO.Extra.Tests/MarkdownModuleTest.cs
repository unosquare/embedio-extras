using NUnit.Framework;
using System.Threading.Tasks;
using Unosquare.Labs.EmbedIO.Extra.Tests.TestObjects;
using Unosquare.Labs.EmbedIO.Markdown;

namespace Unosquare.Labs.EmbedIO.Extra.Tests
{
    [TestFixture]
    public class MarkdownModuleTest : FixtureBase
    {
        public MarkdownModuleTest() 
            : base(ws => ws.RegisterModule(new MarkdownStaticModule(TestHelper.SetupStaticFolder())))
        {
            // placeholder
        }

        [Test]
        public async Task GetIndex()
        {
            var html = await GetString(string.Empty);

            Assert.AreEqual(html.Replace("\r", string.Empty).Replace("\n", string.Empty),
                Resources.IndexHtml.Replace("\r", string.Empty).Replace("\n", string.Empty));
        }
    }
}