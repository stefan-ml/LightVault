using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Text.Encodings.Web;

namespace LightVault.WebApplication.Services
{
    public class EmptyAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        public EmptyAuthenticationHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock)
            : base(options, logger, encoder, clock)
        {
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            // Always return no server authentication.
            return Task.FromResult(AuthenticateResult.NoResult());
        }
    }

}
