namespace EduFlow.Application.Options;

public sealed class ClientAppOptions
{
    public const string SectionName = "ClientApp";

    public required string BaseUrl { get; set; }
}
