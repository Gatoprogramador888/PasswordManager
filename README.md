### Descripción de Capas

1. **PasswordManager.API**: Punto de entrada de la aplicación. Contiene la configuración de la tubería HTTP (*middleware*), inyección de dependencias y mapeo de endpoints mediante **Minimal APIs**. Incluye protección contra fuerza bruta (*Tarpit Middleware*).
2. **PasswordManager.Application**: Define los casos de uso (comandos y consultas), servicios de tokens/validaciones y las abstracciones del sistema.
3. **PasswordManager.Domain**: El núcleo de la aplicación. Contiene las entidades principales (`User`, `VaultEntry`, `RefreshToken`) e interfaces de repositorio sin dependencias externas.
4. **PasswordManager.Infrastructure**: Implementación del acceso a datos usando **Entity Framework Core** configurado para **MySQL**, incluyendo repositorios concretos y migraciones.
5. **PasswordManager.Tests**: Suite de pruebas unitarias y de integración para validar la autenticación y la gestión de la bóveda (*vault*).

---

## 🛠️ Tecnologías Utilizadas

* **Lenguaje & Framework:** C# / .NET 10
* **API:** ASP.NET Core Minimal APIs
* **Base de Datos:** MySQL / EF Core (Code-First)
* **Autenticación:** Google OAuth 2.0 & JSON Web Tokens (JWT)
* **Contenedores:** Docker & Docker Compose
* **Testing:** xUnit / Moq

---

## 🚀 Despliegue con Docker

Para levantar toda la infraestructura (API + Base de Datos MySQL) en un solo comando:

```bash
docker-compose up -d --build
