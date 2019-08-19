using EmbedIO.AspNetCore;

namespace Unosquare.EmbedIO.AspNetCore.Sample
{
    using Microsoft.AspNetCore.Hosting;

    public class Program
    {
        public static void Main(string[] args)
        {
            var host = new WebHostBuilder()
                .UseEmbedIO()
                .UseStartup<Startup>()
                .Build();

            host.Run();
        }
    }
}