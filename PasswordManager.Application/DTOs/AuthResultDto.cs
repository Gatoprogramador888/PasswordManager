using System;
using System.Collections.Generic;
using System.Text;

namespace PasswordManager.Application.DTOs
{
    public record AuthResultDto(string AccessToken, string RefreshToken);
}
