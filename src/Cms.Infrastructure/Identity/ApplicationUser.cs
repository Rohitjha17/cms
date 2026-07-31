using Microsoft.AspNetCore.Identity;

namespace Cms.Infrastructure.Identity;

public class ApplicationUser : IdentityUser
{
    public Guid? TenantId { get; set; }
    public string? FullName { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
}

public class ApplicationRole : IdentityRole
{
    public string? Description { get; set; }
}
