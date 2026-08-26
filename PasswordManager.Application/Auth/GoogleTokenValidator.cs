using Google.Apis.Auth;
using Microsoft.Extensions.Options;
using PasswordManager.Application.DTOs;
using PasswordManager.Application.Interfaces;
using PasswordManager.Application.Settings;
using System;
using System.Collections.Generic;
using System.Text;

namespace PasswordManager.Application.Auth
{
    //Clase que no se puede heredar o sobre escribir
    public sealed class GoogleTokenValidator(IOptions<GoogleSettings> options) : IGoogleTokenValidator
    {
        private readonly GoogleSettings _settings = options.Value;

        public async Task<GoogleUserInfo?> ValidateAsync(string idToken, CancellationToken ct = default)
        {
            try
            {
                var validation = new GoogleJsonWebSignature.ValidationSettings
                {
                    Audience = [_settings.ClientId]
                };

                Console.WriteLine($"[DEBUG] ClientId configurado: '{_settings.ClientId}'");
                Console.WriteLine($"[DEBUG] IdToken recibido (primeros 20 chars): '{idToken?[..20]}'");

                var payload = await GoogleJsonWebSignature.ValidateAsync(idToken, validation);

                return new GoogleUserInfo(payload.Subject, payload.Email);
            }
            catch
            {
                return null;
            }
        }
    }

}
