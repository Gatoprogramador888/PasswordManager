# Password Manager API

API REST de gestión de contraseñas con arquitectura **Zero-Knowledge** desarrollada en **.NET 10** con **C#**. El servidor nunca puede leer las contraseñas del usuario — todo el cifrado ocurre en el cliente.

Desplegada en **Oracle Cloud Free Tier** con HTTPS, protección DDoS mediante Cloudflare y Redis para gestión de sesiones efímeras.

---

## ¿Por qué Zero-Knowledge?

En un gestor de contraseñas convencional, el servidor descifra y conoce las credenciales del usuario. Si la base de datos se compromete, todos los datos quedan expuestos.

Con Zero-Knowledge el servidor almacena únicamente **blobs cifrados** que son inútiles sin la master password del cliente, la cual **nunca sale del navegador**:

```
Cliente                              Servidor
──────────────────────               ──────────────────────
masterPassword + email               Recibe blob cifrado
       ↓                                      ↓
PBKDF2 = EncryptionKey   Guarda sin leer nada
       ↓
AES-256-GCM cifra el vault
       ↓
Envía blob cifrado → → → → → → → → →
```

El costo consciente de este modelo: **si el usuario olvida su master password, los datos se pierden para siempre**. No existe recuperación posible porque el servidor nunca tuvo la llave. Es el mismo tradeoff que implementan Bitwarden y 1Password.

---

## Arquitectura

Clean Architecture con 4 capas. La dependencia fluye siempre hacia adentro:

```
PasswordManager.API
├── Controllers
│   ├── AuthController      → login, refresh, logout
│   ├── VaultController     → CRUD del vault cifrado
│   └── HealthController    → GET /api/health
├── Middleware
│   ├── ExceptionHandlerMiddleware  → respuestas de error consistentes
│   └── TarpitMiddleware            → delay exponencial en intentos fallidos
└── Program.cs

PasswordManager.Application
├── Auth
│   ├── GoogleTokenValidator  → valida ID Token con Google
│   └── JwtService            → genera Access Token y Refresh Token
├── Vault
│   ├── GetVaultQuery
│   ├── AddEntryCommand
│   ├── UpdateEntryCommand
│   └── DeleteEntryCommand
├── Interfaces
│   ├── IRefreshTokenStore    → abstracción de Redis para tokens
│   └── IIdempotencyStore     → abstracción de Redis para idempotencia
└── Settings
    ├── GoogleSettings
    └── JwtSettings

PasswordManager.Domain
├── Entities
│   ├── User          → google_id, email
│   └── VaultEntry    → name (en claro), encrypted_blob, iv
└── Interfaces
    ├── IUserRepository
    └── IVaultRepository

PasswordManager.Infrastructure
├── Persistence
│   ├── AppDbContext
│   ├── Configurations    → Fluent API para MySQL
│   └── Repositories      → UserRepository, VaultRepository
├── Redis
│   ├── RefreshTokenStore   → TTL 1 hora
│   └── IdempotencyStore    → TTL 24 horas
└── DependencyInjection.cs
```

---

## Decisiones técnicas

### Autenticación híbrida

```
Paso 1 → Sign in with Google → el servidor sabe quién eres (JWT propio)
Paso 2 → Master Password     → nunca sale del cliente, deriva la EncryptionKey
```

Google autentica la identidad. La master password protege el vault. Son responsabilidades distintas e independientes.

### Refresh Tokens en Redis, no en MySQL

Los refresh tokens son datos efímeros con ciclo de vida corto. Redis los expira automáticamente con TTL sin necesidad de jobs de limpieza. MySQL es para datos persistentes; Redis para datos con tiempo de vida natural.

```
Login exitoso → SET refresh:{sha256(token)} {userId} EX 3600
Logout        → DEL refresh:{sha256(token)}
Expiración    → Redis lo elimina solo al cumplir 1 hora
```

El token nunca se guarda en claro — solo su hash SHA-256.

### Rotación de Refresh Tokens

Cada vez que se usa un refresh token para obtener un nuevo access token, el refresh token usado se revoca y se emite uno nuevo. Si alguien roba un refresh token y lo usa, el dueño legítimo nota que su sesión fue cerrada.

