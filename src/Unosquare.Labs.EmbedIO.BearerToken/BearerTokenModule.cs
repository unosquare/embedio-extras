namespace Unosquare.Labs.EmbedIO.BearerToken
{
    using System;
    using Microsoft.IdentityModel.Tokens;
    using System.Threading.Tasks;
    using System.Collections.Generic;
    using System.Text;
    using Constants;

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
            if (secretKey == null)
            {
                // TODO: Make secretKey parameter mandatory and and an overload that takes in a string for a secretKey
                secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9eyJjbGF"));
            }
            
            AddHandler(endpoint, HttpVerbs.Post, (context, ct) =>
            {
                var validationContext = context.GetValidationContext();
                authorizationServerProvider.ValidateClientAuthentication(validationContext);

                if (validationContext.IsValidated)
                {
                    context.JsonResponse(new BearerToken
                    {
                        Token = validationContext.GetToken(secretKey),
                        TokenType = "bearer",
                        ExpirationDate = authorizationServerProvider.GetExpirationDate(),
                        Username = validationContext.ClientId
                    });
                }
                else
                {
                    context.Rejected();
                }

                return Task.FromResult(true);
            });

            AddHandler(ModuleMap.AnyPath, HttpVerbs.Any, (context, ct) =>
            {
                if (routes != null) 
                {
                    var path = context.RequestPath();
                    var match = false;
                    foreach (var p in routes) 
                    {
                        var wildcard = p.IndexOf(ModuleMap.AnyPath, StringComparison.Ordinal);
                        
                        if ((wildcard == -1 && p.Equals(path))
                            || (wildcard != -1
                                && (
                                   // wildcard at the end
                                   path.StartsWith(p.Substring(0, p.Length - ModuleMap.AnyPath.Length))
                                   // wildcard in the middle so check both start/end
                                   || (path.StartsWith(p.Substring(0, wildcard))
                                       && path.EndsWith(p.Substring(wildcard + 1)))
                                   )
                                )
                        ) {
                            match = true;
                            break;
                        }
                    }

                    if (!match) 
                    {
                        return Task.FromResult(false);
                    }
                }

                // decode token to see if it's valid
                if (context.GetSecurityToken(secretKey) != null)
                {
                    return Task.FromResult(false);
                }

                context.Rejected();

                return Task.FromResult(true);
            });
        }

        /// <inheritdoc />
        public override string Name => nameof(BearerTokenModule);
    }
}
