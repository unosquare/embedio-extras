namespace Unosquare.Labs.EmbedIO.BearerToken
{
    using JWT;
    using System;
    using Net;

    /// <summary>
    /// Context to share data with AuthorizationServerProvider
    /// </summary>
    public class ValidateClientAuthenticationContext
    {
        /// <summary>
        /// The Client Id
        /// </summary>
        public string ClientId { get; protected set; }

        /// <summary>
        /// Flags if the Validation has errors
        /// </summary>
        public bool HasError { get; protected set; }

        /// <summary>
        /// Indicates if the Validation is right
        /// </summary>
        public bool IsValidated { get; protected set; }

        /// <summary>
        /// Http Context instance
        /// </summary>
        public HttpListenerContext HttpContext { get; protected set; }

        /// <summary>
        /// Claims
        /// </summary>
        public StandardClaims StandardClaims { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="httpContext">The HttpListenerContext instance</param>
        public ValidateClientAuthenticationContext(HttpListenerContext httpContext)
        {
            if (httpContext == null)
                throw new ArgumentNullException("Context is null", "httpContext");

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
        public string GetToken(string secretKey) => JsonWebToken.Encode(StandardClaims, secretKey, JwtHashAlgorithm.HS256);
    }
}