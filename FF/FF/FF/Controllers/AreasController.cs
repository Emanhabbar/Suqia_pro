using FF.Data;
using FF.Models; // هذا السطر ضروري لكي يتعرف على Area
using Microsoft.AspNetCore.Authorization; // ضروري لسمة [Authorize]
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; // ضروري لـ ToListAsync, FindAsync, إلخ
using System.Linq; // ضروري لـ Any
using System.Threading.Tasks; // ضروري لـ Task

namespace FF.Controllers
{
    // السماح فقط لدور Admin بالوصول إلى جميع الإجراءات في هذا المتحكم
    [Authorize(Roles = "Admin")]
    public class AreasController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AreasController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Areas
        // يعرض قائمة بجميع المناطق.
        public async Task<IActionResult> Index()
        {
            return View(await _context.Areas.ToListAsync());
        }

        // GET: Areas/Details/5
        // يعرض تفاصيل منطقة محددة.
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var area = await _context.Areas
                .FirstOrDefaultAsync(m => m.Id == id);
            if (area == null)
            {
                return NotFound();
            }

            return View(area);
        }

        // GET: Areas/Create
        // يعرض نموذج إنشاء منطقة جديدة.
        public IActionResult Create()
        {
            return View();
        }

        // POST: Areas/Create
        // يعالج إرسال نموذج إنشاء منطقة جديدة.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Id")] Area area)
        {
            if (ModelState.IsValid)
            {
                _context.Add(area);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(area);
        }

        // GET: Areas/Edit/5
        // يعرض نموذج تعديل منطقة موجودة.
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var area = await _context.Areas.FindAsync(id);
            if (area == null)
            {
                return NotFound();
            }
            return View(area);
        }

        // POST: Areas/Edit/5
        // يعالج إرسال نموذج تعديل منطقة موجودة.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name")] Area area)
        {
            if (id != area.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(area);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AreaExists(area.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(area);
        }

        // GET: Areas/Delete/5
        // يعرض صفحة تأكيد حذف منطقة.
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var area = await _context.Areas
                .FirstOrDefaultAsync(m => m.Id == id);
            if (area == null)
            {
                return NotFound();
            }

            return View(area);
        }

        // POST: Areas/Delete/5
        // يعالج تأكيد حذف منطقة.
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var area = await _context.Areas.FindAsync(id);
            if (area != null)
            {
                _context.Areas.Remove(area);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        // التحقق مما إذا كانت المنطقة موجودة
        private bool AreaExists(int id)
        {
            return _context.Areas.Any(e => e.Id == id);
        }
    }
}
