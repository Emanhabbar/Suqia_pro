using System.Diagnostics;
using FF.Models;
using Microsoft.AspNetCore.Mvc;
using FF.Data; // ≈÷«›… Â–« «·«” Ì—«œ ··Ê’Ê· ≈·Ï ApplicationDbContext
using Microsoft.EntityFrameworkCore; // ≈÷«›… Â–« «·«” Ì—«œ ·‹ CountAsync
using System.Threading.Tasks; // ≈÷«›… Â–« «·«” Ì—«œ ·‹ Task Ê async/await
using Microsoft.AspNetCore.Authorization; // ·≈÷«›… ”„… [Authorize]
using Microsoft.AspNetCore.Identity; // ≈–« ﬂ‰   ” Œœ„ UserManager Ê SignInManager ›Ì «·„ Õﬂ„

namespace FF.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context; //  ⁄—Ì› ApplicationDbContext

        //  ÕœÌÀ Constructor ·Õﬁ‰ ApplicationDbContext
        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context; //  ÂÌ∆… DbContext
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        // ≈÷«›… ≈Ã—«¡ AdminDashboard
         [Authorize(Roles = "Admin")] // Ì„ﬂ‰ ≈÷«›… Â–Â «·”„… ·÷„«‰ √‰ «·„”ƒÊ·Ì‰ ›ﬁÿ Ì„ﬂ‰Â„ «·Ê’Ê·
        public async Task<IActionResult> AdminDashboard()
        {
            // ·« Õ«Ã… ·Ã·» «·»Ì«‰«  Â‰« ≈–« ﬂ«‰ View ÌﬁÊ„ »–·ﬂ »‰›”Â°
            // Ê·ﬂ‰ ≈–« √—œ   „—Ì—Â« „‰ «·„ Õﬂ„° ›Ì„ﬂ‰ﬂ ›⁄· –·ﬂ Â‰«:
            // var totalUsers = await _context.Users.CountAsync();
            // ViewData["TotalUsers"] = totalUsers; // „À«· ⁄·Ï  „—Ì— «·»Ì«‰« 

            return View(); // ”Ì»ÕÀ ⁄‰ Views/Home/AdminDashboard.cshtml
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
