using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ItemProcessorApp.Filters;

public class AuthFilter : IActionFilter
{
    private static readonly string[] PublicActions = { "Login" };
    private static readonly string[] PublicControllers = { "Auth" };

    public void OnActionExecuting(ActionExecutingContext context)
    {
        var controllerName = context.RouteData.Values["controller"]?.ToString() ?? "";
        var actionName = context.RouteData.Values["action"]?.ToString() ?? "";

        if (PublicControllers.Contains(controllerName, StringComparer.OrdinalIgnoreCase))
            return;

        var userId = context.HttpContext.Session.GetInt32("UserId");
        if (userId == null)
        {
            context.Result = new RedirectToActionResult("Login", "Auth", null);
        }
    }

    public void OnActionExecuted(ActionExecutedContext context) { }
}
