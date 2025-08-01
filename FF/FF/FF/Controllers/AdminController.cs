using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace FF.Controllers
{
    [Authorize(Roles = "Admin")] // Only Admin role can access this controller
    public class AdminController : Controller
    {
        private readonly ILogger<AdminController> _logger;

        public AdminController(ILogger<AdminController> logger)
        {
            _logger = logger;
        }

        // GET: /Admin/Dashboard
        // This action will render the empty dashboard view for the Admin.
        public IActionResult Dashboard()
        {
            _logger.LogInformation("Admin Dashboard accessed.");
            return View();
        }
    }
}
