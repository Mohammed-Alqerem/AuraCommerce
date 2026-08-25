using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using OnlineStore.Constants;
using OnlineStore.Controllers;
using OnlineStore.Extensions;
using OnlineStore.Filters;
using OnlineStore.Models;
using OnlineStore.Models.ViewModels;
using OnlineStore.Services;

namespace AuraCommerce.Tests;

public class AuthenticationTests
{
    [Fact]
    public void PasswordHasher_AcceptsValidPassword_AndRejectsInvalidPassword()
    {
        var user = new Users();
        var hasher = new PasswordHasher<Users>();
        user.Password = hasher.HashPassword(user, "StrongPass1!");

        Assert.NotEqual(PasswordVerificationResult.Failed,
            hasher.VerifyHashedPassword(user, user.Password, "StrongPass1!"));
        Assert.Equal(PasswordVerificationResult.Failed,
            hasher.VerifyHashedPassword(user, user.Password, "wrong-password"));
    }

    [Fact]
    public void AdminFilter_AllowsAdmin_AndBlocksCustomer()
    {
        var adminContext = CreateActionContext(UserRoles.Admin);
        new RequireAdminAttribute().OnActionExecuting(adminContext);
        Assert.Null(adminContext.Result);

        var customerContext = CreateActionContext(UserRoles.Customer);
        new RequireAdminAttribute().OnActionExecuting(customerContext);
        Assert.IsType<RedirectToActionResult>(customerContext.Result);
    }

    [Fact]
    public void CustomerFilter_BlocksAdmin()
    {
        var context = CreateActionContext(UserRoles.Admin);
        new RequireCustomerAttribute().OnActionExecuting(context);

        var redirect = Assert.IsType<RedirectToActionResult>(context.Result);
        Assert.Equal("Admin", redirect.ControllerName);
    }

    [Fact]
    public async Task ProfilePasswordChange_InvalidatesPreviouslyIssuedTokens()
    {
        await using var database = await TestDatabase.CreateAsync();
        var hasher = new PasswordHasher<Users>();
        var user = new Users
        {
            Id = 500,
            Name = "Security Test",
            Email = "security@example.com",
            NormalizedEmail = "SECURITY@EXAMPLE.COM",
            Phone = "12345",
            Address = "Test address",
            Role = UserRoles.Customer,
            SecurityVersion = 7,
            CreatedAt = DateTime.UtcNow
        };
        user.Password = hasher.HashPassword(user, "OldPass1!");
        database.Context.Users.Add(user);
        await database.Context.SaveChangesAsync();

        var controller = new AccountController(
            database.Context,
            hasher,
            NullLogger<AccountController>.Instance,
            null!,
            null!,
            null!,
            null!,
            new ExternalProviderAvailability(false, false));
        var session = new TestSession();
        session.SetInt32(SessionKeys.UserId, user.Id);
        session.SetString(SessionKeys.UserRole, UserRoles.Customer);
        TestHttpContext.AttachTo(controller, session);

        var result = await controller.Profile(new ProfileViewModel
        {
            Name = user.Name,
            Email = user.Email,
            Phone = user.Phone,
            Address = user.Address,
            CurrentPassword = "OldPass1!",
            NewPassword = "NewPass1!",
            ConfirmNewPassword = "NewPass1!"
        });

        Assert.IsType<RedirectToActionResult>(result);
        var saved = await database.Context.Users.AsNoTracking().SingleAsync(item => item.Id == user.Id);
        Assert.Equal(8, saved.SecurityVersion);
        Assert.NotEqual(PasswordVerificationResult.Failed,
            hasher.VerifyHashedPassword(saved, saved.Password, "NewPass1!"));
    }

    private static ActionExecutingContext CreateActionContext(string role)
    {
        var session = new TestSession();
        session.SetInt32(SessionKeys.UserId, 42);
        session.SetString(SessionKeys.UserRole, role);
        var httpContext = TestHttpContext.WithSession(session);
        var actionContext = new ActionContext(
            httpContext,
            new RouteData(),
            new ActionDescriptor(),
            new ModelStateDictionary());
        return new ActionExecutingContext(
            actionContext,
            [],
            new Dictionary<string, object?>(),
            new object());
    }
}
