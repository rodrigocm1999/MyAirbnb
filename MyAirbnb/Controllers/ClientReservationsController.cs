using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyAirbnb.Data;
using MyAirbnb.Models;
using MyAirbnb.Other;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace MyAirbnb.Controllers
{
    [Authorize(Roles = "Client, Worker, Manager, Admin")]
    public class ClientReservationsController : Controller
    {
        public readonly ApplicationDbContext _context;

        public ClientReservationsController(ApplicationDbContext context)
        {
            _context = context;
        }


        public IActionResult Index()
        {
            var reservations = _context.Reservations
                .Include(e => e.Post)
                .Include(e => e.Comment)
                .Where(e => e.UserId == User.GetUserId());

            var model = new List<ReservationModel>();
            foreach (var r in reservations)
            {
                bool canComment = r.State == ReservationState.Finished && r.RatingPost == null /*&& r.Comment == null*/;

                model.Add(new ReservationModel
                {
                    Id = r.Id,
                    Post = r.Post,
                    TotalPrice = r.TotalPrice,
                    RatingUser = r.RatingUser,
                    RatingPost = r.RatingPost,
                    StartDate = r.StartDate,
                    EndDate = r.EndDate,
                    State = r.State,
                    Comment = r.Comment,
                    CanComment = canComment,
                });
            }

            return View(model);
        }

        public IActionResult Details(int? id)
        {
            if (id == null) return NotFound();
            var reservationId = id.Value;

            var reservation = _context.Reservations
                .Include(e => e.Post)
                .Include(e => e.Comment)
                .Include(e => e.User)
                .Include(e => e.Worker)
                .Include(e => e.UserWorker)
                .FirstOrDefault(e => e.Id == reservationId && e.UserId == User.GetUserId());

            if (reservation == null) return NotFound();

            reservation.CheckInItems = null;
            reservation.CheckOutItems = null;
            reservation.CheckInNotes = null;
            reservation.CheckOutNotes = null;
            reservation.CheckOutImages = null;

            return View(reservation);
        }

        public IActionResult Comment(int? id)
        {
            if (id == null) return NotFound();
            var reservationId = id.Value;

            var reservation = _context.Reservations
                .Include(e => e.Post)
                .FirstOrDefault(e => e.Id == reservationId && e.UserId == User.GetUserId()
                    && e.State == ReservationState.Finished && e.RatingPost == null);
            if (reservation == null) return NotFound();

            var model = new ReservationCommentModel
            {
                Id = reservation.Id,
                Post = reservation.Post,
                TotalPrice = reservation.TotalPrice,
            };

            return View(model);
        }

        [HttpPost]
        public IActionResult Comment(int? id, ReservationCommentModel model)
        {
            if (!ModelState.IsValid) return View(model);

            if (id == null) return NotFound();
            var reservationId = id.Value;

            var reservation = _context.Reservations
                .Include(e => e.Post)
                .FirstOrDefault(e => e.Id == reservationId && e.UserId == User.GetUserId()
                    && e.State == ReservationState.Finished && e.RatingPost == null);
            if (reservation == null) return NotFound();

            reservation.RatingPost = model.RatingPost;
            if (!string.IsNullOrWhiteSpace(model.Comment))
            {
                reservation.Comment = new Comment()
                {
                    UserId = User.GetUserId(),
                    PostId = reservation.PostId,
                    Text = model.Comment,
                };
            }
            _context.SaveChanges();

            var avg = _context.Reservations
                .Where(e => e.PostId == reservation.PostId && e.State == ReservationState.Finished && e.RatingPost != null)
                .Average(e => e.RatingPost);

            if (avg.HasValue)
                reservation.Post.Rating = (float)avg.Value;

            _context.SaveChanges();
            return RedirectToAction("Index");
        }

        public IActionResult Create(int? id)
        {
            if (id == null) return NotFound();
            var postId = id.Value;

            var post = _context.Posts.FirstOrDefault(e => e.Id == postId);
            if (post == null) return NotFound();

            var model = new ReservationCreationModel { PostId = postId, Price = post.Price };
            //TODO atualizar preço ao mudar as datas
            return View(model);
        }

        [HttpPost]
        public IActionResult Create(int? id, ReservationCreationModel reservation)
        {
            if (!ModelState.IsValid)
                return View(reservation);

            if (id == null) return NotFound();
            var postId = id.Value;

            var post = _context.Posts.FirstOrDefault(e => e.Id == postId);
            if (post == null) return NotFound();

            var numberOfDays = (reservation.EndDate.Date - reservation.StartDate.Date).Days;

            var rese = new Reservation
            {
                PostId = postId,
                StartDate = reservation.StartDate,
                EndDate = reservation.EndDate,
                State = ReservationState.Pending,
                UserId = User.GetUserId(),
                WorkerId = post.WorkerId,
                Price = post.Price,
                TotalPrice = post.Price * numberOfDays,
            };

            _context.Reservations.Add(rese);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }
    }
}
