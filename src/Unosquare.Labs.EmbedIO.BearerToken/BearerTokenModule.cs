namespace EmbedIO.BearerToken
{
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
        private readonly string _tokenEndpoint;
        private readonly IEnumerable<string> _routes;
        private readonly IAuthorizationServerProvider _authorizationServerProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="BearerTokenModule" /> class.
        /// </summary>
        /// <param name="baseUrlPath">The base URL path.</param>
        /// <param name="authorizationServerProvider">The authorization server provider.</param>
        /// <param name="secretKey">The secret key.</param>
        /// <param name="routes">The routes.</param>
        /// <param name="endpoint">The endpoint.</param>
        public BearerTokenModule(
            string baseUrlPath,
            IAuthorizationServerProvider authorizationServerProvider,
            SymmetricSecurityKey secretKey,
            IEnumerable<string>? routes = null,
            string endpoint = "/token")
            : base(baseUrlPath)
        {
            SecretKey = secretKey ?? new SymmetricSecurityKey(Encoding.UTF8.GetBytes("0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9eyJjbGF"));
            _tokenEndpoint = endpoint;
            _routes = routes;
            _authorizationServerProvider = authorizationServerProvider;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BearerTokenModule" /> class.
        /// </summary>
        /// <param name="baseUrlPath">The base URL path.</param>
        /// <param name="authorizationServerProvider">The authorization server provider.</param>
        /// <param name="secretKeyString">The secret key string.</param>
        /// <param name="routes">The routes.</param>
        /// <param name="endpoint">The endpoint.</param>
        /// <exception cref="ArgumentException">A key must be 40 chars</exception>
        public BearerTokenModule(
            string baseUrlPath,
            IAuthorizationServerProvider authorizationServerProvider,
            string secretKeyString,
            IEnumerable<string>? routes = null,
            string endpoint = "/token")
            : this(
                baseUrlPath,
                authorizationServerProvider,
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKeyString)),
                routes,
                endpoint)
        {
            if (secretKeyString.Length != 40)
                throw new ArgumentException("A key must be 40 chars");
        }

        /// <summary>
        /// Gets the secret key.
        /// </summary>
        /// <value>
        /// The secret key.
        /// </value>
        public SymmetricSecurityKey SecretKey { get; }

        /// <inheritdoc />
        public override bool IsFinalHandler => false;

        //This method is used to validate the root of the request match some of the registered routes.
        private bool Match(string path)
        {
            var match = false;

            foreach (var p in _routes)
            {
                var wildcard = p.IndexOf("*", StringComparison.Ordinal);

                if ((wildcard == -1 && p == path)
                    || (wildcard != -1
                        && (
                            // wildcard at the end
                            path.StartsWith(p.Substring(0, p.Length - 1), StringComparison.OrdinalIgnoreCase)
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

        /// <inheritdoc />
        protected override async Task OnRequestAsync(IHttpContext context)
        {
            //The problem is in some point of this function. :(

            if (context.RequestedPath == _tokenEndpoint && context.Request.HttpVerb == HttpVerbs.Post)
            {
                await OnTokenRequest(context); //this is for log in 
                return;
            }

            //if (_routes != null)
            //{
            //    if (!Match(context.RequestedPath))
            //    {
            //        return;
            //    }
            //}

            // decode token to see if it's valid
            if (context.GetSecurityToken(SecretKey) != null)//!= null) //validate that token is present and token is valid.
            {
                return;
            }

            //If the token is not valid or is not present in the request should be rejected.
            context.Rejected();
            context.SetHandled();
        }

        private async Task OnTokenRequest(IHttpContext context)
        {
            context.SetHandled();

            var validationContext = context.GetValidationContext();
            await _authorizationServerProvider.ValidateClientAuthentication(validationContext);

            if (!validationContext.IsValidated)
            {
                context.Rejected(validationContext.ErrorPayload);
                return;
            }

            var expiryDate = DateTime.SpecifyKind(
                DateTime.FromBinary(_authorizationServerProvider.GetExpirationDate()),
                DateTimeKind.Utc);

            await context.SendDataAsync(
                new BearerToken
                {
                    Token = validationContext.GetToken(SecretKey, expiryDate),
                    TokenType = "bearer",
                    ExpirationDate = _authorizationServerProvider.GetExpirationDate(),
                    Username = validationContext.IdentityName,
                });
        }
    }
}
