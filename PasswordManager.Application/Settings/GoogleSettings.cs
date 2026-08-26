using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace PasswordManager.Application.Settings
{
    public sealed class GoogleSettings
    {
        [JsonPropertyName("client_id")]
        public string ClientId { get; init; } = string.Empty;
    }
}
