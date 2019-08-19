using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.Features.Authentication;

namespace EmbedIO.AspNetCore.Wrappers
{
    class AuthenticationHandler : IAuthenticationHandler
    {

        public Task AuthenticateAsync(AuthenticateContext context)
        {
            throw new NotImplementedException();
        }

        public Task ChallengeAsync(ChallengeContext context)
        {
            throw new NotImplementedException();
        }

        public void GetDescriptions(DescribeSchemesContext context)
        {
            throw new NotImplementedException();
        }

        public Task SignInAsync(SignInContext context)
        {
            throw new NotImplementedException();
        }

        public Task SignOutAsync(SignOutContext context)
        {
            throw new NotImplementedException();
        }
    }
}
