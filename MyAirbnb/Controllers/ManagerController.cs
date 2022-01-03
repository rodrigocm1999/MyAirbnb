using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyAirbnb.Data;
using MyAirbnb.Other;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyAirbnb.Controllers
{
    [Authorize(Roles = "Manager")]
    public class ManagerController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ManagerController(ApplicationDbContext dbContext)
        {
            _context = dbContext;
        }

        public IActionResult Index()
        {
            //TODO
            var workers = _context.Workers;
            return View(workers);
        }


        public IActionResult CreateWorkerAccount()
        {
            //TODO
            return View();
        }


        public IActionResult ManageWorkerAccounts()
        {
            var manager = _context.Managers
                .Include(e => e.Workers)
                .FirstOrDefault(e => e.Id == User.GetUserId());
            //TODO
            return View(manager.Workers);
        }

        public IActionResult Checklist()
        {
            var spaceCategories = _context.SpaceCategories;
            var manager = _context.Managers
                .Include(e => e.CheckLists)
                .FirstOrDefault(e => e.Id == User.GetUserId());
            if (manager == null) return NotFound();

            List<SpaceCategoriesManagerList> categories = new List<SpaceCategoriesManagerList>(spaceCategories.Count());
            foreach (var cat in spaceCategories)
            {
                var checklists = manager.CheckLists.FirstOrDefault(e => e.SpaceCategoryId == cat.Id);
                var scml = new SpaceCategoriesManagerList
                {
                    Id = cat.Id,
                    Name = cat.Name,
                    HasDefinedItems = checklists != null,
                };
                if (checklists != null)
                {
                    scml.CheckInItems = checklists.CheckInItems;
                    scml.CheckOutItems = checklists.CheckOutItems;
                }
                categories.Add(scml);
            }

            return View(categories);
        }

        public class SpaceCategoriesManagerList
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public bool HasDefinedItems { get; set; }

            public string CheckInItems { get; set; }
            public string CheckOutItems { get; set; }
        }

        public IActionResult EditCheckList(int? id)
        {
            if (!id.HasValue) return NotFound();
            var spaceCategoryId = id.Value;

            var manager = _context.Managers
                .Include(e => e.CheckLists)
                .FirstOrDefault(e => e.Id == User.GetUserId());

            var checklist = manager.CheckLists.FirstOrDefault(e => e.SpaceCategoryId == spaceCategoryId);

            var editCheckLists = new EditCheckLists { SpaceCategoryId = spaceCategoryId };

            if (checklist != null)
            {
                editCheckLists.CheckInItems = checklist.CheckInItems.Replace('\n', ';');
                editCheckLists.CheckOutItems = checklist.CheckOutItems.Replace('\n', ';');
            }
            //TODO
            return View(editCheckLists);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditCheckList(int id, [Bind("SpaceCategoryId,CheckInItems,CheckOutItems")] EditCheckLists editCheckLists)
        {
            var spaceCategoryId = id;

            var manager = _context.Managers
                .Include(e => e.CheckLists)
                .FirstOrDefault(e => e.Id == User.GetUserId());

            var checklist = manager.CheckLists.FirstOrDefault(e => e.SpaceCategoryId == spaceCategoryId);

            if (checklist == null)
            {
                checklist = new Models.CheckList { SpaceCategoryId = spaceCategoryId };
                manager.CheckLists.Add(checklist);
            }
            checklist.CheckInItems = editCheckLists.CheckInItems;
            checklist.CheckOutItems = editCheckLists.CheckOutItems;

            await _context.SaveChangesAsync();
            //TODO
            return RedirectToAction(nameof(Checklist));
        }

        public async Task<IActionResult> DeleteCheckList(int? id)
        {
            if (!id.HasValue) return NotFound();
            var spaceCategoryId = id.Value;
            var manager = _context.Managers
                .Include(e => e.CheckLists)
                .FirstOrDefault(e => e.Id == User.GetUserId());

            var e = manager.CheckLists.FirstOrDefault(e => e.SpaceCategoryId == spaceCategoryId);
            if (e != null)
            {
                manager.CheckLists.Remove(e);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Checklist));
        }

        public class EditCheckLists
        {
            [HiddenInput]
            public int SpaceCategoryId { get; set; }
            public string CheckInItems { get; set; } = "";
            public string CheckOutItems { get; set; } = "";
        }
    }
}