### Tarpit Middleware

En lugar de bloquear al atacante con un rate limiter simple, el Tarpit introduce un delay exponencial **asíncrono** en respuestas 401/403 repetidas. El hilo se libera con `Task.Delay` — el servidor sigue respondiendo a usuarios legítimos normalmente mientras el atacante espera.

```
Intentos 1-2  →     0ms
Intentos 3-4  →  3,000ms
Intentos 5-6  → 10,000ms
Intentos 7-9  → 30,000ms
Intentos 10+  → 60,000ms
```

El contador se reinicia en login exitoso para no penalizar a usuarios legítimos que se equivocaron una vez.

### Idempotencia en escrituras

El cliente genera un UUID por operación de formulario. Si el usuario hace clic múltiples veces (red lenta, doble clic), el servidor detecta el key duplicado en Redis y descarta el request sin insertar de nuevo.

```
UUID generado al abrir el form
    → Clic × 3 → solo el primero se procesa
    → Redis guarda el key por 24 horas
    → Nuevo UUID solo después de operación exitosa
```

### ExceptionHandler centralizado

Ningún stack trace llega al cliente en producción. El middleware atrapa todas las excepciones, las loguea en el servidor con el detalle completo, y regresa al cliente únicamente un código y mensaje genérico.

```json
{ "code": "INTERNAL_ERROR", "message": "Ocurrió un error interno." }
```

---

## Stack

| Capa | Tecnología | Razón |
|---|---|---|
| Runtime | .NET 10 / SDK 9 | SDK 9 por compatibilidad con Pomelo EF Core |
| API | ASP.NET Core Controllers | Estándar enterprise reconocible en GDL |
| ORM | EF Core 9 + Pomelo | Pomelo no tiene soporte estable para EF Core 10 aún |
| Base de datos | MySQL en Aiven | Externo gestionado, sin carga en la VPS |
| Caché / Sesiones | Redis | Datos efímeros con TTL natural |
| Auth | Google OAuth 2.0 + JWT propio | Sin almacenar passwords, sin dependencia continua de Google |
| Deploy | Docker + Nginx | Imagen mínima con multistage build |
| DNS / DDoS | Cloudflare (capa gratuita) | Protección sin costo en Free Tier |
| VPS | Oracle Cloud Free Tier | 6GB RAM, ARM, sin costo |

---

## Infraestructura en producción

```
Internet
    ↓ HTTPS (443)
 Cloudflare       ← DDoS protection, SSL termination
    ↓# Password Manager API

API REST de gestión de contraseñas con arquitectura **Zero-Knowledge** desarrollada en **.NET 10** con **C#**. El servidor nunca puede leer las contraseñas del usuario — todo el cifrado ocurre en el cliente.

Desplegada en **Oracle Cloud Free Tier** con HTTPS, protección DDoS mediante Cloudflare y Redis para gestión de sesiones efímeras.

---

## ¿Por qué Zero-Knowledge?

En un gestor de contraseñas convencional, el servidor descifra y conoce las credenciales del usuario. Si la base de datos se compromete, todos los datos quedan expuestos.

Con Zero-Knowledge el servidor almacena únicamente **blobs cifrados** que son inútiles sin la master password del cliente, la cual **nunca sale del navegador**:

```
Cliente                              Servidor
──────────────────────               ──────────────────────
masterPassword + email               Recibe blob cifrado
       ↓                                      ↓
PBKDF2 = EncryptionKey   Guarda sin leer nada
       ↓
AES-256-GCM cifra el vault
       ↓
Envía blob cifrado → → → → → → → → →
```

El costo consciente de este modelo: **si el usuario olvida su master password, los datos se pierden para siempre**. No existe recuperación posible porque el servidor nunca tuvo la llave. Es el mismo tradeoff que implementan Bitwarden y 1Password.

---

## Arquitectura

Clean Architecture con 4 capas. La dependencia fluye siempre hacia adentro:

```
PasswordManager.API
├── Controllers
│   ├── AuthController      → login, refresh, logout
│   ├── VaultController     → CRUD del vault cifrado
│   └── HealthController    → GET /api/health
├── Middleware
│   ├── ExceptionHandlerMiddleware  → respuestas de error consistentes
│   └── TarpitMiddleware            → delay exponencial en intentos fallidos
└── Program.cs

