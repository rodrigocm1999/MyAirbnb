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
            return View();
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
    }
}
