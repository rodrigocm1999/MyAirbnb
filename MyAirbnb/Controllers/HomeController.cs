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

        public IActionResult Index()
        {
            var posts = _context.Posts.Include(p => p.Comodities).Take(App.ItemsPerPage);
            return View(posts);
        }

        public IActionResult Details(int? id)
        {
            if (id == null) return NotFound();
            var post = _context.Posts.FirstOrDefault(p => p.Id == id);
            if (post == null) return NotFound();
            return View(post);
        }

        public IActionResult Fill()
        {
            var context = _context;
            var comodities = new List<Comodity>
            {
                new Comodity{Name = "Wifi" },
                new Comodity{Name = "Dryer" },
                new Comodity{Name = "Dishwasher" },
                new Comodity{Name = "WashingMachine" },
                new Comodity{Name = "CoffeeMachine" },
                new Comodity{Name = "TV" },
                new Comodity{Name = "Fridge" },
                new Comodity{Name = "AirConditioning" },
                new Comodity{Name = "Bedsheets" },
                new Comodity{Name = "Vaccum" },
                new Comodity{Name = "Microwave" },
                new Comodity{Name = "Balcony" },
                new Comodity{Name = "Garage" },
            };
            context.Comodities.AddRange(comodities);

            var posts = new List<Post>
            {
                new Post{Address="Rua do aço",Availability=AvailabilityType.Available,Description="Este apartamente tem aço",Html="<h1>Conteudo teste</h1>",Title="Casa para 3 nabos",NBedrooms=1,NBeds=3,Price=400.99f,Rating=0,Type=PropertyType.Apartment},
                new Post{Address="Rua ao lado da rua do aço",Availability=AvailabilityType.AlreadyRented,Description="Esta casa não vale nada",Html="<h2>Conteudo teste numero 2</h2>",Title="Casa a cair aos bocados",NBedrooms=2,NBeds=1,Price=20.99f,Rating=3,Type=PropertyType.Home},
            };

            Random rand = new Random(4321);

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

            context.SaveChanges();
            return View();
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
