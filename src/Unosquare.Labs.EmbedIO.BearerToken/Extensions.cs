namespace EmbedIO.BearerToken
{
    using Microsoft.IdentityModel.Tokens;
    using Swan.Logging;
    using System;
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
        /// <param name="error">The error.</param>
        public static void Rejected(this IHttpContext context, object? error = null)
        {
            context.Response.Headers.Add(HttpHeaderNames.WWWAuthenticate, "Bearer");

            throw HttpException.Unauthorized(data: error);
        }

        /// <summary>
        /// Gets the <see cref="SecurityToken" /> of the current context.
        /// Returns null when the token is not found or not validated.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="secretKey">The secret key.</param>
        /// <returns>The security token from the HTTP Context.</returns>
        public static SecurityToken? GetSecurityToken(this IHttpContext context, string secretKey)
            => context.GetSecurityToken(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)));

        /// <summary>
        /// Gets the security token.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="secretKey">The secret key.</param>
        /// <returns>The security token from the HTTP Context.</returns>
        public static SecurityToken? GetSecurityToken(this IHttpContext context, SymmetricSecurityKey? secretKey = null)
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
        public static ClaimsPrincipal? GetPrincipal(this IHttpContext context, SymmetricSecurityKey? secretKey = null)
            => context.GetPrincipal(secretKey, out _);

        /// <summary>
        /// Gets the principal.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="secretKey">The secret key.</param>
        /// <param name="securityToken">The security token.</param>
        /// <returns>The claims.</returns>
        public static ClaimsPrincipal? GetPrincipal(
            this IHttpContext context,
            SymmetricSecurityKey? secretKey,
            out SecurityToken? securityToken)
        {
            var authHeader = context!.Request.Headers[HttpHeaderNames.Authorization];

            securityToken = null;

            if (string.IsNullOrWhiteSpace(authHeader) || !authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
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
        /// <param name="this">The webserver.</param>
        /// <param name="baseUrlPath">The base URL path.</param>
        /// <param name="secretKey">The secret key.</param>
        /// <param name="authorizationProvider">The authorization provider.</param>
        /// <returns>
        /// The same web server.
        /// </returns>
        public static IWebServer WithBearerToken(
            this IWebServer @this,
            string baseUrlPath,
            SymmetricSecurityKey secretKey,
            IAuthorizationServerProvider? authorizationProvider = null) =>
            @this.WithModule(
                new BearerTokenModule(
                    baseUrlPath,
                    authorizationProvider ?? new BasicAuthorizationServerProvider(),
                    secretKey));

        /// <summary>
        /// Withes the bearer token.
        /// </summary>
        /// <param name="this">The webserver.</param>
        /// <param name="baseUrlPath">The base URL path.</param>
        /// <param name="secretKeyString">The secret key string.</param>
        /// <param name="authorizationProvider">The authorization provider.</param>
        /// <returns>
        /// The same web server.
        /// </returns>
        public static IWebServer WithBearerToken(
            this IWebServer @this,
            string baseUrlPath,
            string secretKeyString,
            IAuthorizationServerProvider? authorizationProvider = null) =>
            @this.WithModule(
                new BearerTokenModule(
                    baseUrlPath,
                    authorizationProvider ?? new BasicAuthorizationServerProvider(),
                    secretKeyString));
    }
}