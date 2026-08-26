namespace PasswordManager.API.Models;

public record AddVaultEntryRequest(string Name, string EncryptedBlob, string Iv);
public record UpdateVaultEntryRequest(string Name, string EncryptedBlob, string Iv);