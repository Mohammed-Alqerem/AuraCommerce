using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using OnlineStore.Extensions;

namespace OnlineStore.Filters
{
    public class RequireLoginAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var userId = context.HttpContext.Session.GetCurrentUserId();
            if (!userId.HasValue)
            {
                var returnUrl = context.HttpContext.Request.Path + context.HttpContext.Request.QueryString;
                context.Result = new RedirectToActionResult("Login", "Account", new { returnUrl });
                return;
            }

            base.OnActionExecuting(context);
        }
    }
}
