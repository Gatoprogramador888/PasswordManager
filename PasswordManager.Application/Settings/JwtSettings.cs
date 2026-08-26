using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace PasswordManager.Application.Settings
{
    public sealed class JwtSettings
    {
        [JsonPropertyName("Secret")]
        public string Secret { get; init; } = string.Empty;
        [JsonPropertyName("Issuer")]
        public string Issuer { get; init; } = string.Empty;
        [JsonPropertyName("Audience")]
        public string Audience { get; init; } = string.Empty;
    }
}
