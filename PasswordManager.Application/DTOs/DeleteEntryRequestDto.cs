using System;
using System.Collections.Generic;
using System.Text;

namespace PasswordManager.Application.DTOs
{
    public record DeleteVaultEntryResponse(bool IsDeleted, int TotalCount);
}
