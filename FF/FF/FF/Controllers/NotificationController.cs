using FF.Data;
using FF.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace FF.Controllers
{
    [Authorize] // تأكدي أن المستخدم مسجل الدخول قبل الوصول لهذا الـ Controller
    [ApiController]
    [Route("api/[controller]")]
    public class NotificationController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public NotificationController(UserManager<ApplicationUser> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        [HttpPost("updateToken")]
        public async Task<IActionResult> UpdateDeviceToken([FromBody] string token)
        {
            // الحصول على المستخدم الحالي
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                // يمكن أن يحدث إذا لم يتم العثور على المستخدم
                return NotFound("User not found.");
            }

            // تحديث DeviceToken للمستخدم
            user.DeviceToken = token;

            // حفظ التغييرات في قاعدة البيانات
            await _context.SaveChangesAsync();

            // إعادة إجابة ناجحة
            return Ok();
        }
    }
}