using Ecom.OneCloud.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Ecom.OneCloud.Controllers
{
    public class BaseController : Controller
    {
        public ILoggingService _loggingService { get; }
        public BaseController(ILoggingService loggingService)
        {
            _loggingService = loggingService;
        }

        protected string GetControllerActionNames()
        {
            var controller = ControllerContext.ActionDescriptor.ControllerName;
            var action = ControllerContext.ActionDescriptor.ActionName;
            return $"{controller} - {action}";
        }

        protected IActionResult ModelError(string message)
        {
            var location = GetControllerActionNames();
            ModelState.AddModelError(location, message);
            _loggingService.LogError($"{location}: {message}");
            var options = HttpContext.RequestServices.GetRequiredService<IOptions<ApiBehaviorOptions>>();
            return options.Value.InvalidModelStateResponseFactory(ControllerContext);
        }
    }
}