PasswordManager.Application
├── Auth
│   ├── GoogleTokenValidator  → valida ID Token con Google
│   └── JwtService            → genera Access Token y Refresh Token
├── Vault
│   ├── GetVaultQuery
│   ├── AddEntryCommand
│   ├── UpdateEntryCommand
│   └── DeleteEntryCommand
├── Interfaces
│   ├── IRefreshTokenStore    → abstracción de Redis para tokens
│   └── IIdempotencyStore     → abstracción de Redis para idempotencia
└── Settings
    ├── GoogleSettings
    └── JwtSettings

PasswordManager.Domain
├── Entities
│   ├── User          → google_id, email
│   └── VaultEntry    → name (en claro), encrypted_blob, iv
└── Interfaces
    ├── IUserRepository
    └── IVaultRepository

PasswordManager.Infrastructure
├── Persistence
│   ├── AppDbContext
│   ├── Configurations    → Fluent API para MySQL
│   └── Repositories      → UserRepository, VaultRepository
├── Redis
│   ├── RefreshTokenStore   → TTL 1 hora
│   └── IdempotencyStore    → TTL 24 horas
└── DependencyInjection.cs
```

---

## Decisiones técnicas

### Autenticación híbrida

```
Paso 1 → Sign in with Google → el servidor sabe quién eres (JWT propio)
Paso 2 → Master Password     → nunca sale del cliente, deriva la EncryptionKey
```

Google autentica la identidad. La master password protege el vault. Son responsabilidades distintas e independientes.

### Refresh Tokens en Redis, no en MySQL

Los refresh tokens son datos efímeros con ciclo de vida corto. Redis los expira automáticamente con TTL sin necesidad de jobs de limpieza. MySQL es para datos persistentes; Redis para datos con tiempo de vida natural.

```
Login exitoso → SET refresh:{sha256(token)} {userId} EX 3600
Logout        → DEL refresh:{sha256(token)}
Expiración    → Redis lo elimina solo al cumplir 1 hora
```

El token nunca se guarda en claro — solo su hash SHA-256.

### Rotación de Refresh Tokens

Cada vez que se usa un refresh token para obtener un nuevo access token, el refresh token usado se revoca y se emite uno nuevo. Si alguien roba un refresh token y lo usa, el dueño legítimo nota que su sesión fue cerrada.

### Tarpit Middleware

En lugar de bloquear al atacante con un rate limiter simple, el Tarpit introduce un delay exponencial **asíncrono** en respuestas 401/403 repetidas. El hilo se libera con `Task.Delay` — el servidor sigue respondiendo a usuarios legítimos normalmente mientras el atacante espera.

```
Intentos 1-2  →     0ms
Intentos 3-4  →  3,000ms
Intentos 5-6  → 10,000ms
Intentos 7-9  → 30,000ms
Intentos 10+  → 60,000ms
```

El contador se reinicia en login exitoso para no penalizar a usuarios legítimos que se equivocaron una vez.

### Idempotencia en escrituras

El cliente genera un UUID por operación de formulario. Si el usuario hace clic múltiples veces (red lenta, doble clic), el servidor detecta el key duplicado en Redis y descarta el request sin insertar de nuevo.

```
UUID generado al abrir el form
    → Clic × 3 → solo el primero se procesa
    → Redis guarda el key por 24 horas
    → Nuevo UUID solo después de operación exitosa
