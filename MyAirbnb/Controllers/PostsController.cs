using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MyAirbnb.Data;
using MyAirbnb.Models;

namespace MyAirbnb.Controllers
{
    public class PostsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PostsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Posts
        public async Task<IActionResult> Index()
        {
            return View(await _context.Posts.ToListAsync());
        }

        // GET: Posts/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var post = await _context.Posts
                .FirstOrDefaultAsync(m => m.Id == id);
            if (post == null)
                return NotFound();

            return View(post);
        }

        // GET: Posts/Create
        public IActionResult Create()
        {
            var editPost = new EditPost()
            {
                Post = new Post(),
                Comodities = _context.Comodities.ToList()
            };
            return View(editPost);
        }

        // POST: Posts/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Title,Address,Description,Html,Price,NBeds,NBedrooms,PropertyType,AvailabilityType,Comodities")] ReceivePost post)
        {
            if (ModelState.IsValid)
            {

                List<Comodity> comodities = null;
                if (post.Comodities != null)
                    comodities = _context.Comodities.Where(c => post.Comodities.Contains(c.Id)).ToList();

                var finalPost = new Post
                {
                    Title = post.Title,
                    Address = post.Address,
                    Description = post.Description,
                    Price = post.Price,
                    NBeds = post.NBeds,
                    NBedrooms = post.NBedrooms,
                    PropertyType = post.PropertyType,
                    AvailabilityType = post.AvailabilityType,
                    Comodities = comodities,
                };

                _context.Add(finalPost);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(post);
        }

        // GET: Posts/Edit/5
        public IActionResult Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var post = _context.Posts
                .Include(p => p.Comodities)
                .Include(p => p.PostImages)
                .FirstOrDefault(p => p.Id == id.Value);
            if (post == null)
            {
                return NotFound();
            }

            var editPost = new EditPost()
            {
                Post = post,
                Comodities = _context.Comodities.ToList()
            };
            return View(editPost);
        }

        // POST: Posts/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Title,Address,Description,Html,Price,NBeds,NBedrooms,PropertyType,AvailabilityType,Comodities")] ReceivePost post)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var dbPost = _context.Posts
                        .Include(e => e.Comodities)
                        .Include(p => p.PostImages)
                        .FirstOrDefault(e => e.Id == id);
                    if (dbPost == null)
                        return NotFound();

                    List<Comodity> comodities = null;
                    if (post.Comodities != null)
                        comodities = _context.Comodities.Where(c => post.Comodities.Contains(c.Id)).ToList();

                    dbPost.Title = post.Title;
                    dbPost.Address = post.Address;
                    dbPost.Description = post.Description;
                    dbPost.Price = post.Price;
                    dbPost.NBeds = post.NBeds;
                    dbPost.NBedrooms = post.NBedrooms;
                    dbPost.PropertyType = post.PropertyType;
                    dbPost.AvailabilityType = post.AvailabilityType;
                    dbPost.Comodities = comodities;

                    _context.Update(dbPost);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PostExists(id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            return View(post);
        }

        // GET: Posts/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var post = await _context.Posts
                .FirstOrDefaultAsync(m => m.Id == id);
            if (post == null)
            {
                return NotFound();
            }

            return View(post);
        }

        // POST: Posts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var post = await _context.Posts.FindAsync(id);
            _context.Posts.Remove(post);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PostExists(int id)
        {
            return _context.Posts.Any(e => e.Id == id);
        }
    }
}
