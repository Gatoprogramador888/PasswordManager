using System;
using System.Collections.Generic;
using System.Text;

namespace PasswordManager.Application.Interfaces
{
    public interface IJwtService
    {
        string GenerateAccessToken(Guid userId, string email);
        string GenerateRefreshToken();
    }
}
