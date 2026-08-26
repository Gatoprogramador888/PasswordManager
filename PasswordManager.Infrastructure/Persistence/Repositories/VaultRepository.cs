using Microsoft.EntityFrameworkCore;
using PasswordManager.Domain.Entities;
using PasswordManager.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace PasswordManager.Infrastructure.Persistence.Repositories
{
    public sealed class VaultRepository(AppDbContext db) : IVaultRepository
    {
        public async Task<IReadOnlyList<VaultEntry>> GetByUserIdAsync(Guid userId, CancellationToken ct = default)
            => await db.VaultEntries
                       .Where(v => v.UserId == userId)
                       .OrderByDescending(v => v.UpdatedAt)
                       .ToListAsync(ct);

        public Task<VaultEntry?> GetByIdAsync(Guid id, Guid userId, CancellationToken ct = default)
            => db.VaultEntries.FirstOrDefaultAsync(v => v.Id == id && v.UserId == userId, ct);

        public async Task AddAsync(VaultEntry entry, CancellationToken ct = default)
        {
            await db.VaultEntries.AddAsync(entry, ct);
            await db.SaveChangesAsync(ct);
        }

        public async Task UpdateAsync(VaultEntry entry, CancellationToken ct = default)
        {
            db.VaultEntries.Update(entry);
            await db.SaveChangesAsync(ct);
        }

        public async Task DeleteAsync(Guid id, Guid userId, CancellationToken ct = default)
        {
            var entry = await GetByIdAsync(id, userId, ct);

            if (entry is null) return;

            db.VaultEntries.Remove(entry);
            await db.SaveChangesAsync(ct);
        }
    }
}
