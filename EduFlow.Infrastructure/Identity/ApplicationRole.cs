using Microsoft.AspNetCore.Identity;

namespace EduFlow.Infrastructure.Identity;

public sealed class ApplicationRole : IdentityRole<Guid>
{
    public ApplicationRole() { }

    public ApplicationRole(string roleName) : base(roleName) { }
}
