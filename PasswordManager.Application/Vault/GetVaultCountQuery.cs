using PasswordManager.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace PasswordManager.Application.Vault
{
    public sealed class GetVaultCountQuery(IVaultRepository repo)
    {
        public async Task<int> HandleAsync(Guid userId, CancellationToken ct = default)
        {
            return await repo.GetCountAsync(userId, ct);
        }
    }
}
