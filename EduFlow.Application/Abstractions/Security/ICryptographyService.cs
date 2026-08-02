namespace EduFlow.Application.Abstractions.Security;

/// <summary>
/// Reversible encryption for PII that must be readable later (e.g. national id, phone).
/// Passwords use ASP.NET Core Identity's one-way hashing and never go through this service.
/// </summary>
public interface ICryptographyService
{
    string Encrypt(string plainText);
    string Decrypt(string cipherText);
}
