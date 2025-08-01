using FF.Data;
using FF.Models;
using FF.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace FF.Controllers
{
    [Authorize(Roles = "Customer,TankOwner,Admin")]
    public class OrdersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<OrdersController> _logger;
        private readonly NotificationService _notificationService;
        private readonly UserManager<ApplicationUser> _userManager;

        public OrdersController(ApplicationDbContext context, ILogger<OrdersController> logger,
            NotificationService notificationService, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _logger = logger;
            _notificationService = notificationService;
            _userManager = userManager;
        }

        // GET: Orders
        public async Task<IActionResult> Index()
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (currentUserId != null)
            {
                _logger.LogInformation($"DEBUG: OrdersController Index - User ID: {currentUserId}");
            }
            else
            {
                _logger.LogWarning("DEBUG: OrdersController Index - User ID is NULL. User might not be fully authenticated or claim is missing.");
            }
            var ordersQuery = _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.Tank)
                .ThenInclude(t => t.Owner)
                .AsQueryable();

            if (User.Identity.IsAuthenticated)
            {
                if (User.IsInRole("Customer") && !User.IsInRole("Admin"))
                {
                    ordersQuery = ordersQuery.Where(o => o.CustomerId == currentUserId);
                }
                else if (User.IsInRole("TankOwner") && !User.IsInRole("Admin"))
                {
                    ordersQuery = ordersQuery.Where(o => o.Tank.OwnerId == currentUserId);
                }
            }
            else
            {
                ordersQuery = ordersQuery.Where(o => false);
            }
            return View(await ordersQuery.ToListAsync());
        }

        // GET: Orders/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var order = await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.Tank)
                .ThenInclude(t => t.Owner)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (order == null) return NotFound();

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!User.IsInRole("Admin"))
            {
                bool isCustomer = User.IsInRole("Customer") && order.CustomerId == currentUserId;
                bool isTankOwner = User.IsInRole("TankOwner") && order.Tank.OwnerId == currentUserId;
                if (!isCustomer && !isTankOwner) return Forbid();
            }
            return View(order);
        }

        // GET: Orders/Create
        [Authorize(Roles = "Customer,Admin")]
        public IActionResult Create()
        {
            if (User.IsInRole("Admin"))
            {
                ViewData["CustomerId"] = new SelectList(_context.Users, "Id", "FullName");
                ViewData["TankId"] = new SelectList(_context.Tanks, "Id", "Name");
            }
            else
            {
                ViewData["TankId"] = new SelectList(_context.Tanks, "Id", "Name");
            }
            return View();
        }

        // POST: Orders/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Customer,Admin")]
        public async Task<IActionResult> Create([Bind("CustomerId,TankId,QuantityInBarrels,DeliveryAddress,ContactPhoneNumber,Id")] Order order)
        {
            if (User.IsInRole("Customer") && !User.IsInRole("Admin"))
            {
                order.CustomerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                // هذا السطر يحل مشكلة الـ ModelState للزبائن
                ModelState.Remove("CustomerId");
            }

            order.OrderDate = DateTime.Now;
            order.Status = OrderStatus.New;

            var selectedTank = await _context.Tanks.FindAsync(order.TankId);
            if (selectedTank != null)
            {
                order.TotalPrice = order.QuantityInBarrels * selectedTank.PricePerBarrel;
            }
            else
            {
                ModelState.AddModelError("TankId", "Selected tank is not valid.");
            }

            if (ModelState.IsValid)
            {
                _context.Add(order);
                await _context.SaveChangesAsync();

                var tank = await _context.Tanks.Include(t => t.Owner).FirstOrDefaultAsync(t => t.Id == order.TankId);
                if (tank != null && tank.Owner != null && !string.IsNullOrEmpty(tank.Owner.DeviceToken))
                {
                    await _notificationService.SendNotification(
                        tank.Owner.DeviceToken,
                        "طلب جديد!",
                        $"تم استلام طلب جديد على خزانك '{tank.Name}' من العميل {User.Identity.Name}."
                    );
                }

                return RedirectToAction(nameof(Index));
            }

            if (User.IsInRole("Admin"))
            {
                ViewData["CustomerId"] = new SelectList(_context.Users, "Id", "FullName", order.CustomerId);
                ViewData["TankId"] = new SelectList(_context.Tanks, "Id", "Name", order.TankId);
            }
            else
            {
                ViewData["TankId"] = new SelectList(_context.Tanks, "Id", "Name", order.TankId);
            }
            return View(order);
        }

        // GET: Orders/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var order = await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.Tank)
                .ThenInclude(t => t.Owner)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (order == null) return NotFound();

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!User.IsInRole("Admin"))
            {
                bool isCustomer = User.IsInRole("Customer") && order.CustomerId == currentUserId;
                bool isTankOwner = User.IsInRole("TankOwner") && order.Tank.OwnerId == currentUserId;
                if (!isCustomer && !isTankOwner) return Forbid();
            }

            if (User.IsInRole("Admin"))
            {
                ViewData["CustomerId"] = new SelectList(_context.Users, "Id", "FullName", order.CustomerId);
                ViewData["TankId"] = new SelectList(_context.Tanks, "Id", "Name", order.TankId);
                ViewData["StatusList"] = new SelectList(Enum.GetValues(typeof(OrderStatus)), order.Status);
            }
            else if (User.IsInRole("TankOwner"))
            {
                ViewData["StatusList"] = new SelectList(new List<OrderStatus> { OrderStatus.InProgress, OrderStatus.Delivered }, order.Status);
            }
            else if (User.IsInRole("Customer"))
            {
                if (order.Status == OrderStatus.New)
                {
                    ViewData["StatusList"] = new SelectList(new List<OrderStatus> { OrderStatus.New, OrderStatus.Canceled }, order.Status);
                }
                else
                {
                    ViewData["StatusList"] = new SelectList(new List<OrderStatus> { order.Status }, order.Status);
                }
            }
            return View(order);
        }

        // POST: Orders/Edit/5
        [Authorize(Roles = "Admin,TankOwner,Customer")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,CustomerId,TankId,QuantityInBarrels,TotalPrice,OrderDate,Status,DeliveryAddress,ContactPhoneNumber")] Order order)
        {
            if (id != order.Id) return NotFound();

            var existingOrder = await _context.Orders.AsNoTracking()
                .Include(o => o.Tank)
                .ThenInclude(t => t.Owner)
                .Include(o => o.Customer)
                .FirstOrDefaultAsync(o => o.Id == id);
            if (existingOrder == null) return NotFound();

            var oldStatus = existingOrder.Status;
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!User.IsInRole("Admin"))
            {
                bool isCustomer = User.IsInRole("Customer") && existingOrder.CustomerId == currentUserId;
                bool isTankOwner = User.IsInRole("TankOwner") && existingOrder.Tank.OwnerId == currentUserId;

                if (!isCustomer && !isTankOwner) return Forbid();

                if (isCustomer)
                {
                    if (existingOrder.Status == OrderStatus.New && order.Status == OrderStatus.Canceled)
                    {
                        order.Status = OrderStatus.Canceled;
                        // Add a message to TempData
                        TempData["StatusMessage"] = "تم إلغاء الطلب بنجاح.";
                    }
                    else if (order.Status != existingOrder.Status)
                    {
                        // Add an error message to ModelState
                        ModelState.AddModelError("Status", "يمكنك فقط إلغاء الطلبات الجديدة.");
                        order.Status = existingOrder.Status;
                    }
                    order.CustomerId = existingOrder.CustomerId;
                    order.TankId = existingOrder.TankId;
                    order.QuantityInBarrels = existingOrder.QuantityInBarrels;
                    order.TotalPrice = existingOrder.TotalPrice;
                    order.OrderDate = existingOrder.OrderDate;
                    order.DeliveryAddress = existingOrder.DeliveryAddress;
                    order.ContactPhoneNumber = existingOrder.ContactPhoneNumber;
                }
                else if (isTankOwner)
                {
                    if (order.Status == OrderStatus.InProgress || order.Status == OrderStatus.Delivered)
                    {
                        // Allowed to change status
                    }
                    else if (order.Status != existingOrder.Status)
                    {
                        ModelState.AddModelError("Status", "يمكنك فقط تغيير الحالة إلى InProgress أو Delivered.");
                        order.Status = existingOrder.Status;
                    }
                    order.CustomerId = existingOrder.CustomerId;
                    order.TankId = existingOrder.TankId;
                    order.QuantityInBarrels = existingOrder.QuantityInBarrels;
                    order.TotalPrice = existingOrder.TotalPrice;
                    order.OrderDate = existingOrder.OrderDate;
                    order.DeliveryAddress = existingOrder.DeliveryAddress;
                    order.ContactPhoneNumber = existingOrder.ContactPhoneNumber;
                }
            }

            if (User.IsInRole("Admin") || (User.IsInRole("Customer") && existingOrder.QuantityInBarrels != order.QuantityInBarrels))
            {
                var selectedTank = await _context.Tanks.FindAsync(order.TankId);
                if (selectedTank != null)
                {
                    order.TotalPrice = order.QuantityInBarrels * selectedTank.PricePerBarrel;
                }
                else
                {
                    ModelState.AddModelError("TankId", "Selected tank is not valid.");
                }
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(order);
                    await _context.SaveChangesAsync();

                    if (oldStatus != order.Status)
                    {
                        if (existingOrder.Customer != null && !string.IsNullOrEmpty(existingOrder.Customer.DeviceToken))
                        {
                            await _notificationService.SendNotification(
                                existingOrder.Customer.DeviceToken,
                                "تحديث حالة الطلب",
                                $"تم تحديث حالة طلبك رقم {existingOrder.Id} إلى '{order.Status}'."
                            );
                        }
                    }
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!OrderExists(order.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }

            if (User.IsInRole("Admin"))
            {
                ViewData["CustomerId"] = new SelectList(_context.Users, "Id", "FullName", order.CustomerId);
                ViewData["TankId"] = new SelectList(_context.Tanks, "Id", "Name", order.TankId);
                ViewData["StatusList"] = new SelectList(Enum.GetValues(typeof(OrderStatus)), order.Status);
            }
            else if (User.IsInRole("TankOwner"))
            {
                ViewData["StatusList"] = new SelectList(new List<OrderStatus> { OrderStatus.InProgress, OrderStatus.Delivered }, order.Status);
            }
            else if (User.IsInRole("Customer"))
            {
                if (existingOrder.Status == OrderStatus.New)
                {
                    ViewData["StatusList"] = new SelectList(new List<OrderStatus> { OrderStatus.New, OrderStatus.Canceled }, order.Status);
                }
                else
                {
                    ViewData["StatusList"] = new SelectList(new List<OrderStatus> { existingOrder.Status }, existingOrder.Status);
                }
            }
            return View(order);
        }

        // GET: Orders/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var order = await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.Tank)
                .ThenInclude(t => t.Owner)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (order == null) return NotFound();

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!User.IsInRole("Admin"))
            {
                bool isCustomer = User.IsInRole("Customer") && order.CustomerId == currentUserId;
                bool isTankOwner = User.IsInRole("TankOwner") && order.Tank.OwnerId == currentUserId;

                if (!isCustomer && !isTankOwner) return Forbid(); // ممنوع على غير المالك والزبون

                if (isCustomer && order.Status != OrderStatus.New)
                {
                    TempData["StatusMessage"] = "يمكن للزبائن حذف (إلغاء) الطلبات الجديدة فقط.";
                    return RedirectToAction(nameof(Index)); // أو يمكنك إرجاع Forbid()
                }

                if (isTankOwner && !(order.Status == OrderStatus.New || order.Status == OrderStatus.InProgress))
                {
                    TempData["StatusMessage"] = "يمكن لمالكي الخزانات حذف (إلغاء) الطلبات الجديدة أو قيد التنفيذ فقط.";
                    return RedirectToAction(nameof(Index)); // أو يمكنك إرجاع Forbid()
                }
            }
            return View(order);
        }

        // POST: Orders/Delete/5
        [Authorize(Roles = "Admin,Customer,TankOwner")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var order = await _context.Orders
                .Include(o => o.Tank)
                .ThenInclude(t => t.Owner)
                .FirstOrDefaultAsync(o => o.Id == id);
            if (order == null) return NotFound();

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!User.IsInRole("Admin"))
            {
                bool isCustomer = User.IsInRole("Customer") && order.CustomerId == currentUserId;
                bool isTankOwner = User.IsInRole("TankOwner") && order.Tank.OwnerId == currentUserId;

                if (!isCustomer && !isTankOwner) return Forbid(); // ممنوع على غير المالك والزبون

                if (isCustomer && order.Status != OrderStatus.New)
                {
                    TempData["StatusMessage"] = "يمكن للزبائن حذف (إلغاء) الطلبات الجديدة فقط.";
                    return RedirectToAction(nameof(Index));
                }

                if (isTankOwner && !(order.Status == OrderStatus.New || order.Status == OrderStatus.InProgress))
                {
                    TempData["StatusMessage"] = "يمكن لمالكي الخزانات حذف (إلغاء) الطلبات الجديدة أو قيد التنفيذ فقط.";
                    return RedirectToAction(nameof(Index));
                }
            }

            _context.Orders.Remove(order);
            await _context.SaveChangesAsync();
            TempData["StatusMessage"] = $"تم حذف الطلب رقم {order.Id} بنجاح.";
            return RedirectToAction(nameof(Index));
        }

        private bool OrderExists(int id)
        {
            return _context.Orders.Any(e => e.Id == id);
        }
    }
}