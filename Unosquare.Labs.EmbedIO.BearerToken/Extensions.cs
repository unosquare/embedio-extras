using System.Collections.Generic;
using System.Net;

namespace Unosquare.Labs.EmbedIO.BearerToken
{
    /// <summary>
    /// Extension methods
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Retrieves a ValidationContext
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static ValidateClientAuthenticationContext GetValidationContext(this HttpListenerContext context)
        {
            return new ValidateClientAuthenticationContext(context);
        }

        /// <summary>
        /// Rejects a authentication challenge
        /// </summary>
        /// <param name="context"></param>
        public static void Rejected(this HttpListenerContext context)
        {
            context.JsonResponse("{'error': 'invalid_grant'}");
            context.Response.StatusCode = (int) HttpStatusCode.Unauthorized;
        }

        /// <summary>
        /// Fluent-like method to attach BearerToken
        /// </summary>
        /// <param name="webserver"></param>
        /// <param name="authorizationProvider"></param>
        /// <param name="routes"></param>
        /// <param name="secretKey"></param>
        public static void UseBearerToken(this WebServer webserver,
            IAuthorizationServerProvider authorizationProvider = null, IEnumerable<string> routes = null,
            string secretKey = "0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9eyJjbGF")
        {
            webserver.RegisterModule(
                new BearerTokenModule(authorizationProvider ?? new BasicAuthorizationServerProvider(), routes, secretKey));
        }
    }
}