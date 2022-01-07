using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyAirbnb.Data;
using MyAirbnb.Models;
using MyAirbnb.Other;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyAirbnb.Controllers
{
    [Authorize(Roles = "Manager, Admin")]
    public class ManagerChecklistsController : Controller
    { 
        private readonly ApplicationDbContext _context;

        public ManagerChecklistsController(ApplicationDbContext context)
        {
            _context = context;
        }

        private IQueryable<Manager> WhereManager()
        {
            return _context.Managers.Where(e => e.Id == User.GetUserId());
        }

        public IActionResult Index(string id)
        {
            var spaceCategories = _context.SpaceCategories;
            Manager manager;
            if (id != null)
                manager = _context.Managers.Where(e => e.Id == id).Include(e => e.CheckLists).FirstOrDefault();
            else
                manager = WhereManager().Include(e => e.CheckLists).FirstOrDefault();

            var categories = new List<SpaceCategoriesManagerList>(spaceCategories.Count());
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
                    scml.CheckInItems = checklists.CheckInItems != null ? checklists.CheckInItems.Replace("\n", " ; ") : "";
                    scml.CheckOutItems = checklists.CheckOutItems != null ? checklists.CheckOutItems.Replace("\n", " ; ") : "";
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

        public IActionResult Edit(int? id)
        {
            if (!id.HasValue) return NotFound();
            var spaceCategoryId = id.Value;
            var manager = WhereManager().Include(e => e.CheckLists).FirstOrDefault();

            var spaceCategory = _context.SpaceCategories.FirstOrDefault(e => e.Id == spaceCategoryId);
            if (spaceCategory == null) return NotFound();

            var checklist = manager.CheckLists.FirstOrDefault(e => e.SpaceCategoryId == spaceCategoryId);
            var editCheckLists = new EditCheckLists { SpaceCategoryId = spaceCategoryId, SpaceCategoryName = spaceCategory.Name };

            if (checklist != null)
            {
                editCheckLists.CheckInItems = checklist.CheckInItems;
                editCheckLists.CheckOutItems = checklist.CheckOutItems;
            }
            return View(editCheckLists);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("SpaceCategoryId,CheckInItems,CheckOutItems")] EditCheckLists editCheckLists)
        {
            var spaceCategoryId = id;
            var manager = WhereManager().Include(e => e.CheckLists).FirstOrDefault();

            var checklist = manager.CheckLists.FirstOrDefault(e => e.SpaceCategoryId == spaceCategoryId);
            if (checklist == null)
            {
                checklist = new CheckList { SpaceCategoryId = spaceCategoryId };
                manager.CheckLists.Add(checklist);
            }

            checklist.CheckInItems = ChecklistsHelper.JoinToStr(ChecklistsHelper.Trim(ChecklistsHelper.SplitItems(editCheckLists.CheckInItems)));
            checklist.CheckOutItems = ChecklistsHelper.JoinToStr(ChecklistsHelper.Trim(ChecklistsHelper.SplitItems(editCheckLists.CheckOutItems)));

            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        public IActionResult Delete(int? id)
        {
            if (!id.HasValue) return NotFound();
            var spaceCategoryId = id.Value;
            var manager = WhereManager().Include(e => e.CheckLists).FirstOrDefault();

            var e = manager.CheckLists.FirstOrDefault(e => e.SpaceCategoryId == spaceCategoryId);
            if (e != null)
            {
                manager.CheckLists.Remove(e);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }
            return NotFound();
        }

        public class EditCheckLists
        {
            [HiddenInput]
            public int SpaceCategoryId { get; set; }

            public string SpaceCategoryName { get; set; }
            public string CheckInItems { get; set; } = "";
            public string CheckOutItems { get; set; } = "";
        }
    }
}
