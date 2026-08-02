using EduFlow.Application.Abstractions.Security;
using Microsoft.AspNetCore.DataProtection;

namespace EduFlow.Infrastructure.Security;

/// <summary>
/// Reversible PII encryption backed by ASP.NET Core's Data Protection API (key management,
/// rotation and purpose isolation handled by the framework instead of a hand-rolled AES routine).
/// </summary>
public sealed class DataProtectionCryptographyService : ICryptographyService
{
    private readonly IDataProtector _protector;

    public DataProtectionCryptographyService(IDataProtectionProvider provider)
    {
        _protector = provider.CreateProtector("EduFlow.Pii.v1");
    }

    public string Encrypt(string plainText) => _protector.Protect(plainText);

    public string Decrypt(string cipherText) => _protector.Unprotect(cipherText);
}
