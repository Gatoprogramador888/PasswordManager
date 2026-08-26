using System;
using System.Collections.Generic;
using System.Text;

namespace PasswordManager.Application.Interfaces
{
    public interface IIdempotencyStore
    {
        Task<bool> WasProcessedAsync(string key, CancellationToken ct = default);
        Task MarkProcessedAsync(string key, CancellationToken ct = default);
    }
}
