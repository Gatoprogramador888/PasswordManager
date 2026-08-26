using System;
using System.Collections.Generic;
using System.Text;

namespace PasswordManager.Domain.Entities
{
    public class User
    {
        public Guid Id { get; private set; }
        public string GoogleId { get; private set; }
        public string Email { get; private set; }
        public DateTime CreatedAt { get; private set; }

        // EF Core necesita un constructor privado sin parámetros
        private User() { }

        public static User Create(string googleId, string email)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(googleId);
            ArgumentException.ThrowIfNullOrWhiteSpace(email);

            return new User
            {
                Id = Guid.NewGuid(),
                GoogleId = googleId,
                Email = email,
                CreatedAt = DateTime.UtcNow
            };
        }
    }
}
