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
using MyAirbnb.Models;
using MyAirbnb.Other;

namespace MyAirbnb.Controllers
{
    [Authorize(Roles = "Worker, Manager, Admin")]
    public class WorkerReservationsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public WorkerReservationsController(ApplicationDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }


        public Client GetClient(string id)
        {
            return _context.Clients.FirstOrDefault(e => e.Id == id);
        }

        public async Task<IActionResult> IndexAsync()
        {
            //ViewData["PostId"] = new SelectList(_context.Posts, "Id", "Address", reservation.PostId);
            //ViewData["WorkerId"] = new SelectList(_context.Workers, "Id", "Id", reservation.WorkerId);

            var reservations = _context.Reservations
                .Include(e => e.Post)
                .Where(e => e.WorkerId == User.GetUserId())
                .OrderBy(e => e.State);

            //(e.State != ReservationState.Finished || (DateTime.Now.Date - e.EndDate.Date).Days <= 7 )

            await reservations
                .Where(e => e.State == ReservationState.OnGoing && DateTime.Now >= e.EndDate)
                .ForEachAsync(e => e.State = ReservationState.ToCheckOut);
            await reservations
                .Where(e => e.State == ReservationState.Accepted && DateTime.Now >= e.StartDate)
                .ForEachAsync(e => e.State = ReservationState.ToCheckIn);

            await _context.SaveChangesAsync();
            return View(reservations);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var reservation = await _context.Reservations
                .Include(r => r.Post)
                .Include(r => r.Worker)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (reservation == null) return NotFound();

            return View(reservation);
        }

        public IActionResult Accept(int? id, bool? accepted)
        {
            if (!id.HasValue) return NotFound();
            var reservationId = id.Value;

            var reservation = _context.Reservations
                .Include(e => e.User)
                .Include(e => e.Post)
                .FirstOrDefault(e => e.Id == reservationId && e.WorkerId == User.GetUserId() && e.State == ReservationState.Pending);
            if (reservation == null) return NotFound();

            if (accepted.HasValue)
            {
                if (accepted.Value)
                    reservation.State = ReservationState.Accepted;
                else
                    reservation.State = ReservationState.Rejected;
                _context.SaveChanges();
                return RedirectToAction(nameof(Index));
            }

            //var userRatings = _context.Reservations
            //    .Where(e => e.UserId == reservation.UserId && e.RatingUser.HasValue);

            //float? userRating = null;
            //if (userRatings.Any())
            //    userRating = (float?)userRatings.Average(e => e.RatingUser.Value);
            var client = GetClient(reservation.UserId);

            var model = new AcceptReservationWorkerInputModel
            {
                ReservationId = reservation.Id,
                User = new SimpleUserModel
                {
                    UserId = reservation.UserId,
                    UserName = reservation.User.UserName,
                    Rating = client == null ? null : client.Rating,
                    PhoneNumber = reservation.User.PhoneNumber,
                }
            };

            //TODO deixar pretty, mostrar cenas do user
            return View(model);
        }

