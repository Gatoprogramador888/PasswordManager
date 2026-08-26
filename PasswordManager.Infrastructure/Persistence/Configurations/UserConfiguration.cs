using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PasswordManager.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace PasswordManager.Infrastructure.Persistence.Configurations
{
    public sealed class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.ToTable("users");

            builder.HasKey(u => u.Id);

            builder.Property(u => u.Id)
                   .HasColumnName("id")
                   .HasColumnType("char(36)");

            builder.Property(u => u.GoogleId)
                   .HasColumnName("google_id")
                   .HasMaxLength(100)
                   .IsRequired();

            builder.Property(u => u.Email)
                   .HasColumnName("email")
                   .HasMaxLength(255)
                   .IsRequired();

            builder.Property(u => u.CreatedAt)
                   .HasColumnName("created_at");

            builder.HasIndex(u => u.GoogleId).IsUnique();
            builder.HasIndex(u => u.Email).IsUnique();
        }
    }
}
