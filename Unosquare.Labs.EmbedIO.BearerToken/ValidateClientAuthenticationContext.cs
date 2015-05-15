namespace Unosquare.Labs.EmbedIO.BearerToken
{
    using System.Net;

    /// <summary>
    /// Context to share data with AuthorizationServerProvider
    /// </summary>
    public class ValidateClientAuthenticationContext
    {
        public string ClientId { get; protected set; }
        public bool HasError { get; protected set; }
        public bool IsValidated { get; protected set; }
        public HttpListenerContext HttpContext { get; protected set; }
        public StandardClaims StandardClaims { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="httpContext">The HttpListenerContext instance</param>
        public ValidateClientAuthenticationContext(HttpListenerContext httpContext)
        {
            HttpContext = httpContext;
            StandardClaims = new StandardClaims();
        }

        /// <summary>
        /// Rejects a validation
        /// </summary>
        public void Rejected()
        {
            IsValidated = false;
            HasError = true;
        }

        /// <summary>
        /// Validates credentials with clientId
        /// </summary>
        /// <param name="clientId"></param>
        public void Validated(string clientId)
        {
            ClientId = clientId;
            StandardClaims.Sub = clientId;

            Validated();
        }

        /// <summary>
        /// Validates credentials
        /// </summary>
        public void Validated()
        {
            IsValidated = true;
            HasError = false;
        }

        /// <summary>
        /// Retrieve JsonWebToken
        /// </summary>
        /// <param name="secretKey"></param>
        /// <returns></returns>
        public string GetToken(string secretKey)
        {
            return JWT.JsonWebToken.Encode(StandardClaims, secretKey, JWT.JwtHashAlgorithm.HS256);
        }
    }
}