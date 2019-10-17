namespace EmbedIO.ExtraSample.Controller
{
    using EmbedIO.Routing;
    using EmbedIO.WebApi;
    using System.Threading.Tasks;

    public class WebController : WebApiController
    {
        public WebController() { }

        [Route(HttpVerbs.Get, "/Home")]
        public async Task<object> Home() => new { value = "holi" };
    }
}