```

### ExceptionHandler centralizado

Ningún stack trace llega al cliente en producción. El middleware atrapa todas las excepciones, las loguea en el servidor con el detalle completo, y regresa al cliente únicamente un código y mensaje genérico.

```json
{ "code": "INTERNAL_ERROR", "message": "Ocurrió un error interno." }
```

---

## Stack

| Capa | Tecnología | Razón |
|---|---|---|
| Runtime | .NET 10 / SDK 9 | SDK 9 por compatibilidad con Pomelo EF Core |
| API | ASP.NET Core Controllers | Estándar enterprise reconocible en GDL |
| ORM | EF Core 9 + Pomelo | Pomelo no tiene soporte estable para EF Core 10 aún |
| Base de datos | MySQL en Aiven | Externo gestionado, sin carga en la VPS |
| Caché / Sesiones | Redis | Datos efímeros con TTL natural |
| Auth | Google OAuth 2.0 + JWT propio | Sin almacenar passwords, sin dependencia continua de Google |
| Deploy | Docker + Nginx | Imagen mínima con multistage build |
| DNS / DDoS | Cloudflare (capa gratuita) | Protección sin costo en Free Tier |
| VPS | Oracle Cloud Free Tier | 6GB RAM, ARM, sin costo |

---

## Infraestructura en producción

```
Internet
    ↓ HTTPS (443)
 Cloudflare       ← DDoS protection, SSL termination
    ↓
 Nginx            ← Reverse proxy en Oracle VPS
    ↓ HTTP interno (8080)
 ASP.NET Core     ← Docker container, restart: always
    ↓                        ↓
Redis (local)          Aiven MySQL (externo, TLS)
127.0.0.1:6379         mysql.aivencloud.com:XXXX
```

Redis escucha únicamente en `127.0.0.1` — nunca expuesto a internet. MySQL en Aiven usa TLS obligatorio.

---

## Esquema de base de datos

```sql
-- Solo dos tablas. El servidor es ciego al contenido del vault.

