namespace EmbedIO.BearerToken
{
    using Microsoft.IdentityModel.Tokens;
    using System;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// EmbedIO module to allow authorizations with Bearer Tokens.
    /// </summary>
    public class BearerTokenModule : WebModuleBase
    {
        private readonly string _tokenEndpoint;
        private readonly IAuthorizationServerProvider _authorizationServerProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="BearerTokenModule" /> class.
        /// </summary>
        /// <param name="baseUrlPath">The base URL path.</param>
        /// <param name="authorizationServerProvider">The authorization server provider.</param>
        /// <param name="secretKey">The secret key.</param>
        /// <param name="endpoint">The endpoint for the authorization (relative to baseUrlPath).</param>
        public BearerTokenModule(
            string baseUrlPath,
            IAuthorizationServerProvider authorizationServerProvider,
            SymmetricSecurityKey secretKey,
            string endpoint = "/token")
            : base(baseUrlPath)
        {
            SecretKey = secretKey ?? new SymmetricSecurityKey(Encoding.UTF8.GetBytes("0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9eyJjbGF"));
            _tokenEndpoint = endpoint;
            _authorizationServerProvider = authorizationServerProvider;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BearerTokenModule" /> class.
        /// </summary>
        /// <param name="baseUrlPath">The base URL path.</param>
        /// <param name="authorizationServerProvider">The authorization server provider.</param>
        /// <param name="secretKeyString">The secret key string.</param>
        /// <param name="endpoint">The endpoint for the authorization (relative to baseUrlPath).</param>
        /// <exception cref="ArgumentNullException">secretKeyString</exception>
        /// <exception cref="ArgumentException">A key must be 40 chars.</exception>
        public BearerTokenModule(
            string baseUrlPath,
            IAuthorizationServerProvider authorizationServerProvider,
            string secretKeyString,
            string endpoint = "/token")
            : this(
                baseUrlPath,
                authorizationServerProvider,
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKeyString)),
                endpoint)
        {
            if (secretKeyString == null)
                throw new ArgumentNullException(nameof(secretKeyString));

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

        /// <inheritdoc />
        protected override async Task OnRequestAsync(IHttpContext context)
        {
            if (context!.RequestedPath == _tokenEndpoint && context.Request.HttpVerb == HttpVerbs.Post)
            {
                await OnTokenRequest(context).ConfigureAwait(false);
                return;
            }

            // decode token to see if it's valid
            if (context.GetSecurityToken(SecretKey) != null)
            {
                return;
            }

            context.Rejected();
            context.SetHandled();
        }

        private async Task OnTokenRequest(IHttpContext context)
        {
            context.SetHandled();

            var validationContext = context.GetValidationContext();
            await _authorizationServerProvider.ValidateClientAuthentication(validationContext)
                .ConfigureAwait(false);

            if (!validationContext.IsValidated)
            {
                context.Rejected(validationContext.ErrorPayload);
                return;
            }

            var expiryDate = DateTime.SpecifyKind(
                DateTime.FromBinary(_authorizationServerProvider.GetExpirationDate()),
                DateTimeKind.Utc);

            await context
                .SendDataAsync(new BearerToken
                {
                    Token = validationContext.GetToken(SecretKey, expiryDate),
                    TokenType = "bearer",
                    ExpirationDate = _authorizationServerProvider.GetExpirationDate(),
                    Username = validationContext.IdentityName,
                })
                .ConfigureAwait(false);
        }
    }
}
