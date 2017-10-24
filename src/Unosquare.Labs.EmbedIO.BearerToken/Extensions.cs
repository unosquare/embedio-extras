namespace Unosquare.Labs.EmbedIO.BearerToken
{
    using System;
    using System.Text;
    using System.Collections.Generic;
    using Microsoft.IdentityModel.Tokens;
    using System.IdentityModel.Tokens.Jwt;
    using Swan;
#if NET46
    using Net;
#else
    using System.Net;
#endif

    /// <summary>
    /// Extension methods
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Retrieves a ValidationContext
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns>The Validation Context from the HTTP Context</returns>
        public static ValidateClientAuthenticationContext GetValidationContext(this HttpListenerContext context)
         => new ValidateClientAuthenticationContext(context);

        /// <summary>
        /// Rejects a authentication challenge
        /// </summary>
        /// <param name="context">The context.</param>
        public static void Rejected(this HttpListenerContext context)
        {
            context.Response.StatusCode = (int)System.Net.HttpStatusCode.Unauthorized;
            context.Response.AddHeader("WWW-Authenticate", "Bearer");
        }

        /// <summary>
        /// Gets the <see cref="SecurityToken" /> of the current context.
        /// Returns null when the token is not found or not validated.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="secretKey">The secret key.</param>
        /// <returns>The security token from the HTTP Context</returns>
        public static SecurityToken GetSecurityToken(this HttpListenerContext context, string secretKey = null)
        => context.GetSecurityToken(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)));

        /// <summary>
        /// Gets the security token.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="secretKey">The secret key.</param>
        /// <returns>The security token from the HTTP Context</returns>
        public static SecurityToken GetSecurityToken(this HttpListenerContext context, SymmetricSecurityKey secretKey = null)
        {
            var authHeader = context.RequestHeader("Authorization");

            if (string.IsNullOrWhiteSpace(authHeader) || !authHeader.StartsWith("Bearer "))
            {
                return null;
            }

            try
            {
                var token = authHeader.Replace("Bearer ", string.Empty);
                var tokenHandler = new JwtSecurityTokenHandler();
                var tokenParmas = new TokenValidationParameters
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    IssuerSigningKey = secretKey
                };

                tokenHandler.ValidateToken(token, tokenParmas, out var validatedToken);
                return validatedToken;
            }
            catch (Exception ex)
            {
                ex.Log("BearerTokenModule");
            }

            return null;
        }

        /// <summary>
        /// Fluent-like method to attach BearerToken
        /// </summary>
        /// <param name="webserver">The webserver.</param>
        /// <param name="authorizationProvider">The authorization provider.</param>
        /// <param name="routes">The routes.</param>
        /// <param name="secretKey">The secret key.</param>
        public static void UseBearerToken(this WebServer webserver,
            IAuthorizationServerProvider authorizationProvider = null,
            IEnumerable<string> routes = null,
            SymmetricSecurityKey secretKey = null)
        {
            webserver.RegisterModule(
                new BearerTokenModule(authorizationProvider ?? new BasicAuthorizationServerProvider(), routes, secretKey));
        }
    }
}
