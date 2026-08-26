using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PasswordManager.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace PasswordManager.Infrastructure.Persistence.Configurations
{
    public sealed class VaultEntryConfiguration : IEntityTypeConfiguration<VaultEntry>
    {
        public void Configure(EntityTypeBuilder<VaultEntry> builder)
        {
            builder.ToTable("vault_entries");

            builder.HasKey(v => v.Id);

            builder.Property(v => v.Id)
                   .HasColumnName("id")
                   .HasColumnType("char(36)");

            builder.Property(v => v.UserId)
                   .HasColumnName("user_id")
                   .HasColumnType("char(36)")
                   .IsRequired();

            builder.Property(v => v.Name)
                   .HasColumnName("name")
                   .HasMaxLength(100)
                   .IsRequired();

            builder.Property(v => v.EncryptedBlob)
                   .HasColumnName("encrypted_blob")
                   .IsRequired();

            builder.Property(v => v.Iv)
                   .HasColumnName("iv")
                   .HasMaxLength(64)
                   .IsRequired();

            builder.Property(v => v.CreatedAt)
                   .HasColumnName("created_at");

            builder.Property(v => v.UpdatedAt)
                   .HasColumnName("updated_at");

            // Relación con User
            builder.HasOne<User>()
                   .WithMany()
                   .HasForeignKey(v => v.UserId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
