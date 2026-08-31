using System;
using System.Collections.Generic;
using System.Text;

namespace PasswordManager.Application.DTOs
{
    public record CreateVaultEntryResponse(Guid Id, int TotalCount);
}
