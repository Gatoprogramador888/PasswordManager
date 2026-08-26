using PasswordManager.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace PasswordManager.Application.Vault
{
    public sealed class DeleteEntryCommand(IVaultRepository repo)
    {
        public async Task<bool> HandleAsync(Guid entryId, Guid userId, CancellationToken ct = default)
        {
            var entry = await repo.GetByIdAsync(entryId, userId, ct);

            if (entry is null)
                return false;

            await repo.DeleteAsync(entryId, userId, ct);

            return true;
        }
    }
}
