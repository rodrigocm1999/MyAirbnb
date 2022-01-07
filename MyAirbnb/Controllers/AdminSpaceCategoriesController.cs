using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MyAirbnb.Data;
using MyAirbnb.Models;

namespace MyAirbnb.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminSpaceCategoriesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminSpaceCategoriesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: AdminSpaceCategories
        public async Task<IActionResult> Index()
        {
            return View(await _context.SpaceCategories.ToListAsync());
        }

        // GET: AdminSpaceCategories/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var spaceCategory = await _context.SpaceCategories
                .FirstOrDefaultAsync(m => m.Id == id);

            if (spaceCategory == null) return NotFound();

            return View(spaceCategory);
        }

        // GET: AdminSpaceCategories/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: AdminSpaceCategories/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name")] SpaceCategory spaceCategory)
        {
            if (ModelState.IsValid)
            {
                _context.Add(spaceCategory);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(spaceCategory);
        }

        // GET: AdminSpaceCategories/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var spaceCategory = await _context.SpaceCategories.FindAsync(id);
            if (spaceCategory == null) return NotFound();

            return View(spaceCategory);
        }

        // POST: AdminSpaceCategories/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name")] SpaceCategory spaceCategory)
        {
            if (id != spaceCategory.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(spaceCategory);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SpaceCategoryExists(spaceCategory.Id))
                        return NotFound();
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(spaceCategory);
        }

        // GET: AdminSpaceCategories/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var spaceCategory = await _context.SpaceCategories
                .FirstOrDefaultAsync(m => m.Id == id);
            if (spaceCategory == null) return NotFound();

            return View(spaceCategory);
        }

        // POST: AdminSpaceCategories/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var spaceCategory = await _context.SpaceCategories.FindAsync(id);
            _context.SpaceCategories.Remove(spaceCategory);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool SpaceCategoryExists(int id)
        {
            return _context.SpaceCategories.Any(e => e.Id == id);
        }
    }
}
