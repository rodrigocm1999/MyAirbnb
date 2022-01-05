using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MyAirbnb.Areas.Identity.Pages.Account;
using MyAirbnb.Data;
using MyAirbnb.Models;
using MyAirbnb.Other;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace MyAirbnb.Controllers
{
    [Authorize(Roles = "Manager")]
    public class ManagerController : Controller
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<RegisterManagerModel> _logger;
        private readonly IEmailSender _emailSender;

        public ManagerController(
            UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager, ILogger<RegisterManagerModel> logger,
            IEmailSender emailSender, RoleManager<IdentityRole> roleManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
            _roleManager = roleManager;
            _context = context;
        }

        private Manager GetThisManager()
        {
            var currentUserId = User.GetUserId();
            return _context.Managers
                    .Include(e => e.Workers)
                    .FirstOrDefault(e => e.Id == User.GetUserId());
        }


        public IActionResult Index()
        {
            var manager = GetThisManager();
            //TODO
            var workerList = new List<WorkerViewModel>(manager.Workers.Count);
            foreach(var a in manager.Workers)
            {
                var user = _context.Users.FirstOrDefault(e => e.Id == a.Id);
                WorkerViewModel workerModel = new WorkerViewModel { Id = user.Id,  Name = user.UserName , Posts = a.Posts};
                workerList.Add(workerModel);
            }

            return View(workerList);
        }



        public IActionResult CreateWorkerAccount() { return View(); }
        public class WorkerModel
        {
            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; }

            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }

            [Required]
            public string Name { get; set; }


            [DataType(DataType.PhoneNumber)]
            [Display(Name = "Phone Number")]
            public string PhoneNumber { get; set; }
        }

        [HttpPost]
        public async Task<IActionResult> CreateWorkerAccountAsync(WorkerModel Input)
        {
            var user = new IdentityUser { UserName = Input.Name, Email = Input.Email, PhoneNumber = Input.PhoneNumber };
            var result = await _userManager.CreateAsync(user, Input.Password);
            if (result.Succeeded)
            {
                var roleExists = await _roleManager.RoleExistsAsync(App.WorkerRole);
                if (!roleExists)
                    await _roleManager.CreateAsync(new IdentityRole { Name = App.WorkerRole });

                await _userManager.AddToRoleAsync(user, App.WorkerRole);
                var manager = GetThisManager();

                _context.Workers.Add(new Worker { Id = user.Id , Manager = manager, ManagerId = manager.Id });
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);
            //TODO verificar a cena de mandar os erros de volta
            return View();
        }

        public IActionResult ManageWorkerAccounts()
        {
            var manager = GetThisManager();
            //TODO
            return View(manager.Workers);
        }

        public IActionResult Checklist()
        {
            var spaceCategories = _context.SpaceCategories;
            var manager = GetThisManager();

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
            var manager = GetThisManager();

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
            var manager = GetThisManager();

            var checklist = manager.CheckLists.FirstOrDefault(e => e.SpaceCategoryId == spaceCategoryId);
            if (checklist == null)
            {
                checklist = new CheckList { SpaceCategoryId = spaceCategoryId };
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
            var manager = GetThisManager();

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
