using StackExchange.Redis;
using System.Security.Cryptography;
using System.Text;
using PasswordManager.Application.Interfaces;

namespace PasswordManager.Infrastructure.Redis
{
    public sealed class RefreshTokenStore(IConnectionMultiplexer redis) : IRefreshTokenStore
    {
        private readonly IDatabase _db = redis.GetDatabase();

        // Nunca guardamos el token en claro, solo su hash
        private static string HashToken(string token)
        {
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
            return Convert.ToHexString(bytes).ToLower();
        }

        public Task SaveAsync(string token, Guid userId, TimeSpan ttl, CancellationToken ct = default)
        {
            var key = $"refresh:{HashToken(token)}";
            return _db.StringSetAsync(key, userId.ToString(), ttl);
        }

        public async Task<Guid?> GetUserIdAsync(string token, CancellationToken ct = default)
        {
            var key = $"refresh:{HashToken(token)}";
            var value = await _db.StringGetAsync(key);

            if (!value.HasValue) return null;

            var text = value.ToString();
            return Guid.TryParse(text, out var userId) ? userId : null;
        }

        public Task RevokeAsync(string token, CancellationToken ct = default)
        {
            var key = $"refresh:{HashToken(token)}";
            return _db.KeyDeleteAsync(key);
        }
    }
}
