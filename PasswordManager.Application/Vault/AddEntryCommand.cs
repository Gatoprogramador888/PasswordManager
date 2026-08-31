using PasswordManager.Application.DTOs;
using PasswordManager.Domain.Entities;
using PasswordManager.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace PasswordManager.Application.Vault
{
    public sealed class AddEntryCommand(IVaultRepository repo)
    {
        public async Task<CreateVaultEntryResponse> HandleAsync(Guid userId, AddEntryRequestDto request, CancellationToken ct = default)
        {
            var entry = VaultEntry.Create(userId, request.Name, request.EncryptedBlob, request.Iv);

            await repo.AddAsync(entry, ct);

            int count = await repo.GetCountAsync(userId, ct);

            return new(entry.Id, count);
        }
    }
}