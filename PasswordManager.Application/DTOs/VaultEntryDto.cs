using System;
using System.Collections.Generic;
using System.Text;

namespace PasswordManager.Application.DTOs
{
    public record VaultEntryDto(
    Guid Id,
    string Name,
    string EncryptedBlob,
    string Iv,
    DateTime UpdatedAt
);
}
