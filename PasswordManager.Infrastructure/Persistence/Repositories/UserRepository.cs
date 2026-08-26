using Microsoft.EntityFrameworkCore;
using PasswordManager.Domain.Entities;
using PasswordManager.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace PasswordManager.Infrastructure.Persistence.Repositories
{
    public sealed class UserRepository(AppDbContext db) : IUserRepository
    {
        public Task<User?> GetByGoogleIdAsync(string googleId, CancellationToken ct = default)
            => db.Users.FirstOrDefaultAsync(u => u.GoogleId == googleId, ct);

        public Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default)
            => db.Users.FirstOrDefaultAsync(u => u.Id == id, ct);

        public async Task AddAsync(User user, CancellationToken ct = default)
        {
            await db.Users.AddAsync(user, ct);
            await db.SaveChangesAsync(ct);
        }
    }
}
