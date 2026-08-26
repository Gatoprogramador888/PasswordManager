using System;
using System.Collections.Generic;
using System.Text;

namespace PasswordManager.Application.Interfaces
{
    public interface IRefreshTokenStore
    {
        Task SaveAsync(string token, Guid userId, TimeSpan ttl, CancellationToken ct = default);
        Task<Guid?> GetUserIdAsync(string token, CancellationToken ct = default);
        Task RevokeAsync(string token, CancellationToken ct = default);
    }
}
