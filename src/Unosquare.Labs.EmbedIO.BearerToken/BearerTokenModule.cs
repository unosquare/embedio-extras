namespace EmbedIO.BearerToken
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.IdentityModel.Tokens;

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
        /// <param name="routes">The routes.</param>
        /// <param name="secretKey">The secret key.</param>
        /// <param name="endpoint">The endpoint.</param>
        public BearerTokenModule(
            string baseUrlPath,
            IAuthorizationServerProvider authorizationServerProvider,
            IEnumerable<string> routes = null,
            SymmetricSecurityKey secretKey = null,
            string endpoint = "/token")
            : base(baseUrlPath)
        {
            // TODO: Make secretKey parameter mandatory and and an overload that takes in a string for a secretKey
            SecretKey = secretKey ?? new SymmetricSecurityKey(Encoding.UTF8.GetBytes("0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9eyJjbGF"));
            _tokenEndpoint = endpoint;
            _routes = routes;
            _authorizationServerProvider = authorizationServerProvider;
        }

        /// <summary>
        /// Gets the secret key.
        /// </summary>
        /// <value>
        /// The secret key.
        /// </value>
        public SymmetricSecurityKey SecretKey { get; }

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
        protected override async Task<bool> OnRequestAsync(IHttpContext context, string path, CancellationToken cancellationToken)
        {
            if (path == _tokenEndpoint && context.Request.HttpVerb == HttpVerbs.Post)
            {
                var validationContext = context.GetValidationContext();
                await _authorizationServerProvider.ValidateClientAuthentication(validationContext);

                if (!validationContext.IsValidated)
                    return await context.Rejected(validationContext.ErrorPayload);

                var expiryDate = DateTime.SpecifyKind(DateTime.FromBinary(_authorizationServerProvider.GetExpirationDate()), DateTimeKind.Utc);

                return await context.SendDataAsync(
                    new BearerToken
                    {
                        Token = validationContext.GetToken(SecretKey, expiryDate),
                        TokenType = "bearer",
                        ExpirationDate = _authorizationServerProvider.GetExpirationDate(),
                        Username = validationContext.IdentityName,
                    },
                    cancellationToken);
            }

            if (_routes != null)
            {
                if (!Match(path))
                {
                    return false;
                }
            }

            // decode token to see if it's valid
            if (context.GetSecurityToken(SecretKey) != null)
            {
                return false;
            }

            context.Rejected(cancellationToken: cancellationToken);

            return true;
        }
    }
}
