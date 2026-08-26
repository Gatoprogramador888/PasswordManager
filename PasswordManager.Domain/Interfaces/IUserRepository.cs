using PasswordManager.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace PasswordManager.Domain.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> GetByGoogleIdAsync(string googleId, CancellationToken ct = default);
        Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task AddAsync(User user, CancellationToken ct = default);
    }
}
