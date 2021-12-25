﻿using Microsoft.AspNetCore.Mvc;
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
                .Take(App.ItemsPerPage);

            var currentLastPostNumber = posts.Count() + amountToSkip;

            return View(new IndexModel
            {
                Posts = posts,
                CurrentPage = page,
                HasNextPage = _context.Posts.Count() > currentLastPostNumber,
                HasPreviousPage = amountToSkip > 0
            });
        }

        public IActionResult Details(int? id)
        {
            if (id == null) return NotFound();
            var post = _context.Posts
                .Include(p => p.PostImages)
                .Include(p => p.Comodities)
                .Include(p => p.Comments)
                .FirstOrDefault(p => p.Id == id);
            if (post == null) return NotFound();
            return View(post);
        }

        public async Task<IActionResult> FillAsync()
        {
            await TestData.FillTestDataAsync(_context);
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
