using System.Collections.Generic;
using System.Net;

namespace Unosquare.Labs.EmbedIO.BearerToken
{
    public class BearerTokenModule : WebModuleBase
    {
        public delegate bool ValidateHandler(WebServer server, HttpListenerContext context, out Dictionary<string, object> payload);

        public BearerTokenModule(ValidateHandler validation, string secretKey = "0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJjbGF")
        {
            AddHandler("/token", HttpVerbs.Get, (server, context) =>
            {
                Dictionary<string, object> payload;

                if (validation(server, context, out payload))
                {
                    if (payload == null) payload = new Dictionary<string, object>();

                    payload.Add("EmbedIOAuthKey", 1);

                    context.JsonResponse(new BearerToken
                    {
                        Token = JWT.JsonWebToken.Encode(payload, secretKey, JWT.JwtHashAlgorithm.HS256)
                    });
                }
                else
                {
                    context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                }

                return true;
            });
        }

        public override string Name
        {
            get { return "Bearer Token Module"; }
        }
    }
}
