# ─────────────────────────────────────
# STAGE 1 — Build
# ─────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copia los csproj primero para aprovechar el cache de capas
# Si no cambia el código pero sí las dependencias, solo reinstala paquetes
COPY PasswordManager.Domain/PasswordManager.Domain.csproj             PasswordManager.Domain/
COPY PasswordManager.Application/PasswordManager.Application.csproj   PasswordManager.Application/
COPY PasswordManager.Infrastructure/PasswordManager.Infrastructure.csproj PasswordManager.Infrastructure/
COPY PasswordManager.API/PasswordManager.API.csproj                   PasswordManager.API/

RUN dotnet restore PasswordManager.API/PasswordManager.API.csproj

# Copia el resto del código y publica
COPY . .
RUN dotnet publish PasswordManager.API/PasswordManager.API.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

# ─────────────────────────────────────
# STAGE 2 — Runtime
# ─────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# Usuario sin privilegios — nunca corras como root en producción
RUN adduser --disabled-password --gecos "" appuser
USER appuser

COPY --from=build /app/publish .

# Puerto que expone la API (HTTP, Nginx hace el HTTPS por fuera)
EXPOSE 8080

ENTRYPOINT ["dotnet", "PasswordManager.API.dll"]
