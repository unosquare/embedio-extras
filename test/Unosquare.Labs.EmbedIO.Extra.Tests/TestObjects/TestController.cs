using EmbedIO.Routing;
using EmbedIO.WebApi;

namespace EmbedIO.Extra.Tests.TestObjects
{
    public class TestController : WebApiController
    {
        [Route(HttpVerbs.Get, "/user")]
        public string GetUserName() =>
            HttpContext.User.Identity.Name;
    }
}
