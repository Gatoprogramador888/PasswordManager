using PasswordManager.Application.DTOs;
using PasswordManager.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace PasswordManager.Application.Vault
{
    public sealed class DeleteEntryCommand(IVaultRepository repo)
    {
        public async Task<DeleteVaultEntryResponse> HandleAsync(Guid entryId, Guid userId, CancellationToken ct = default)
        {
            var entry = await repo.GetByIdAsync(entryId, userId, ct);

            if (entry is null)
                return new(false, 0);

            await repo.DeleteAsync(entryId, userId, ct);

            int count = await repo.GetCountAsync(userId, ct);
            
            return new(true, count);
        }
    }
}
