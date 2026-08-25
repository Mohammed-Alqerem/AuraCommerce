using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OnlineStore.Constants;
using OnlineStore.Data;
using OnlineStore.Models;

namespace OnlineStore.Services;

public sealed record ExternalIdentity(
    string Provider,
    string ProviderKey,
    string? Email,
    bool EmailVerified,
    string? DisplayName);

public enum ExternalAccountResolutionKind
{
    SignedIn,
    LinkRequired,
    Rejected
}

public sealed record ExternalAccountResolution(
    ExternalAccountResolutionKind Kind,
    Users? User = null,
    string? NormalizedEmail = null);

public interface IExternalAccountService
{
    Task<ExternalAccountResolution> ResolveAsync(
        ExternalIdentity identity,
        CancellationToken cancellationToken = default);

    Task<bool> LinkAsync(
        Users user,
        ExternalIdentity identity,
        CancellationToken cancellationToken = default);
}

public sealed class ExternalAccountService(
    ApplicationDbContext context,
    IPasswordHasher<Users> passwordHasher,
    ILogger<ExternalAccountService> logger) : IExternalAccountService
{
    private readonly EmailAddressAttribute _emailValidator = new();

    public async Task<ExternalAccountResolution> ResolveAsync(
        ExternalIdentity identity,
        CancellationToken cancellationToken = default)
    {
        if (!IsValidIdentity(identity))
        {
            return new(ExternalAccountResolutionKind.Rejected);
        }

        var linkedUser = await context.UserExternalLogins
            .AsNoTracking()
            .Where(login => login.Provider == identity.Provider && login.ProviderKey == identity.ProviderKey)
            .Select(login => login.User)
            .SingleOrDefaultAsync(cancellationToken);

        if (linkedUser is not null)
        {
            return linkedUser.Role == UserRoles.Customer
                ? new(ExternalAccountResolutionKind.SignedIn, linkedUser)
                : new(ExternalAccountResolutionKind.Rejected);
        }

        if (!identity.EmailVerified ||
            string.IsNullOrWhiteSpace(identity.Email) ||
            !_emailValidator.IsValid(identity.Email))
        {
            return new(ExternalAccountResolutionKind.Rejected);
        }

        var email = identity.Email.Trim();
        var normalizedEmail = NormalizeEmail(email);
        var existingUser = await context.Users
            .AsNoTracking()
            .SingleOrDefaultAsync(user => user.NormalizedEmail == normalizedEmail, cancellationToken);

        if (existingUser is not null)
        {
            return existingUser.Role == UserRoles.Customer
                ? new(ExternalAccountResolutionKind.LinkRequired, NormalizedEmail: normalizedEmail)
                : new(ExternalAccountResolutionKind.Rejected);
        }

        var user = new Users
        {
            Name = CreateDisplayName(identity.DisplayName, email),
            Email = email,
            NormalizedEmail = normalizedEmail,
            Role = UserRoles.Customer,
            EmailConfirmed = true,
            CreatedAt = DateTime.UtcNow,
            Cart = new Cart { CreatedAt = DateTime.UtcNow }
        };
        var generatedCredential = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
        user.Password = passwordHasher.HashPassword(user, generatedCredential);
        user.ExternalLogins.Add(new UserExternalLogin
        {
            Provider = identity.Provider,
            ProviderKey = identity.ProviderKey,
            CreatedAt = DateTime.UtcNow
        });

        context.Users.Add(user);
        try
        {
            await context.SaveChangesAsync(cancellationToken);
            return new(ExternalAccountResolutionKind.SignedIn, user);
        }
        catch (DbUpdateException exception)
        {
            logger.LogWarning(exception, "External account creation encountered a uniqueness conflict.");
            return new(ExternalAccountResolutionKind.Rejected);
        }
    }

    public async Task<bool> LinkAsync(
        Users user,
        ExternalIdentity identity,
        CancellationToken cancellationToken = default)
    {
        if (user.Role != UserRoles.Customer ||
            !IsValidIdentity(identity) ||
            !identity.EmailVerified ||
            string.IsNullOrWhiteSpace(identity.Email) ||
            !string.Equals(user.NormalizedEmail, NormalizeEmail(identity.Email), StringComparison.Ordinal))
        {
            return false;
        }

        var providerLogin = await context.UserExternalLogins
            .SingleOrDefaultAsync(
                login => login.Provider == identity.Provider && login.ProviderKey == identity.ProviderKey,
                cancellationToken);
        if (providerLogin is not null)
        {
            return providerLogin.UserId == user.Id;
        }

        var userAlreadyLinked = await context.UserExternalLogins.AnyAsync(
            login => login.UserId == user.Id && login.Provider == identity.Provider,
            cancellationToken);
        if (userAlreadyLinked)
        {
            return false;
        }

        context.UserExternalLogins.Add(new UserExternalLogin
        {
            UserId = user.Id,
            Provider = identity.Provider,
            ProviderKey = identity.ProviderKey,
            CreatedAt = DateTime.UtcNow
        });

        try
        {
            await context.SaveChangesAsync(cancellationToken);
            return true;
        }
        catch (DbUpdateException exception)
        {
            logger.LogWarning(exception, "External account linking encountered a uniqueness conflict for user {UserId}.", user.Id);
            return false;
        }
    }

    private static bool IsValidIdentity(ExternalIdentity identity) =>
        ExternalAuthenticationSchemes.IsSupported(identity.Provider) &&
        !string.IsNullOrWhiteSpace(identity.ProviderKey) &&
        identity.ProviderKey.Length <= 256;

    private static string NormalizeEmail(string email) => email.Trim().ToUpperInvariant();

    private static string CreateDisplayName(string? displayName, string email)
    {
        var candidate = string.IsNullOrWhiteSpace(displayName)
            ? email.Split('@', 2)[0]
            : displayName.Trim();
        candidate = candidate.Length > 50 ? candidate[..50] : candidate;
        return candidate.Length >= 3 ? candidate : "Aura customer";
    }
}
