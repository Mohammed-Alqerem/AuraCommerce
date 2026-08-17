using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace OnlineStore.Filters
{
    public class RequireCustomerAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var userId = context.HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
            {
                var returnUrl = context.HttpContext.Request.Path + context.HttpContext.Request.QueryString;
                context.Result = new RedirectToActionResult("Login", "Account", new { returnUrl });
                return;
            }

            if (userId.Value == 1)
            {
                context.Result = new RedirectToActionResult("Index", "Admin", null);
                return;
            }

            base.OnActionExecuting(context);
        }
    }
}
