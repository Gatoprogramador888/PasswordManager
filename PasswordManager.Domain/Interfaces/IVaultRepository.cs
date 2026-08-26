using PasswordManager.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace PasswordManager.Domain.Interfaces
{
    public interface IVaultRepository
    {
        Task<IReadOnlyList<VaultEntry>> GetByUserIdAsync(Guid userId, CancellationToken ct = default);
        Task<VaultEntry?> GetByIdAsync(Guid id, Guid userId, CancellationToken ct = default);
        Task AddAsync(VaultEntry entry, CancellationToken ct = default);
        Task UpdateAsync(VaultEntry entry, CancellationToken ct = default);
        Task DeleteAsync(Guid id, Guid userId, CancellationToken ct = default);
    }
}
