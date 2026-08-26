using PasswordManager.Application.Interfaces;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Text;

namespace PasswordManager.Infrastructure.Redis
{
    public sealed class IdempotencyStore(IConnectionMultiplexer redis) : IIdempotencyStore
    {
        private readonly IDatabase _db = redis.GetDatabase();

        public async Task<bool> WasProcessedAsync(string key, CancellationToken ct = default)
        {
            var redisKey = $"idempotency:{key}";
            return await _db.KeyExistsAsync(redisKey);
        }

        public async Task MarkProcessedAsync(string key, CancellationToken ct = default)
        {
            var redisKey = $"idempotency:{key}";
            // Vive 24 horas — suficiente para evitar duplicados sin acumular basura
            await _db.StringSetAsync(redisKey, "1", TimeSpan.FromHours(24));
        }
    }
}
