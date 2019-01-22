namespace Unosquare.Labs.EmbedIO.BearerToken
{
    using Microsoft.IdentityModel.Tokens;
    using Swan;
    using System;
    using System.Collections.Generic;
    using System.IdentityModel.Tokens.Jwt;
    using System.Security.Claims;
    using System.Text;

    /// <summary>
    /// Extension methods.
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Retrieves a ValidationContext.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns>The Validation Context from the HTTP Context.</returns>
        public static ValidateClientAuthenticationContext GetValidationContext(this IHttpContext context)
            => new ValidateClientAuthenticationContext(context);

        /// <summary>
        /// Rejects a authentication challenge.
        /// </summary>
        /// <param name="context">The context.</param>
        public static void Rejected(this IHttpContext context)
        {
            context.Response.StatusCode = (int) System.Net.HttpStatusCode.Unauthorized;
            context.Response.AddHeader("WWW-Authenticate", "Bearer");
        }

        /// <summary>
        /// Gets the <see cref="SecurityToken" /> of the current context.
        /// Returns null when the token is not found or not validated.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="secretKey">The secret key.</param>
        /// <returns>The security token from the HTTP Context.</returns>
        public static SecurityToken GetSecurityToken(this IHttpContext context, string secretKey)
            => context.GetSecurityToken(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)));

        /// <summary>
        /// Gets the security token.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="secretKey">The secret key.</param>
        /// <returns>The security token from the HTTP Context.</returns>
        public static SecurityToken GetSecurityToken(this IHttpContext context, SymmetricSecurityKey secretKey = null)
        {
            context.GetPrincipal(secretKey, out var securityToken);

            return securityToken;
        }

        /// <summary>
        /// Gets the principal.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="secretKey">The secret key.</param>
        /// <returns>The security token from the HTTP Context.</returns>
        public static ClaimsPrincipal GetPrincipal(this IHttpContext context, SymmetricSecurityKey secretKey = null)
            => context.GetPrincipal(secretKey, out _);

        /// <summary>
        /// Gets the principal.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="secretKey">The secret key.</param>
        /// <param name="securityToken">The security token.</param>
        /// <returns>The claims.</returns>
        public static ClaimsPrincipal GetPrincipal(
            this IHttpContext context,
            SymmetricSecurityKey secretKey,
            out SecurityToken securityToken)
        {
            var authHeader = context.RequestHeader("Authorization");
            securityToken = null;

            if (string.IsNullOrWhiteSpace(authHeader) || !authHeader.StartsWith("Bearer "))
            {
                return null;
            }

            try
            {
                var token = authHeader.Replace("Bearer ", string.Empty);
                var tokenHandler = new JwtSecurityTokenHandler();
                var tokenParams = new TokenValidationParameters
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    IssuerSigningKey = secretKey,
                };

                return tokenHandler.ValidateToken(token, tokenParams, out securityToken);
            }
            catch (Exception ex)
            {
                securityToken = null;
                ex.Log(nameof(BearerTokenModule));
            }

            return null;
        }

        /// <summary>
        /// Fluent-like method to attach BearerToken.
        /// </summary>
        /// <param name="webserver">The webserver.</param>
        /// <param name="authorizationProvider">The authorization provider.</param>
        /// <param name="routes">The routes.</param>
        /// <param name="secretKey">The secret key.</param>
        /// <returns>The same web server.</returns>
        public static IWebServer UseBearerToken(this IWebServer webserver,
            IAuthorizationServerProvider authorizationProvider = null,
            IEnumerable<string> routes = null,
            SymmetricSecurityKey secretKey = null)
        {
            webserver.RegisterModule(
                new BearerTokenModule(
                    authorizationProvider ?? new BasicAuthorizationServerProvider(), 
                    routes,
                    secretKey));

            return webserver;
        }
    }
}