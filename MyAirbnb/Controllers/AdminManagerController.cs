using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MyAirbnb.Data;
using MyAirbnb.Models;
using MyAirbnb.Other;

namespace MyAirbnb.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminManagerController : Controller
    {
        private readonly ApplicationDbContext _context;

        private readonly UserManager<IdentityUser> _userManager;

        public AdminManagerController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: AdminManager
        public async Task<IActionResult> Index()
        {
            var result = await _userManager.GetUsersInRoleAsync(App.ManagerRole);
            List<ManageViewModel> managers = new();
            foreach (var manager in result)
            {
                managers.Add(new ManageViewModel() { Id = manager.Id, Name = manager.UserName });
            }
            return View(managers);
        }

        // GET: AdminManager/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var manager = await _context.Managers
                .FirstOrDefaultAsync(m => m.Id == id);
            if (manager == null)
            {
                return NotFound();
            }
            var name =  _context.Users
                .FirstOrDefaultAsync(m => m.Id == id).Result.UserName;
            var viewModel = new ManageViewModel() { Id = manager.Id, Workers = manager.Workers, Name = name };
            return View(viewModel);
        }

        // GET: AdminManager/Create
        //public IActionResult Create()
        //{
        //    return View();
        //}

        //// POST: AdminManager/Create
        //// To protect from overposting attacks, enable the specific properties you want to bind to.
        //// For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Create([Bind("Id")] Manager manager)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        _context.Add(manager);
        //        await _context.SaveChangesAsync();
        //        return RedirectToAction(nameof(Index));
        //    }
        //    return View(manager);
        //}

        // GET: AdminManager/Edit/5
        //public async Task<IActionResult> Edit(string id)
        //{
        //    if (id == null)
        //    {
        //        return NotFound();
        //    }

        //    var manager = await _context.Managers.FindAsync(id);
        //    if (manager == null)
        //    {
        //        return NotFound();
        //    }
        //    return View(manager);
        //}

        // POST: AdminManager/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Edit(string id, [Bind("Id")] Manager manager)
        //{
        //    if (id != manager.Id)
        //    {
        //        return NotFound();
        //    }

        //    if (ModelState.IsValid)
        //    {
        //        try
        //        {
        //            _context.Update(manager);
        //            await _context.SaveChangesAsync();
        //        }
        //        catch (DbUpdateConcurrencyException)
        //        {
        //            if (!ManagerExists(manager.Id))
        //            {
        //                return NotFound();
        //            }
        //            else
        //            {
        //                throw;
        //            }
        //        }
        //        return RedirectToAction(nameof(Index));
        //    }
        //    return View(manager);
        //}

        // GET: AdminManager/Delete/5
        //public async Task<IActionResult> Delete(string id)
        //{
        //    if (id == null)
        //    {
        //        return NotFound();
        //    }

        //    var manager = await _context.Managers
        //        .FirstOrDefaultAsync(m => m.Id == id);
        //    if (manager == null)
        //    {
        //        return NotFound();
        //    }

        //    return View(manager);
        //}

        // POST: AdminManager/Delete/5
        //[HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> DeleteConfirmed(string id)
        //{
        //    var manager = await _context.Managers.FindAsync(id);
        //    _context.Managers.Remove(manager);
        //    await _context.SaveChangesAsync();
        //    return RedirectToAction(nameof(Index));
        //}

        //private bool ManagerExists(string id)
        //{
        //    return _context.Managers.Any(e => e.Id == id);
        //}
    }
}
