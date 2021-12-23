using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MyAirbnb.Data;
using MyAirbnb.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace MyAirbnb.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext dbContext)
        {
            _logger = logger;
            _context = dbContext;
        }

        public IActionResult Index(int? id)
        {
            var page = id;
            int amountToSkip = 0;
            if (page != null)
                amountToSkip = (page.Value - 1) * App.ItemsPerPage;

            var posts = _context.Posts
                .Include(p => p.Comodities)
                .Include(p => p.PostImages)
                .Skip(amountToSkip)
                .Take(App.ItemsPerPage);
            return View(posts);
        }

        public IActionResult Details(int? id)
        {
            if (id == null) return NotFound();
            var post = _context.Posts
                .Include(p => p.Comodities)
                .Include(p => p.PostImages)
                .FirstOrDefault(p => p.Id == id);
            if (post == null) return NotFound();
            return View(post);
        }

        public async Task<IActionResult> FillAsync()
        {
            var context = _context;
            var comodities = new List<Comodity>(){
                new Comodity{Name = "Wifi" },
                new Comodity{Name = "Dryer" },
                new Comodity{Name = "Dishwasher" },
                new Comodity{Name = "Washing Machine" },
                new Comodity{Name = "Coffee Machine" },
                new Comodity{Name = "TV" },
                new Comodity{Name = "Fridge" },
                new Comodity{Name = "Air Conditioning" },
                new Comodity{Name = "Bedsheets" },
                new Comodity{Name = "Vaccum" },
                new Comodity{Name = "Microwave" },
                new Comodity{Name = "Balcony" },
                new Comodity{Name = "Garage" },
            };

            if (_context.Posts.FirstOrDefault() == null)
            {
                context.Comodities.AddRange(comodities);
            }

            var posts = new List<Post>
            {
                new Post{Address="Rua do aço",Availability=AvailabilityType.Available,Description="Este apartamente tem aço",Title="Casa para 3 nabos",NBedrooms=1,NBeds=3,Price=400.99f,Rating=0,Type=PropertyType.Apartment},
                new Post{Address="Rua ao lado da rua do aço",Availability=AvailabilityType.AlreadyRented,Description="Esta casa não vale nada",Title="Casa a cair aos bocados",NBedrooms=2,NBeds=1,Price=20.99f,Rating=3,Type=PropertyType.Home},
            };

            Random rand = new Random(4321);


            if (_context.Posts.FirstOrDefault() == null)
            {
                foreach (var p in posts)
                {
                    p.Comodities = new List<Comodity>();
                    var amount = rand.Next(10);
                    for (int i = 0; i < amount; i++)
                    {
                        var randIndex = rand.Next(comodities.Count);
                        p.Comodities.Add(comodities[randIndex]);
                    }
                }

                context.Posts.AddRange(posts);
            }
            await context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
