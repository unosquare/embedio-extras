using System.Threading;

namespace Unosquare.Labs.EmbedIO.Owin.Tests
{
    public static class Resources
    {
        private const string ServerAddress = "http://localhost:{0}/";
        public static int Counter = 9699;

        public static string GetServerAddress()
        {
            Interlocked.Increment(ref Counter);
            return string.Format(ServerAddress, Counter);
        }
    }
}
