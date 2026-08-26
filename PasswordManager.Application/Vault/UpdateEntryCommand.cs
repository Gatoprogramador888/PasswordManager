using PasswordManager.Application.DTOs;
using PasswordManager.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace PasswordManager.Application.Vault
{
    public sealed class UpdateEntryCommand(IVaultRepository repo)
    {
        public async Task<bool> HandleAsync(Guid entryId, Guid userId, UpdateEntryRequestDto request, CancellationToken ct = default)
        {
            var entry = await repo.GetByIdAsync(entryId, userId, ct);

            if (entry is null)
                return false; // no existe o no le pertenece

            entry.Update(request.Name, request.EncryptedBlob, request.Iv);

            await repo.UpdateAsync(entry, ct);

            return true;
        }
    }
}
