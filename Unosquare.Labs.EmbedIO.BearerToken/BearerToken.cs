using Newtonsoft.Json;

namespace Unosquare.Labs.EmbedIO.BearerToken
{
    public class BearerToken
    {
        [JsonProperty("access_token")]
        public string Token { get; set; }
        [JsonProperty("token_type")]
        public string TokenType { get; set; }
        [JsonProperty("expires_in")]
        public long ExpirationDate { get; set; }
        [JsonProperty("userName")]
        public string Username { get; set; }
    }
}
