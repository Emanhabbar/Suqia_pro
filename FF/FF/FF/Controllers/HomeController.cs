using System.Diagnostics;
using FF.Models;
using Microsoft.AspNetCore.Mvc;
using FF.Data; // ����� ��� ��������� ������ ��� ApplicationDbContext
using Microsoft.EntityFrameworkCore; // ����� ��� ��������� �� CountAsync
using System.Threading.Tasks; // ����� ��� ��������� �� Task � async/await
using Microsoft.AspNetCore.Authorization; // ������ ��� [Authorize]
using Microsoft.AspNetCore.Identity; // ��� ��� ������ UserManager � SignInManager �� �������

namespace FF.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context; // ����� ApplicationDbContext

        // ����� Constructor ���� ApplicationDbContext
        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context; // ����� DbContext
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        // ����� ����� AdminDashboard
         [Authorize(Roles = "Admin")] // ���� ����� ��� ����� ����� �� ��������� ��� ������ ������
        public async Task<IActionResult> AdminDashboard()
        {
            // �� ���� ���� �������� ��� ��� ��� View ���� ���� �����
            // ���� ��� ���� ������� �� ������� ������ ��� ��� ���:
            // var totalUsers = await _context.Users.CountAsync();
            // ViewData["TotalUsers"] = totalUsers; // ���� ��� ����� ��������

            return View(); // ����� �� Views/Home/AdminDashboard.cshtml
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
