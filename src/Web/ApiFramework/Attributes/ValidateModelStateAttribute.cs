using CleanTemplate.ApiFramework.Tools;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace CleanTemplate.ApiFramework.Attributes
{
    /// <summary>
    /// Action filter that validates <see cref="ActionExecutingContext.ModelState"/> before
    /// a controller action runs. Applied to controllers through filters registration.
    /// </summary>
    public class ValidateModelStateAttribute : ActionFilterAttribute
    {
        /// <summary>
        /// Executes before the controller action. If the model state is invalid a
        /// <see cref="ApiResult"/> containing the validation errors is returned and the
        /// action is short-circuited.
        /// </summary>
        /// <param name="context">Current action context supplied by ASP.NET Core.</param>
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (!context.ModelState.IsValid)
            {
                var modelStateEntries = context.ModelState.Values;

                foreach (var item in modelStateEntries)
                {
                    foreach (var error in item.Errors)
                    {
                        ApiResult resultObject = new ApiResult(error.ErrorMessage);
                        context.Result = new JsonResult(resultObject);
                    }
                }
            }
        }
    }
}
