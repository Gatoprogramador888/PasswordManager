using PasswordManager.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace PasswordManager.Application.Interfaces
{
    public interface IGoogleTokenValidator
    {
        Task<GoogleUserInfo?> ValidateAsync(string idToken, CancellationToken ct = default);
    }
}
