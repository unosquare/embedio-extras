using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.Extensions.DependencyInjection;

namespace EmbedIO.AspNetCore
{
    public static class WebHostBuilderExtensions
    {
        public static IWebHostBuilder UseEmbedIO(this IWebHostBuilder hostBuilder)
            => hostBuilder.ConfigureServices(services => { services.AddSingleton<IServer, EmbedIOServer>(); });
    }
}