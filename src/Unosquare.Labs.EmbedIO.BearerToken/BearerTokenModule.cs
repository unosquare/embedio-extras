namespace Unosquare.Labs.EmbedIO.BearerToken
{
    using Microsoft.IdentityModel.Tokens;
    using System.Threading.Tasks;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Swan;

    /// <summary>
    /// EmbedIO module to allow authorizations with Bearer Tokens
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
                if (routes != null && routes.Contains(context.RequestPath()) == false) return Task.FromResult(false);

                // decode token to see if it's valid
                if (context.GetSecurityToken(secretKey) != null)
                {
                    return Task.FromResult(false);
                }

                context.Rejected();

                return Task.FromResult(true);
            });
        }

        /// <summary>
        /// Returns Module Name
        /// </summary>
        public override string Name => nameof(BearerTokenModule).Humanize();
    }
}
