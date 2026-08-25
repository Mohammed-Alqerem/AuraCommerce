using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using OnlineStore.Constants;
using OnlineStore.Models;
using OnlineStore.Services;

namespace AuraCommerce.Tests;

public class ExternalAccountServiceTests
{
    [Fact]
    public async Task ResolveAsync_NewVerifiedIdentity_CreatesCustomerCartAndLogin()
    {
        await using var database = await TestDatabase.CreateAsync();
        var service = CreateService(database);

        var result = await service.ResolveAsync(new ExternalIdentity(
            ExternalAuthenticationSchemes.Google,
            "google-subject-1",
            "new.customer@example.com",
            true,
            "New Customer"));

        Assert.Equal(ExternalAccountResolutionKind.SignedIn, result.Kind);
        Assert.NotNull(result.User);
        var saved = await database.Context.Users
            .Include(user => user.Cart)
            .Include(user => user.ExternalLogins)
            .SingleAsync(user => user.NormalizedEmail == "NEW.CUSTOMER@EXAMPLE.COM");
        Assert.Equal(UserRoles.Customer, saved.Role);
        Assert.True(saved.EmailConfirmed);
        Assert.NotEmpty(saved.Password);
        Assert.NotNull(saved.Cart);
        var login = Assert.Single(saved.ExternalLogins);
        Assert.Equal(ExternalAuthenticationSchemes.Google, login.Provider);
        Assert.Equal("google-subject-1", login.ProviderKey);
    }

    [Fact]
    public async Task ResolveAsync_ExistingEmail_RequiresPasswordLinkInsteadOfAutoLinking()
    {
        await using var database = await TestDatabase.CreateAsync();
        var user = await AddCustomerAsync(database, "existing@example.com");
        var service = CreateService(database);

        var result = await service.ResolveAsync(new ExternalIdentity(
            ExternalAuthenticationSchemes.Apple,
            "apple-subject-1",
            user.Email,
            true,
            user.Name));

        Assert.Equal(ExternalAccountResolutionKind.LinkRequired, result.Kind);
        Assert.Equal(user.NormalizedEmail, result.NormalizedEmail);
        Assert.Empty(await database.Context.UserExternalLogins.ToListAsync());
    }

    [Fact]
    public async Task ResolveAsync_UnverifiedEmail_IsRejectedWithoutCreatingAccount()
    {
        await using var database = await TestDatabase.CreateAsync();
        var service = CreateService(database);

        var result = await service.ResolveAsync(new ExternalIdentity(
            ExternalAuthenticationSchemes.Google,
            "google-subject-2",
            "unverified@example.com",
            false,
            "Unverified Customer"));

        Assert.Equal(ExternalAccountResolutionKind.Rejected, result.Kind);
        Assert.False(await database.Context.Users.AnyAsync(user => user.NormalizedEmail == "UNVERIFIED@EXAMPLE.COM"));
        Assert.Empty(await database.Context.UserExternalLogins.ToListAsync());
    }

    [Fact]
    public async Task ResolveAsync_KnownProviderSubject_SignsInLinkedCustomer()
    {
        await using var database = await TestDatabase.CreateAsync();
        var user = await AddCustomerAsync(database, "linked@example.com");
        database.Context.UserExternalLogins.Add(new UserExternalLogin
        {
            UserId = user.Id,
            Provider = ExternalAuthenticationSchemes.Google,
            ProviderKey = "known-subject"
        });
        await database.Context.SaveChangesAsync();
        var service = CreateService(database);

        var result = await service.ResolveAsync(new ExternalIdentity(
            ExternalAuthenticationSchemes.Google,
            "known-subject",
            null,
            false,
            null));

        Assert.Equal(ExternalAccountResolutionKind.SignedIn, result.Kind);
        Assert.Equal(user.Id, result.User?.Id);
    }

    [Fact]
    public async Task LinkAsync_RequiresMatchingVerifiedCustomerEmail()
    {
        await using var database = await TestDatabase.CreateAsync();
        var user = await AddCustomerAsync(database, "owner@example.com");
        var service = CreateService(database);
        var mismatched = new ExternalIdentity(
            ExternalAuthenticationSchemes.Apple,
            "apple-subject-2",
            "someone.else@example.com",
            true,
            null);

        Assert.False(await service.LinkAsync(user, mismatched));
        Assert.Empty(await database.Context.UserExternalLogins.ToListAsync());

        var matching = mismatched with { Email = user.Email };
        Assert.True(await service.LinkAsync(user, matching));
        var saved = await database.Context.UserExternalLogins.SingleAsync();
        Assert.Equal(user.Id, saved.UserId);
    }

    [Fact]
    public async Task ResolveAsync_LinkedAdministrator_IsRejected()
    {
        await using var database = await TestDatabase.CreateAsync();
        var administrator = await database.Context.Users
            .FirstAsync(user => user.Role == UserRoles.Admin);
        database.Context.UserExternalLogins.Add(new UserExternalLogin
        {
            UserId = administrator.Id,
            Provider = ExternalAuthenticationSchemes.Google,
            ProviderKey = "admin-subject"
        });
        await database.Context.SaveChangesAsync();
        var service = CreateService(database);

        var result = await service.ResolveAsync(new ExternalIdentity(
            ExternalAuthenticationSchemes.Google,
            "admin-subject",
            administrator.Email,
            true,
            administrator.Name));

        Assert.Equal(ExternalAccountResolutionKind.Rejected, result.Kind);
        Assert.Null(result.User);
    }

    private static ExternalAccountService CreateService(TestDatabase database) =>
        new(
            database.Context,
            new PasswordHasher<Users>(),
            NullLogger<ExternalAccountService>.Instance);

    private static async Task<Users> AddCustomerAsync(TestDatabase database, string email)
    {
        var user = new Users
        {
            Name = "Existing Customer",
            Email = email,
            NormalizedEmail = email.ToUpperInvariant(),
            Role = UserRoles.Customer,
            CreatedAt = DateTime.UtcNow,
            Cart = new Cart { CreatedAt = DateTime.UtcNow }
        };
        user.Password = new PasswordHasher<Users>().HashPassword(user, "StrongPass1!");
        database.Context.Users.Add(user);
        await database.Context.SaveChangesAsync();
        return user;
    }
}