CREATE TABLE users (
    id          CHAR(36)     PRIMARY KEY,
    google_id   VARCHAR(100) UNIQUE NOT NULL,
    email       VARCHAR(255) UNIQUE NOT NULL,
    created_at  DATETIME     NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE vault_entries (
    id              CHAR(36)     PRIMARY KEY,
    user_id         CHAR(36)     NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    name            VARCHAR(100) NOT NULL,        -- metadata en claro
    encrypted_blob  TEXT         NOT NULL,        -- el servidor no puede leer esto
    iv              VARCHAR(64)  NOT NULL,        -- Salt(16b) + IV(12b) en Base64
    created_at      DATETIME     NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at      DATETIME     NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
);

-- Refresh tokens → Redis (no existen en MySQL)
```

---

## Endpoints

```
POST   /api/auth/login      → recibe Google ID Token, regresa JWT propio
POST   /api/auth/refresh    → rota el refresh token, regresa nuevo access token
POST   /api/auth/logout     → revoca el refresh token en Redis

GET    /api/vault           → lista entradas (blobs cifrados, sin descifrar)
POST   /api/vault           → agrega entrada cifrada
PUT    /api/vault/{id}      → modifica entrada cifrada
DELETE /api/vault/{id}      → elimina entrada

GET    /api/health          → estado de la API
```

Todos los endpoints de vault requieren `Authorization: Bearer {accessToken}`.
Add y Update aceptan el header `Idempotency-Key` para prevenir duplicados.

---

## Correr en local

**Requisitos:**
- .NET SDK 9
- MySQL (XAMPP o cualquier instancia local)
- Redis

**1 — Clona el repo**
```bash
git clone https://github.com/Gatoprogramador888/PasswordManager.git
cd PasswordManager
```

**2 — Crea el archivo de configuración local**

Crea `PasswordManager.API/appsettings.Development.json` (no se sube al repo):

```json
{
  "ConnectionStrings": {
    "MySQL": "Server=localhost;Port=3306;Database=password_manager;Uid=root;Pwd=;SslMode=None;",
    "Redis": "127.0.0.1:6379"
  },
  "Google": {
    "ClientId": "TU_GOOGLE_CLIENT_ID"
  },
  "Jwt": {
    "Secret":   "minimo-32-caracteres-cambia-esto",
    "Issuer":   "password-manager-api",
    "Audience": "password-manager-client"
  }
}
```

**3 — Crea la base de datos**
```sql
CREATE DATABASE password_manager CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
```

Ejecuta el SQL del archivo `schema.sql` en la raíz del repo.

**4 — Corre la API**
```bash
dotnet run --project PasswordManager.API
```

API disponible en `https://localhost:7188` — documentación en `/scalar/v1`.

---

## Deploy con Docker

```bash
# Crea el archivo .env en la VPS (nunca en el repo)
cp .env.example .env
nano .env  # llena los valores reales

# Levanta API + Redis
docker-compose up -d --build
```

---

## Deuda técnica conocida

- Sin tests automatizados — pendiente xUnit + Moq en capa Application
- Sin paginación en `GET /vault` — escala mal con muchas entradas
- Sin logging estructurado — pendiente Serilog con sink a archivo

Estos puntos son mejoras conocidas, no descuidos. La prioridad fue construir la base correcta primero.
 ASP.NET Core     ← Docker container, restart: always
    ↓                        ↓
Redis (local)          Aiven MySQL (externo, TLS)
127.0.0.1:6379         mysql.aivencloud.com:XXXX
```

Redis escucha únicamente en `127.0.0.1` — nunca expuesto a internet. MySQL en Aiven usa TLS obligatorio.

---

## Esquema de base de datos

```sql
-- Solo dos tablas. El servidor es ciego al contenido del vault.

CREATE TABLE users (
    id          CHAR(36)     PRIMARY KEY,
    google_id   VARCHAR(100) UNIQUE NOT NULL,
    email       VARCHAR(255) UNIQUE NOT NULL,
    created_at  DATETIME     NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE vault_entries (
    id              CHAR(36)     PRIMARY KEY,
    user_id         CHAR(36)     NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    name            VARCHAR(100) NOT NULL,        -- metadata en claro
    encrypted_blob  TEXT         NOT NULL,        -- el servidor no puede leer esto
    iv              VARCHAR(64)  NOT NULL,        -- Salt(16b) + IV(12b) en Base64
    created_at      DATETIME     NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at      DATETIME     NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
);

-- Refresh tokens → Redis (no existen en MySQL)
```

---

## Endpoints

```
POST   /api/auth/login      → recibe Google ID Token, regresa JWT propio
POST   /api/auth/refresh    → rota el refresh token, regresa nuevo access token
POST   /api/auth/logout     → revoca el refresh token en Redis

GET    /api/vault           → lista entradas (blobs cifrados, sin descifrar)
POST   /api/vault           → agrega entrada cifrada
PUT    /api/vault/{id}      → modifica entrada cifrada
DELETE /api/vault/{id}      → elimina entrada

GET    /api/health          → estado de la API
```

Todos los endpoints de vault requieren `Authorization: Bearer {accessToken}`.
Add y Update aceptan el header `Idempotency-Key` para prevenir duplicados.

---

## Correr en local

**Requisitos:**
- .NET SDK 9
- MySQL (XAMPP o cualquier instancia local)
- Redis

**1 — Clona el repo**
```bash
git clone https://github.com/Gatoprogramador888/PasswordManager.git
cd PasswordManager
```

**2 — Crea el archivo de configuración local**

Crea `PasswordManager.API/appsettings.Development.json` (no se sube al repo):

```json
{
  "ConnectionStrings": {
    "MySQL": "Server=localhost;Port=3306;Database=password_manager;Uid=root;Pwd=;SslMode=None;",
    "Redis": "127.0.0.1:6379"
  },
  "Google": {
    "ClientId": "TU_GOOGLE_CLIENT_ID"
  },
  "Jwt": {
    "Secret":   "minimo-32-caracteres-cambia-esto",
    "Issuer":   "password-manager-api",
    "Audience": "password-manager-client"
  }
}
```

**3 — Crea la base de datos**
```sql
CREATE DATABASE password_manager CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
```

Ejecuta el SQL del archivo `schema.sql` en la raíz del repo.

**4 — Corre la API**
```bash
dotnet run --project PasswordManager.API
```

API disponible en `https://localhost:7188` — documentación en `/scalar/v1`.

---

## Deploy con Docker

```bash
# Crea el archivo .env en la VPS (nunca en el repo)
cp .env.example .env
nano .env  # llena los valores reales

# Levanta API + Redis
docker-compose up -d --build
```

---

## Deuda técnica conocida

- Sin tests automatizados — pendiente xUnit + Moq en capa Application
- Sin paginación en `GET /vault` — escala mal con muchas entradas
- Sin logging estructurado — pendiente Serilog con sink a archivo

Estos puntos son mejoras conocidas, no descuidos. La prioridad fue construir la base correcta primero.