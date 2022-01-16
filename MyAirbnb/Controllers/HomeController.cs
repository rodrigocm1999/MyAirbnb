using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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
        private readonly UserManager<ApplicationUser> _userManager;
        public HomeController(UserManager<ApplicationUser> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }
        public IActionResult Index(int? id, [Bind(Prefix = "Search")] IndexSearch search)
        {
            var page = id.HasValue ? id.Value : 1;
            int amountToSkip = (page - 1) * App.ItemsPerPage;

            var posts = _context.Posts
                .Include(p => p.Comodities)
                .Include(p => p.PostImages)
                .Include(p => p.SpaceCategory)
                .Skip(amountToSkip)
                .Take(App.ItemsPerPage + 1)
                .Where(p => !p.Hidden);

            var spaceCategories = _context.SpaceCategories.ToList();
            //var empety = new SpaceCategory { Name = " " };
            //spaceCategories.Add(empety);
            var listSpaceCategories = new SelectList(spaceCategories, nameof(SpaceCategory.Id), nameof(SpaceCategory.Name));

            var ve = search.SpaceCategory;

            if (search.City != null)
                posts = posts.Where(p => p.City.Contains(search.City));
            if (search.Nbeds.HasValue)
                posts = posts.Where(p => p.NBeds == search.Nbeds.Value);
            if (search.NRooms.HasValue)
                posts = posts.Where(p => p.NBedrooms == search.NRooms.Value);
            if(search.SpaceCategory != null)
                if(search.SpaceCategory.Name != null)
                    posts = posts.Where(p => p.SpaceCategoryId == search.SpaceCategory.Id);

            var model = new IndexModel
            {
                CurrentPage = page,
                HasNextPage = posts.Count() > App.ItemsPerPage,
                HasPreviousPage = amountToSkip > 0,
                Posts = posts.Take(App.ItemsPerPage),
                Search = search,
                SpaceCategories = listSpaceCategories
            };
            return View(model);
        }

        public IActionResult Details(int? id)
        {
            if (id == null) return NotFound();
            var post = _context.Posts
                .Include(p => p.PostImages)
                .Include(p => p.Comodities)
                .Include(p => p.SpaceCategory)
                .Include(p => p.Comments).ThenInclude(p => p.User)
                .FirstOrDefault(p => p.Id == id && !p.Hidden);
            if (post == null) return NotFound();

            return View(post);
        }

        public async Task<IActionResult> FillAsync()
        {
            var filler = new TestData(_userManager, _context);
            await filler.FillTestDataAsync();
            return RedirectToAction(nameof(Index));
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
