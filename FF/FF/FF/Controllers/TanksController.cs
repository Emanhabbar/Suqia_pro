using FF.Data;
using FF.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace FF.Controllers
{
    [Authorize(Roles = "TankOwner,Admin")]
    public class TanksController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TanksController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Tanks
        public async Task<IActionResult> Index()
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var tanksQuery = _context.Tanks
                .Include(t => t.Owner) // Ensure owner is included
                .Include(t => t.Areas) // Ensure areas are included
                .AsQueryable();

            if (User.IsInRole("TankOwner") && !User.IsInRole("Admin"))
            {
                tanksQuery = tanksQuery.Where(t => t.OwnerId == currentUserId);
            }

            return View(await tanksQuery.ToListAsync());
        }

        // GET: Tanks/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tank = await _context.Tanks
                .Include(t => t.Owner)
                .Include(t => t.Areas)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (tank == null)
            {
                return NotFound();
            }

            if (User.IsInRole("TankOwner") && !User.IsInRole("Admin"))
            {
                var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (tank.OwnerId != currentUserId)
                {
                    return Forbid();
                }
            }

            return View(tank);
        }

        // GET: Tanks/Create
        public IActionResult Create()
        {
            if (User.IsInRole("Admin"))
            {
                ViewData["OwnerId"] = new SelectList(_context.Users, "Id", "FullName");
            }
            ViewData["AvailableAreas"] = new SelectList(_context.Areas, "Id", "Name");
            return View();
        }

        // POST: Tanks/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Capacity,PricePerBarrel,Location,TypeOfWater,OwnerId,Id")] Tank tank, int[] selectedAreaIds)
        {
            if (User.IsInRole("TankOwner") && !User.IsInRole("Admin"))
            {
                tank.OwnerId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            }
            else if (!User.IsInRole("Admin") && string.IsNullOrEmpty(tank.OwnerId))
            {
                return Forbid();
            }

            // Remove 'Owner' and 'Areas' from ModelState to prevent validation errors related to navigation properties.
            // These should be set AFTER successful model binding and validation of direct properties.
            ModelState.Remove(nameof(tank.Owner));
            ModelState.Remove(nameof(tank.Areas));
            // Optional: If OwnerId is causing issues after being set, you *might* need to remove it as well,
            // but this is less common if it's a simple string.
            // ModelState.Remove(nameof(tank.OwnerId)); // Consider uncommenting this if problems persist with OwnerId validation

            if (ModelState.IsValid)
            {
                if (selectedAreaIds != null && selectedAreaIds.Length > 0)
                {
                    var areasToAdd = await _context.Areas
                                                    .Where(a => selectedAreaIds.Contains(a.Id))
                                                    .ToListAsync();
                    tank.Areas = areasToAdd;
                }

                _context.Add(tank);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            // Reload ViewData if ModelState is invalid, so the form can be re-rendered with validation messages
            if (User.IsInRole("Admin"))
            {
                ViewData["OwnerId"] = new SelectList(_context.Users, "Id", "FullName", tank.OwnerId);
            }
            ViewData["AvailableAreas"] = new SelectList(_context.Areas, "Id", "Name", selectedAreaIds);

            // If ModelState is invalid, re-render the view with the current tank object to display errors
            return View(tank);
        }

        // GET: Tanks/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tank = await _context.Tanks
                .Include(t => t.Areas)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (tank == null)
            {
                return NotFound();
            }

            if (User.IsInRole("TankOwner") && !User.IsInRole("Admin"))
            {
                var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (tank.OwnerId != currentUserId)
                {
                    return Forbid();
                }
            }

            if (User.IsInRole("Admin"))
            {
                ViewData["OwnerId"] = new SelectList(_context.Users, "Id", "FullName", tank.OwnerId);
            }

            var selectedAreaIds = tank.Areas.Select(a => a.Id).ToList();
            ViewData["AvailableAreas"] = new SelectList(_context.Areas, "Id", "Name", selectedAreaIds);

            return View(tank);
        }

        // POST: Tanks/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Name,Capacity,PricePerBarrel,Location,TypeOfWater,OwnerId,Id")] Tank tank, int[] selectedAreaIds)
        {
            if (id != tank.Id)
            {
                return NotFound();
            }

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var existingTank = await _context.Tanks.AsNoTracking()
                                             .Include(t => t.Areas)
                                             .FirstOrDefaultAsync(t => t.Id == id);

            if (existingTank == null)
            {
                return NotFound();
            }

            if (User.IsInRole("TankOwner") && !User.IsInRole("Admin"))
            {
                if (existingTank.OwnerId != currentUserId)
                {
                    return Forbid();
                }
                tank.OwnerId = existingTank.OwnerId;
            }

            ModelState.Remove(nameof(tank.Owner));
            ModelState.Remove(nameof(tank.Areas));
            // Optional: Consider uncommenting this if problems persist with OwnerId validation
            // ModelState.Remove(nameof(tank.OwnerId)); 

            if (ModelState.IsValid)
            {
                try
                {
                    var tankToUpdate = await _context.Tanks
                                                    .Include(t => t.Areas)
                                                    .FirstOrDefaultAsync(t => t.Id == id);

                    if (tankToUpdate == null) return NotFound();

                    tankToUpdate.Name = tank.Name;
                    tankToUpdate.Capacity = tank.Capacity;
                    tankToUpdate.PricePerBarrel = tank.PricePerBarrel;
                    tankToUpdate.Location = tank.Location;
                    tankToUpdate.TypeOfWater = tank.TypeOfWater;

                    if (User.IsInRole("Admin"))
                    {
                        tankToUpdate.OwnerId = tank.OwnerId;
                    }

                    tankToUpdate.Areas.Clear();
                    if (selectedAreaIds != null && selectedAreaIds.Length > 0)
                    {
                        var areasToAdd = await _context.Areas
                                                        .Where(a => selectedAreaIds.Contains(a.Id))
                                                        .ToListAsync();
                        foreach (var area in areasToAdd)
                        {
                            tankToUpdate.Areas.Add(area);
                        }
                    }

                    _context.Update(tankToUpdate);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TankExists(tank.Id))
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

            if (User.IsInRole("Admin"))
            {
                ViewData["OwnerId"] = new SelectList(_context.Users, "Id", "FullName", tank.OwnerId);
            }
            var currentSelectedAreas = tank.Areas?.Select(a => a.Id).ToList() ?? new List<int>();
            ViewData["AvailableAreas"] = new SelectList(_context.Areas, "Id", "Name", currentSelectedAreas);

            return View(tank);
        }

        // GET: Tanks/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tank = await _context.Tanks
                .Include(t => t.Owner)
                .Include(t => t.Areas)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (tank == null)
            {
                return NotFound();
            }

            if (User.IsInRole("TankOwner") && !User.IsInRole("Admin"))
            {
                var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (tank.OwnerId != currentUserId)
                {
                    return Forbid();
                }
            }

            return View(tank);
        }

        // POST: Tanks/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var tank = await _context.Tanks
                .Include(t => t.Areas)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (tank == null)
            {
                return NotFound();
            }

            if (User.IsInRole("TankOwner") && !User.IsInRole("Admin"))
            {
                var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (tank.OwnerId != currentUserId)
                {
                    return Forbid();
                }
            }

            _context.Tanks.Remove(tank);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TankExists(int id)
        {
            return _context.Tanks.Any(e => e.Id == id);
        }
    }
}