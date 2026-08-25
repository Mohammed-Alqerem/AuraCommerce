using System.Text.Json;
using System.Security.Cryptography;
using Microsoft.AspNetCore.DataProtection;

namespace OnlineStore.Services;

public sealed record AccountTokenPayload(int UserId, string Email, int SecurityVersion, string Purpose);

public interface IAccountTokenService
{
    string Create(int userId, string email, int securityVersion, string purpose, TimeSpan lifetime);
    bool TryRead(string token, string purpose, out AccountTokenPayload? payload);
}

public sealed class AccountTokenService(IDataProtectionProvider provider) : IAccountTokenService
{
    private const string ProtectorPurpose = "AuraCommerce.AccountTokens.v1";

    public string Create(int userId, string email, int securityVersion, string purpose, TimeSpan lifetime)
    {
        var protector = provider.CreateProtector(ProtectorPurpose, purpose).ToTimeLimitedDataProtector();
        return protector.Protect(JsonSerializer.Serialize(new AccountTokenPayload(userId, email, securityVersion, purpose)), lifetime);
    }

    public bool TryRead(string token, string purpose, out AccountTokenPayload? payload)
    {
        payload = null;
        if (string.IsNullOrWhiteSpace(token) || string.IsNullOrWhiteSpace(purpose))
        {
            return false;
        }

        try
        {
            var protector = provider.CreateProtector(ProtectorPurpose, purpose).ToTimeLimitedDataProtector();
            payload = JsonSerializer.Deserialize<AccountTokenPayload>(protector.Unprotect(token));
            return payload is not null && payload.Purpose == purpose;
        }
        catch (Exception exception) when (exception is CryptographicException or JsonException)
        {
            return false;
        }
    }
}

public interface IStoreEmailSender
{
    Task<bool> SendAsync(string recipient, string subject, string htmlBody, CancellationToken cancellationToken = default);
}

public sealed class UnconfiguredStoreEmailSender(ILogger<UnconfiguredStoreEmailSender> logger) : IStoreEmailSender
{
    public Task<bool> SendAsync(string recipient, string subject, string htmlBody, CancellationToken cancellationToken = default)
    {
        logger.LogWarning("Email delivery is not configured. Message '{Subject}' for {Recipient} was not sent.", subject, recipient);
        return Task.FromResult(false);
    }
}
