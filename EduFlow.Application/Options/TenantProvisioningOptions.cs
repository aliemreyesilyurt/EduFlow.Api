namespace EduFlow.Application.Options;

public sealed class TenantProvisioningOptions
{
    public const string SectionName = "TenantProvisioning";

    public required string DefaultAdminPassword { get; set; }
}
