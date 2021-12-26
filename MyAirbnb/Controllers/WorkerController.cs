using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyAirbnb.Data;
using MyAirbnb.Extensions;
using MyAirbnb.Models;

namespace MyAirbnb.Controllers
{

    [Authorize(Roles = "Worker, Manager")]
    public class WorkerController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public WorkerController(ApplicationDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        // GET: Posts
        public async Task<IActionResult> Index()
        {
            var userId = User.GetUserId();
            var postsList = await _context.Posts.Where(e => e.WorkerId == userId).ToListAsync();
            return View(postsList);
        }

        // GET: Posts/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var userId = User.GetUserId();
            var post = await _context.Posts
                .FirstOrDefaultAsync(e => e.Id == id && e.WorkerId == userId);
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
                    WorkerId = User.GetUserId(),
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
                return NotFound();

            var userId = User.GetUserId();
            var post = _context.Posts
                .Include(e => e.Comodities)
                .Include(e => e.PostImages)
                .FirstOrDefault(e => e.Id == id.Value && e.WorkerId == userId);
            if (post == null)
                return NotFound();

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
                    var userId = User.GetUserId();
                    var dbPost = _context.Posts
                        .Include(e => e.Comodities)
                        .Include(e => e.PostImages)
                        .FirstOrDefault(e => e.Id == id && e.WorkerId == userId);
                    if (dbPost == null)
                        return NotFound();

                    List<Comodity> comodities = null;
                    if (post.Comodities != null)
                        comodities = _context.Comodities.Where(e => post.Comodities.Contains(e.Id)).ToList();

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
                        return NotFound();
                    else
                        throw;
                }
            }
            return View(post);
        }

        // GET: Posts/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var post = await _context.Posts
                .FirstOrDefaultAsync(e => e.Id == id);
            if (post == null)
                return NotFound();

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


        [HttpPost]
        public async Task<IActionResult> RemovePostImage(int postId, int postImageId)
        {
            var userId = User.GetUserId();
            var post = await _context.Posts
                .Include(e => e.PostImages)
                .FirstOrDefaultAsync(e => e.Id == postId && e.WorkerId == userId);
            if (post == null) return NotFound();

            var postImage = post.PostImages.FirstOrDefault(e => e.Id == postImageId);
            if (postImage == null) return NotFound();

            post.PostImages.Remove(postImage);
            await _context.SaveChangesAsync();
            //TODO talvez remover o ficheiro, talvez não, fazer tipo facebook e guardar tudo
            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> UploadPostImage(int id, IEnumerable<IFormFile> files)
        {
            var userId = User.GetUserId();
            var post = await _context.Posts
                .Include(e => e.PostImages)
                .FirstOrDefaultAsync(e => e.Id == id && e.WorkerId == userId);
            if (post == null) return NotFound();

            var imagesPath = App.PostImagesFolderName;

            var newImages = new List<PostImage>();

            foreach (var formFile in files)
            {
                if (formFile.Length <= 0) continue;

                var filePath = "/"+imagesPath + $@"/{Path.GetRandomFileName()}.jpg"; //so para mostrar no explorardor de ficheiros

                using (var stream = System.IO.File.Create(_environment.WebRootPath + filePath))
                {
                    await formFile.CopyToAsync(stream);
                }
                var postImage = new PostImage
                {
                    FilePath = filePath
                };
                post.PostImages.Add(postImage);
                newImages.Add(postImage);
            }

            await _context.SaveChangesAsync();

            //var toSendBack = new List<PostImage>();
            //foreach (var img in newImages)
            //{
            //    toSendBack.Add(new PostImage
            //    {
            //        Id = img.Id,
            //        FilePath = img.FilePath
            //    });
            //}

            //TODO talvez remover o ficheiro, talvez não, fazer tipo facebook e guardar 
            return Ok(newImages);
        }

    }
}
