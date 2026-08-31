using PasswordManager.Application.DTOs;
using PasswordManager.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace PasswordManager.Application.Vault
{
    public sealed class GetVaultQuery(IVaultRepository repo)
    {
        [System.Obsolete("Se usara ahora con minimos y maximos")]
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

        public async Task<IReadOnlyList<VaultEntryDto>> HandleAsync(Guid userId, int min = 0, int max = 10, CancellationToken ct = default)
        {
            if(min < 0 || max < 0 || min >= max)
            {
                throw new ArgumentException("Invalid min or max values");
            }
            var entries = await repo.GetByUserIdAsync(userId, min, max, ct);

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
