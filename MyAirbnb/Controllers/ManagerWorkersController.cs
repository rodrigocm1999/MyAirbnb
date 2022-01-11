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
    [Authorize(Roles = "Manager, Admin")]
    public class ManagerWorkersController : Controller
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<RegisterManagerModel> _logger;
        private readonly IEmailSender _emailSender;

        public ManagerWorkersController(
            UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, ILogger<RegisterManagerModel> logger,
            IEmailSender emailSender, RoleManager<IdentityRole> roleManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
            _roleManager = roleManager;
            _context = context;
        }

        private IQueryable<Manager> WhereManager()
        {
            return _context.Managers.Where(e => e.Id == User.GetUserId());
        }
        public IActionResult Index(string id)
        {
            Manager manager;
            if (id != null)
                manager = _context.Managers.Where(e => e.Id == id).Include(e => e.Workers).FirstOrDefault();
            else
                manager = WhereManager().Include(e => e.Workers).FirstOrDefault();


            var workerList = new List<WorkerViewModel>(manager.Workers.Count);
            var adminRoleId = _context.Roles.FirstOrDefault(x => x.Name == App.AdminRole).Id;
            foreach (var a in manager.Workers)
            {
                var user = _context.Users.FirstOrDefault(e => e.Id == a.Id);
                IdentityUserRole<string> userHasAdminRole = _context.UserRoles.FirstOrDefault(x => x.RoleId == adminRoleId && x.UserId == user.Id);
                if (userHasAdminRole == null)
                {
                    WorkerViewModel workerModel = new WorkerViewModel { Id = user.Id, Name = user.UserName, Posts = a.Posts };
                    workerList.Add(workerModel);
                }
            }

            return View(workerList);
        }

        public IActionResult Details(string id)
        {
            return RedirectToAction("Index", "WorkerPosts", new { workerId = id });
        }

        public IActionResult Create() { return View(); }
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
            [Display(Name = "First Name")]
            public string FirstName { get; set; }

            [Required]
            [Display(Name = "Last Name")]
            public string LastName { get; set; }


            [DataType(DataType.PhoneNumber)]
            [Display(Name = "Phone Number")]
            public string PhoneNumber { get; set; }
        }

        [HttpPost]
        public async Task<IActionResult> CreateAsync(WorkerModel Input)
        {
            var userName = Input.FirstName + Input.LastName;
            var user = new ApplicationUser { UserName = userName, FirstName = Input.FirstName, LastName = Input.LastName, Email = Input.Email, PhoneNumber = Input.PhoneNumber };
            var result = await _userManager.CreateAsync(user, Input.Password);
            if (result.Succeeded)
            {
                var roleExists = await _roleManager.RoleExistsAsync(App.WorkerRole);
                if (!roleExists)
                    await _roleManager.CreateAsync(new IdentityRole { Name = App.WorkerRole });

                await _userManager.AddToRoleAsync(user, App.WorkerRole);
                var manager = WhereManager().FirstOrDefault();

                _context.Workers.Add(new Worker { Id = user.Id, ManagerId = manager.Id });
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);
            //TODO verificar a cena de mandar os erros de volta
            return View();
        }

        public IActionResult Delete(string id)
        {
            var worker = _context.Users.FirstOrDefault(e => e.Id == id);
            var workerModel = new WorkerViewModel { Id = worker.Id, Name = worker.UserName };
            if (worker == null) return NotFound();

            return View(workerModel);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var worker = _context.Users.FirstOrDefault(e => e.Id == id);
            var post = _context.Posts.Where(e => e.WorkerId == worker.Id);

            var maganerId = WhereManager().FirstOrDefault().Id;
            await post.ForEachAsync(x =>
            {
                x.WorkerId = maganerId;
            });

            var reservations = _context.Reservations.Where(e => e.WorkerId == worker.Id);
            await reservations.ForEachAsync(x =>
            {
                x.WorkerId = maganerId;
            });
            _context.Remove(worker);

            //faltava remover o user
            var user = await _userManager.GetUserAsync(User);
            await _userManager.DeleteAsync(user);

            _context.SaveChanges();
            return RedirectToAction(nameof(Index));
        }

    }
}