        public IActionResult CheckIn(int? id)
        {
            if (!id.HasValue) return NotFound();
            var reservationId = id.Value;

            var reservation = _context.Reservations
                .Include(e => e.Worker).ThenInclude(e => e.Manager).ThenInclude(e => e.CheckLists)
                .Include(e => e.Post)
                .Include(e => e.User)
                .FirstOrDefault(e => e.Id == reservationId && e.WorkerId == User.GetUserId() && e.State == ReservationState.ToCheckIn);
            if (reservation == null) return NotFound();

            var checkList = reservation.Worker.Manager.CheckLists.FirstOrDefault(e => e.SpaceCategoryId == reservation.Post.SpaceCategoryId);

            //var userRatings = _context.Reservations
            //   .Where(e => e.UserId == reservation.UserId && e.RatingUser.HasValue);

            //float? userRating = null;
            //if (userRatings.Any())
            //    userRating = (float?)userRatings.Average(e => e.RatingUser.Value);

            var client = GetClient(reservation.UserId);

            var model = new CheckInWorkerInputModel
            {
                ReservationId = reservation.Id,
                CheckItems = checkList == null ? new List<string>() : ChecklistsHelper.SplitItems(checkList.CheckInItems),
                User = new SimpleUserModel
                {
                    UserId = reservation.UserId,
                    UserName = reservation.User.UserName,
                    Rating = client == null ? null : client.Rating,
                    PhoneNumber = reservation.User.PhoneNumber,
                },
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CheckIn(int? id, CheckInWorkerOutputModel model)
        {
            if (!id.HasValue) return NotFound();
            var reservationId = id.Value;

            var reservation = _context.Reservations
                .FirstOrDefault(e => e.Id == reservationId && e.WorkerId == User.GetUserId() && e.State == ReservationState.ToCheckIn);
            if (reservation == null) return NotFound();

            reservation.CheckInItems = ChecklistsHelper.JoinForDatabase(model.CheckItems, model.ItemsIndeces);
            if (!string.IsNullOrEmpty(model.Notes))
                reservation.CheckInNotes = model.Notes;
            reservation.State = ReservationState.OnGoing;

            _context.SaveChanges();
            return RedirectToAction("Index");
        }

        public IActionResult CheckOut(int? id)
        {
            if (!id.HasValue) return NotFound();
            var reservationId = id.Value;

            var reservation = _context.Reservations
                .Include(e => e.Worker).ThenInclude(e => e.Manager).ThenInclude(e => e.CheckLists)
                .Include(e => e.Post)
                .Include(e => e.CheckOutImages)
                .Include(e => e.User)
                .FirstOrDefault(e => e.Id == reservationId && e.WorkerId == User.GetUserId() && e.State == ReservationState.ToCheckOut);
            if (reservation == null) return NotFound();

            var checkList = reservation.Worker.Manager.CheckLists.FirstOrDefault(e => e.SpaceCategoryId == reservation.Post.SpaceCategoryId);

            var client = GetClient(reservation.UserId);

            var model = new CheckOutWorkerInputModel
            {
                ReservationId = reservation.Id,
                CheckItems = checkList == null ? new List<string>() : ChecklistsHelper.SplitItems(checkList.CheckOutItems),
                Files = reservation.CheckOutImages,
                User = new SimpleUserModel
                {
                    UserId = reservation.UserId,
                    UserName = reservation.User.UserName,
                    Rating = client == null ? null : client.Rating,
                    PhoneNumber = reservation.User.PhoneNumber,
                },
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CheckOut(int? id, CheckOutWorkerOutputModel model)
        {
            if (!id.HasValue) return NotFound();
            var reservationId = id.Value;

            var reservation = _context.Reservations
                .FirstOrDefault(e => e.Id == reservationId && e.WorkerId == User.GetUserId() && e.State == ReservationState.ToCheckOut);
            if (reservation == null) return NotFound();

            reservation.CheckOutItems = ChecklistsHelper.JoinForDatabase(model.CheckItems, model.ItemsIndeces);
            if (!string.IsNullOrEmpty(model.Notes))
                reservation.CheckOutNotes = model.Notes;
            reservation.State = ReservationState.Finished;
            reservation.RatingUser = model.RatingUser;

            _context.SaveChanges();

            var userId = reservation.UserId;
            var average = _context.Reservations.Where(e => e.UserId == userId && e.RatingUser != null).Average(e => e.RatingUser);
            if (average.HasValue)
            {
                var client = _context.Clients.FirstOrDefault(e => e.Id == userId);
                if (client == null)
                {
                    client = new Client { Id = userId, Rating = (float)average };
                    _context.Clients.Add(client);
                }
                else
                    client.Rating = (float)average;
            }
            _context.SaveChanges();
            return RedirectToAction("Index");
        }



        [HttpPost]
        public async Task<IActionResult> RemoveCheckOutImage(int id, int fileId)
        {
            var reservationId = id;
            var reservation = await _context.Reservations
                .Include(e => e.CheckOutImages)
                .FirstOrDefaultAsync(e => e.Id == reservationId && e.WorkerId == User.GetUserId());
            if (reservation == null) return NotFound();

            var postImage = reservation.CheckOutImages.FirstOrDefault(e => e.Id == fileId);
            if (postImage == null) return NotFound();

            reservation.CheckOutImages.Remove(postImage);
            await _context.SaveChangesAsync();

            var fullPath = _environment.WebRootPath + postImage.FilePath;
            new FileInfo(fullPath).Delete();

            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> UploadCheckOutImage(int id, IEnumerable<IFormFile> files)
        {
            var reservation = await _context.Reservations
                .Include(e => e.CheckOutImages)
                .FirstOrDefaultAsync(e => e.Id == id && e.WorkerId == User.GetUserId());
            if (reservation == null) return NotFound();

            var imagesPath = App.ReservationImagesFolderName;
            var newImages = new List<CheckOutImage>();

            foreach (var formFile in files)
            {
                if (formFile.Length <= 0) continue;
                var filePath = "/" + imagesPath + $@"/{id}-{Path.GetRandomFileName()}.jpg";
                // .jpg so para mostrar no explorardor de ficheiros, não interessa se é jpg ou não

                using (var stream = System.IO.File.Create(_environment.WebRootPath + filePath))
                {
                    await formFile.CopyToAsync(stream);
                }
                var checkOutImage = new CheckOutImage
                {
                    FilePath = filePath
                };
                reservation.CheckOutImages.Add(checkOutImage);
                newImages.Add(checkOutImage);
            }

            await _context.SaveChangesAsync();
            return Ok(newImages);
        }


        public IActionResult DebugChangeEndDate(int id)
        {
            var reservation = _context.Reservations
                .FirstOrDefault(e => e.Id == id && e.WorkerId == User.GetUserId());
            reservation.EndDate = DateTime.Today;

            _context.SaveChanges();
            return RedirectToAction("Index");
        }

    }
}
