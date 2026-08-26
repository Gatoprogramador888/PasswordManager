using PasswordManager.Application.DTOs;
using PasswordManager.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace PasswordManager.Application.Vault
{
    public sealed class GetVaultQuery(IVaultRepository repo)
    {
        public async Task<IReadOnlyList<VaultEntryDto>> HandleAsync(Guid userId, CancellationToken ct = default)
        {
            var entries = await repo.GetByUserIdAsync(userId, ct);

            return entries.Select(e => new VaultEntryDto(
                e.Id,
                e.Name,
                e.EncryptedBlob,
                e.Iv,
                e.UpdatedAt
            )).ToList();
        }
    }
}
