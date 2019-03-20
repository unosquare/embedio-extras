namespace Unosquare.Labs.EmbedIO.BearerToken
{
    using Constants;
    using Microsoft.IdentityModel.Tokens;
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// EmbedIO module to allow authorizations with Bearer Tokens.
    /// </summary>
    public class BearerTokenModule : WebModuleBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BearerTokenModule"/> class.
        /// </summary>
        /// <param name="authorizationServerProvider">The authorization server provider.</param>
        /// <param name="routes">The routes.</param>
        /// <param name="secretKey">The secret key.</param>
        /// <param name="endpoint">The endpoint.</param>
        public BearerTokenModule(
            IAuthorizationServerProvider authorizationServerProvider,
            IEnumerable<string> routes = null, 
            SymmetricSecurityKey secretKey = null, 
            string endpoint = "/token")
        {
            // TODO: Make secretKey parameter mandatory and and an overload that takes in a string for a secretKey
            SecretKey = secretKey ?? new SymmetricSecurityKey(Encoding.UTF8.GetBytes("0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9eyJjbGF"));
            
            AddHandler(endpoint, HttpVerbs.Post, async (context, ct) =>
            {
                var validationContext = context.GetValidationContext();
                await authorizationServerProvider.ValidateClientAuthentication(validationContext);

                if (!validationContext.IsValidated) 
                    return await context.Rejected(validationContext.ErrorPayload);

                var expiryDate = DateTime.SpecifyKind(DateTime.FromBinary(authorizationServerProvider.GetExpirationDate()), DateTimeKind.Utc);

                return await context.JsonResponseAsync(new BearerToken
                {
                    Token = validationContext.GetToken(SecretKey, expiryDate),
                    TokenType = "bearer",
                    ExpirationDate = authorizationServerProvider.GetExpirationDate(),
                    Username = validationContext.IdentityName,
                });
            });

            AddHandler(ModuleMap.AnyPath, HttpVerbs.Any, (context, ct) =>
            {
                if (routes != null)
                {
                    if (!Match(routes, context)) 
                    {
                        return Task.FromResult(false);
                    }
                }

                // decode token to see if it's valid
                if (context.GetSecurityToken(SecretKey) != null)
                {
                    return Task.FromResult(false);
                }

                context.Rejected();

                return Task.FromResult(true);
            });
        }

        /// <summary>
        /// Gets the secret key.
        /// </summary>
        /// <value>
        /// The secret key.
        /// </value>
        public SymmetricSecurityKey SecretKey { get; }

        /// <inheritdoc />
        public override string Name => nameof(BearerTokenModule);
        private static bool Match(IEnumerable<string> routes, IHttpContext context)
        {
            var path = context.RequestPath();
            var match = false;

            foreach (var p in routes)
            {
                var wildcard = p.IndexOf(ModuleMap.AnyPath, StringComparison.Ordinal);

                if ((wildcard == -1 && p == path)
                    || (wildcard != -1
                        && (
                            // wildcard at the end
                            path.StartsWith(p.Substring(0, p.Length - ModuleMap.AnyPath.Length), StringComparison.OrdinalIgnoreCase)
                            // wildcard in the middle so check both start/end
                            || (path.StartsWith(p.Substring(0, wildcard), StringComparison.OrdinalIgnoreCase)
                                && path.EndsWith(p.Substring(wildcard + 1), StringComparison.OrdinalIgnoreCase))
                        )
                    )
                )
                {
                    match = true;
                    break;
                }
            }

            return match;
        }
    }
}
