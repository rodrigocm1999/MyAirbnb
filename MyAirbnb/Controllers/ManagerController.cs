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
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            ILogger<RegisterManagerModel> logger,
            IEmailSender emailSender,
            RoleManager<IdentityRole> roleManager,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
            _roleManager = roleManager;
            _context = context;
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
        public class InputModel
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
        public async Task<IActionResult> CreateWorkerAccountAsync(InputModel Input)
        {
           
            var user = new IdentityUser { UserName = Input.Name, Email = Input.Email, PhoneNumber = Input.PhoneNumber };
            var result = await _userManager.CreateAsync(user, Input.Password);
            if (result.Succeeded)
            {
                _logger.LogInformation("Manager created a new account with password.");

                await PrepareRoleAsync();
                await _userManager.AddToRoleAsync(user, App.ManagerRole);

                _context.Workers.Add(new Worker { Id = user.Id });
                await _context.SaveChangesAsync();
                await _signInManager.SignInAsync(user, isPersistent: false);
                return RedirectToAction(nameof(Index));

            }
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View();

        }

        private async Task PrepareRoleAsync()
        {
            var exists = await _roleManager.RoleExistsAsync(App.WorkerRole);
            if (!exists)
                await _roleManager.CreateAsync(new IdentityRole { Name = App.WorkerRole });
        }


        public IActionResult ManageWorkerAccounts()
        {
            var manager = _context.Managers
                .Include(e => e.Workers)
                .FirstOrDefault(e => e.Id == User.GetUserId());
            //TODO
            return View(manager.Workers);
        }
    }
}
