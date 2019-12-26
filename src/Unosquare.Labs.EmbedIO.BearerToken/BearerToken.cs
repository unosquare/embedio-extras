using Swan.Formatters;

namespace EmbedIO.BearerToken
{
    /// <summary>
    /// Represents a Bearer Token JSON response.
    /// </summary>
    public class BearerToken
    {
        /// <summary>
        /// The JsonWebToken.
        /// </summary>
        [JsonProperty("access_token")]
        public string Token { get; set; }
        
        /// <summary>
        /// The Token type.
        /// </summary>
        [JsonProperty("token_type")]
        public string TokenType { get; set; }
        
        /// <summary>
        /// Expiration Date.
        /// </summary>
        [JsonProperty("expires_in")]
        public long ExpirationDate { get; set; }
        
        /// <summary>
        /// Client username.
        /// </summary>
        [JsonProperty("userName")]
        public string? Username { get; set; }
    }
}
