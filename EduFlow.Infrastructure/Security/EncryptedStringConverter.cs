using EduFlow.Application.Abstractions.Security;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace EduFlow.Infrastructure.Security;

/// <summary>EF Core value converter that transparently encrypts/decrypts a nullable string column.</summary>
public sealed class EncryptedStringConverter(ICryptographyService cryptographyService)
    : ValueConverter<string?, string?>(
        v => v == null ? null : cryptographyService.Encrypt(v),
        v => v == null ? null : cryptographyService.Decrypt(v));
