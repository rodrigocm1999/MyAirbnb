using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MyAirbnb.Data;
using MyAirbnb.Other;
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

        private string _userId = null;
        private string UserId
        {
            get
            {
                if (_userId == null)
                    _userId = User.GetUserId();
                return _userId;
            }
        }

        private IQueryable<Worker> WhereThisWorker()
        {
            return _context.Workers.Where(e => e.Id == UserId);
        }

        private bool PostExists(int id)
        {
            return _context.Posts.Any(e => e.Id == id);
        }

        private EditPost CreateEditPostObject(Post post)
        {
            var spaceCategories = _context.SpaceCategories;
            var editPost = new EditPost()
            {
                Post = post,
                Comodities = _context.Comodities.ToList(),
                SpaceCategories = new SelectList(spaceCategories, nameof(SpaceCategory.Id), nameof(SpaceCategory.Name), post.SpaceCategoryId),
            };
            return editPost;
        }

        // GET: Posts
        public IActionResult Posts(string id)
        {
            if (id != null)
            {
                var workerId = id;
                var worker = _context.Workers
                    .Include(e => e.Posts)
                    .Where(e => e.ManagerId == User.GetUserId() && e.Id == workerId)
                    .FirstOrDefault();
                if (worker == null) return NotFound();
                return View(worker.Posts);
            }
            else
            {
                var postsList = _context.Posts.Where(e => e.WorkerId == UserId);
                return View(postsList);
            }
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var post = await _context.Posts
                .FirstOrDefaultAsync(e => e.Id == id && e.WorkerId == UserId);

            if (post == null) return NotFound();

            return View(post);
        }
        
        public IActionResult Create()
        {
            var post = new Post()
            {
                WorkerId = User.GetUserId(),
                Hidden = true,
            };
            _context.Posts.Add(post);
            _context.SaveChanges();
            return RedirectToAction(nameof(Edit), new { post.Id });
        }


        // POST: Posts/Create
        //NOT IN USE ANYMORE, Now post is created and inserted into database when entering the Create page ------------------------
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Create([Bind("Title,Address,Description,Html,Price,NBeds,NBedrooms,PropertyType,AvailabilityType,Comodities")] ReceivePost post)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        List<Comodity> comodities = null;
        //        if (post.Comodities != null)
        //            comodities = _context.Comodities.Where(c => post.Comodities.Contains(c.Id)).ToList();

        //        var finalPost = new Post
        //        {
        //            WorkerId = User.GetUserId(),
        //            Title = post.Title,
        //            Address = post.Address,
        //            Description = post.Description,
        //            Price = post.Price,
        //            NBeds = post.NBeds,
        //            NBedrooms = post.NBedrooms,
        //            AvailabilityType = post.AvailabilityType,
        //            Comodities = comodities,
        //        };

        //        _context.Add(finalPost);
        //        await _context.SaveChangesAsync();
        //        return RedirectToAction(nameof(Index));
        //    }
        //    return View(post);
        //}

        // GET: Posts/Edit/5
        public IActionResult Edit(int? id)
        {
            if (id == null) return NotFound();

            var post = _context.Posts
                .Include(e => e.Comodities)
                .Include(e => e.PostImages)
                .FirstOrDefault(e => e.Id == id.Value && e.WorkerId == UserId);

            if (post == null) return NotFound();

            EditPost editPost = CreateEditPostObject(post);
            return View(editPost);
        }

        // POST: Posts/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Title,Address,Description,Html,Price,NBeds,NBedrooms,PropertyType,AvailabilityType,Comodities,SpaceCategoryId")] ReceivePost post)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var userId = User.GetUserId();
                    var dbPost = _context.Posts
                        .Include(e => e.Comodities)
                        .Include(e => e.PostImages)
                        .FirstOrDefault(e => e.Id == id && e.WorkerId == UserId);
                    if (dbPost == null) return NotFound();

                    List<Comodity> comodities = null;
                    if (post.Comodities != null)
                        comodities = _context.Comodities.Where(e => post.Comodities.Contains(e.Id)).ToList();

                    dbPost.Title = post.Title;
                    dbPost.Address = post.Address;
                    dbPost.Description = post.Description;
                    dbPost.Price = post.Price;
                    dbPost.NBeds = post.NBeds;
                    dbPost.NBedrooms = post.NBedrooms;
                    dbPost.SpaceCategoryId = post.SpaceCategoryId;
                    dbPost.Comodities = comodities;
                    dbPost.Hidden = false;

                    _context.Update(dbPost);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PostExists(id)) return NotFound();
                    throw;
                }
            }
            return View(post);
        }

        // GET: Posts/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var post = await _context.Posts
                .FirstOrDefaultAsync(e => e.Id == id && e.WorkerId == UserId);
            if (post == null) return NotFound();

            return View(post);
        }

        // POST: Posts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var post = await _context.Posts
                .Include(e => e.PostImages)
                .FirstOrDefaultAsync(e => e.Id == id && e.WorkerId == UserId);
            foreach (var postImage in post.PostImages)
            {
                var fullPath = _environment.WebRootPath + postImage.FilePath;
                new FileInfo(fullPath).Delete();
            }
            _context.Posts.Remove(post);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> RemovePostImage(int postId, int postImageId)
        {
            var post = await _context.Posts
                .Include(e => e.PostImages)
                .FirstOrDefaultAsync(e => e.Id == postId && e.WorkerId == UserId);
            if (post == null) return NotFound();

            var postImage = post.PostImages.FirstOrDefault(e => e.Id == postImageId);
            if (postImage == null) return NotFound();

            post.PostImages.Remove(postImage);
            await _context.SaveChangesAsync();

            var fullPath = _environment.WebRootPath + postImage.FilePath;
            new FileInfo(fullPath).Delete();

            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> UploadPostImage(int id, IEnumerable<IFormFile> files)
        {
            var post = await _context.Posts
                .Include(e => e.PostImages)
                .FirstOrDefaultAsync(e => e.Id == id && e.WorkerId == UserId);
            if (post == null) return NotFound();

            var imagesPath = App.PostImagesFolderName;
            var newImages = new List<PostImage>();

            foreach (var formFile in files)
            {
                if (formFile.Length <= 0) continue;
                var filePath = "/" + imagesPath + $@"/{Path.GetRandomFileName()}.jpg";
                // .jpg so para mostrar no explorardor de ficheiros, não interessa se é jpg ou não

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
            return Ok(newImages);
        }



    }
}
