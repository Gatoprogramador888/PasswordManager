using System;
using System.Collections.Generic;
using System.Text;

namespace PasswordManager.Domain.Entities
{
    public class VaultEntry
    {
        public Guid Id { get; private set; }
        public Guid UserId { get; private set; }
        public string Name { get; private set; }
        public string EncryptedBlob { get; private set; }
        public string Iv { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime UpdatedAt { get; private set; }

        private VaultEntry() { }

        public static VaultEntry Create(Guid userId, string name, string encryptedBlob, string iv)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(name);
            ArgumentException.ThrowIfNullOrWhiteSpace(encryptedBlob);
            ArgumentException.ThrowIfNullOrWhiteSpace(iv);

            if (userId == Guid.Empty)
                throw new ArgumentException("UserId no puede ser vacío.", nameof(userId));

            return new VaultEntry
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Name = name,
                EncryptedBlob = encryptedBlob,
                Iv = iv,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
        }

        public void Update(string name, string encryptedBlob, string iv)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(name);
            ArgumentException.ThrowIfNullOrWhiteSpace(encryptedBlob);
            ArgumentException.ThrowIfNullOrWhiteSpace(iv);

            Name = name;
            EncryptedBlob = encryptedBlob;
            Iv = iv;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
