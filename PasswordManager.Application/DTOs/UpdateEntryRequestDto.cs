using System;
using System.Collections.Generic;
using System.Text;

namespace PasswordManager.Application.DTOs
{
    public record UpdateEntryRequestDto(string Name, string EncryptedBlob, string Iv);
}
