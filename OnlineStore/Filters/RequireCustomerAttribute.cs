using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using OnlineStore.Constants;
using OnlineStore.Extensions;

namespace OnlineStore.Filters
{
    public class RequireCustomerAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var session = context.HttpContext.Session;
            var userId = session.GetCurrentUserId();
            if (!userId.HasValue)
            {
                var returnUrl = context.HttpContext.Request.Path + context.HttpContext.Request.QueryString;
                context.Result = new RedirectToActionResult("Login", "Account", new { returnUrl });
                return;
            }

            if (session.IsInRole(UserRoles.Admin))
            {
                context.Result = new RedirectToActionResult("Index", "Admin", null);
                return;
            }

            if (!session.IsInRole(UserRoles.Customer))
            {
                session.Clear();
                context.Result = new RedirectToActionResult("Login", "Account", null);
                return;
            }

            base.OnActionExecuting(context);
        }
    }
}
