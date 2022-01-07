using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MyAirbnb.Data;
using MyAirbnb.Models;
using MyAirbnb.Other;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace MyAirbnb.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext dbContext)
        {
            _context = dbContext;
        }

        public IActionResult Index(int? id)
        {
            var page = id != null ? id.Value : 1;
            int amountToSkip = amountToSkip = (page - 1) * App.ItemsPerPage;

            var posts = _context.Posts
                .Include(p => p.Comodities)
                .Include(p => p.PostImages)
                .Skip(amountToSkip)
                .Take(App.ItemsPerPage)
                .Where(p => !p.Hidden);

            var currentLastPostNumber = posts.Count() + amountToSkip;

            var model = new IndexModel
            {
                Posts = posts,
                CurrentPage = page,
                HasNextPage = _context.Posts.Count() > currentLastPostNumber,
                HasPreviousPage = amountToSkip > 0
            };

            return View(model);
        }

        public IActionResult Details(int? id)
        {
            if (id == null) return NotFound();
            var post = _context.Posts
                .Include(p => p.PostImages)
                .Include(p => p.Comodities)
                .Include(p => p.Comments).ThenInclude(p => p.User)
                .FirstOrDefault(p => p.Id == id && !p.Hidden);
            if (post == null) return NotFound();

            return View(post);
        }

        public async Task<IActionResult> FillAsync()
        {
            await TestData.FillTestDataAsync(_context);
            return RedirectToAction(nameof(Index));
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
