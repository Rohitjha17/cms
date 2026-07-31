using System.Security.Claims;
using Cms.Domain.Constants;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace Cms.Infrastructure.Identity;

public sealed class TenantUserClaimsPrincipalFactory
    : UserClaimsPrincipalFactory<ApplicationUser, ApplicationRole>
{
    public TenantUserClaimsPrincipalFactory(
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager,
        IOptions<IdentityOptions> options)
        : base(userManager, roleManager, options)
    {
    }

    protected override async Task<ClaimsIdentity> GenerateClaimsAsync(ApplicationUser user)
    {
        var identity = await base.GenerateClaimsAsync(user);
        if (user.TenantId.HasValue)
        {
            identity.AddClaim(new Claim(AppClaimTypes.TenantId, user.TenantId.Value.ToString()));
        }

        return identity;
    }
}
