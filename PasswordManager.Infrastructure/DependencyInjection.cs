using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PasswordManager.Application.Auth;
using PasswordManager.Application.Interfaces;
using PasswordManager.Application.Settings;
using PasswordManager.Application.Vault;
using PasswordManager.Domain.Interfaces;
using PasswordManager.Infrastructure.Persistence;
using PasswordManager.Infrastructure.Persistence.Repositories;
using PasswordManager.Infrastructure.Redis;
using StackExchange.Redis;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace PasswordManager.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(
            this IServiceCollection services,
            IConfiguration config)
        {
            // MySQL
            var connString = config.GetConnectionString("MySQL")!;
            //services.AddDbContext<AppDbContext>(options =>
            //options.UseMySql(connString, new MariaDbServerVersion(new Version(10, 4, 32))));
            services.AddDbContext<AppDbContext>(options =>
                options.UseMySql(connString, ServerVersion.AutoDetect(connString)));


            // Redis
            services.AddSingleton<IConnectionMultiplexer>(
                ConnectionMultiplexer.Connect(config.GetConnectionString("Redis")!));

            // Repositories
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IVaultRepository, VaultRepository>();

            // Redis store
            services.AddScoped<IRefreshTokenStore, RefreshTokenStore>();

            // Auth services
            services.AddScoped<IGoogleTokenValidator, GoogleTokenValidator>();
            services.AddScoped<IJwtService, JwtService>();

            // Vault use cases
            services.AddScoped<GetVaultQuery>();
            services.AddScoped<AddEntryCommand>();
            services.AddScoped<UpdateEntryCommand>();
            services.AddScoped<DeleteEntryCommand>();

            // Idempotency store
            services.AddScoped<IIdempotencyStore, IdempotencyStore>();

            // Settings
            services.Configure<GoogleSettings>(config.GetSection("Google"));
            services.Configure<JwtSettings>(config.GetSection("Jwt"));

            return services;
        }
    }
}
