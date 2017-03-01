namespace Unosquare.Labs.EmbedIO.BearerToken
{
    using System.Collections.Generic;
    using Microsoft.IdentityModel.Tokens;
#if NET46
    using System.Net;
#else
    using Unosquare.Net;
#endif

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
            context.Response.StatusCode = (int)System.Net.HttpStatusCode.Unauthorized;
            context.Response.AddHeader("WWW-Authenticate", "Bearer");
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
            SymmetricSecurityKey secretKey = null)
        {
            webserver.RegisterModule(
                new BearerTokenModule(authorizationProvider ?? new BasicAuthorizationServerProvider(), routes, secretKey));
        }
    }
}