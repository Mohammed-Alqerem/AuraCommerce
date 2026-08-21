using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Routing;
using OnlineStore.Constants;
using OnlineStore.Extensions;
using OnlineStore.Filters;
using OnlineStore.Models;

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
