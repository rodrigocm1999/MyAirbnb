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
    public class AdminManagersController : Controller
    {
        private readonly ApplicationDbContext _context;

        private readonly UserManager<ApplicationUser> _userManager;

        public AdminManagersController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: AdminManager
        public async Task<IActionResult> Index()
        {
            var result = await _userManager.GetUsersInRoleAsync(App.ManagerRole);
            List<ManagerViewModel> managers = new();
            foreach (var manager in result)
            {
                managers.Add(new ManagerViewModel() { Manager = manager });
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
            var user =  _context.Users.FirstOrDefault(m => m.Id == manager.Id);
            var viewModel = new ManagerViewModel() { Manager = user };
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
        public async Task<IActionResult> Delete(string id)
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
            var user = _context.Users.FirstOrDefault(m => m.Id == manager.Id);
            var viewModel = new ManagerViewModel() { Manager = user };

            return View(viewModel);
        }

        // POST: AdminManager/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var manager = await _context.Managers.FindAsync(id);
            var userManager = await _context.Users.FindAsync(manager.Id);
            foreach (var worker in manager.Workers)
            {
                //foreach (var post in worker.Posts)
                //{
                //    await _context.Posts.FindAsync(post.Id);
                //}
                Worker workerToRemove = await _context.Workers.FindAsync(worker.Id);
                var userWorkerToRemove = await _context.Users.FindAsync(worker.Id);
                _context.Users.Remove(userWorkerToRemove);
                _context.Workers.Remove(workerToRemove);
                await _context.SaveChangesAsync();
            }
            _context.Users.Remove(userManager);
            _context.Managers.Remove(manager);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        //private bool ManagerExists(string id)
        //{
        //    return _context.Managers.Any(e => e.Id == id);
        //}
    }
}
