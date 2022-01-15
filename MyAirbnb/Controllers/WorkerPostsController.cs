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

    [Authorize(Roles = "Worker, Manager, Admin")]
    public class WorkerPostsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public WorkerPostsController(ApplicationDbContext context, IWebHostEnvironment environment)
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

        private IQueryable<Post> WhereThisPost(int id)
        {
            //if (workerId == null)
            //    return _context.Posts.Where(e => e.Id == id && e.WorkerId == UserId);

            var query = _context.Posts.Include(e => e.Worker).Where(e => e.Id == id && (e.WorkerId == UserId || e.Worker.ManagerId == UserId));
            return query;
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

        public class IndexModel
        {
            public ApplicationUser User { get; set; }
            public string WorkerId { get; set; }
            public IEnumerable<Post> Posts { get; set; }
        }

        public IActionResult Index(string workerId)
        {
            if (workerId != null)
            {
                var worker = _context.Workers
                    .Include(e => e.Posts)
                    .Where(e => e.ManagerId == User.GetUserId() && e.Id == workerId)
                    .FirstOrDefault();
                if (worker == null) return NotFound();
                var model = new IndexModel
                {
                    User = _context.Users.FirstOrDefault(e => e.Id == workerId),
                    Posts = worker.Posts,
                    WorkerId = workerId,
                };
                return View(model);
            }
            else
            {
                var postsList = _context.Posts.Where(e => e.WorkerId == UserId);
                var model = new IndexModel
                {
                    User = _context.Users.FirstOrDefault(e => e.Id == workerId),
                    Posts = postsList
                };
                return View(model);
            }
        }
        //TODO fazer cancelamento talvez antes de ser aceite

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


        public IActionResult Edit(int? id)
        {
            if (!id.HasValue) return NotFound();
            var postId = id.Value;

            var post = WhereThisPost(postId)
                .Include(e => e.Comodities)
                .Include(e => e.PostImages)
                .FirstOrDefault();
            if (post == null) return NotFound();

            EditPost editPost = CreateEditPostObject(post);
            return View(editPost);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind(Prefix = "Post")] ReceivePost formPost)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var post = WhereThisPost(id)
                        .Include(e => e.Comodities)
                        .Include(e => e.PostImages)
                        .FirstOrDefault();
                    if (post == null) return NotFound();

                    List<Comodity> comodities = null;
                    if (formPost.Comodities != null)
                        comodities = _context.Comodities.Where(e => formPost.Comodities.Contains(e.Id)).ToList();

                    post.Title = formPost.Title;
                    post.Address = formPost.Address;
                    post.City = formPost.City;
                    post.Description = formPost.Description;
                    post.Price = formPost.Price;
                    post.NBeds = formPost.NBeds;
                    post.NBedrooms = formPost.NBedrooms;
                    post.SpaceCategoryId = formPost.SpaceCategoryId;
                    post.Comodities = comodities;
                    post.Hidden = false;

                    _context.Update(post);
                    await _context.SaveChangesAsync();

                    if (post.WorkerId != UserId)
                        return RedirectToAction(nameof(Index), new { workerId = post.WorkerId });
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PostExists(id)) return NotFound();
                    throw;
                }
            }
            return View(formPost);
        }

        public IActionResult Delete(int? id)
        {
            if (!id.HasValue) return NotFound();

            var post = WhereThisPost(id.Value).FirstOrDefault();
            if (post == null) return NotFound();

            return View(post);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var post = WhereThisPost(id)
                .Include(e => e.PostImages)
                .FirstOrDefault();
            foreach (var postImage in post.PostImages)
            {
                var fullPath = _environment.WebRootPath + postImage.FilePath;
                new FileInfo(fullPath).Delete();
            }
            _context.Posts.Remove(post);
            await _context.SaveChangesAsync();
            if (post.WorkerId != UserId)
                return RedirectToAction(nameof(Index), new { workerId = post.WorkerId });
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> RemovePostImage(int id, int fileId)
        {
            var postId = id;
            var post = WhereThisPost(postId)
                .Include(e => e.PostImages)
                .FirstOrDefault();
            if (post == null) return NotFound();

            var postImage = post.PostImages.FirstOrDefault(e => e.Id == fileId);
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
            var post = WhereThisPost(id)
               .Include(e => e.PostImages)
               .FirstOrDefault();
            if (post == null) return NotFound();

            var imagesPath = App.PostImagesFolderName;
            var newImages = new List<PostImage>();

            foreach (var formFile in files)
            {
                if (formFile.Length <= 0) continue;
                var filePath = "/" + imagesPath + $@"/{id}-{Path.GetRandomFileName()}.jpg";
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
