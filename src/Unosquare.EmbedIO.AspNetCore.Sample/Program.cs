using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using System;
using System.IO;

namespace Unosquare.EmbedIO.AspNetCore.Sample
{
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
