namespace PasswordManager.API.Models;

public record GoogleLoginRequest(string IdToken);
public record RefreshTokenRequest(string RefreshToken);