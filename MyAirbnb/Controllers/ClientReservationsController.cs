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

        public class ReservationTableModel
        {

            public int Id { get; set; }
            public Post Post { get; set; }

            public bool CanComment { get; set; } = false;
            public Comment Comment { get; set; }

            public int? RatingUser { get; set; }
            public int? RatingPost { get; set; }
            public int TotalPrice { get; set; }

            public ReservationState State { get; set; }

            [DataType(DataType.Date)]
            public DateTime StartDate { get; set; }
            [DataType(DataType.Date)]
            public DateTime EndDate { get; set; }
        }

        public IActionResult Index()
        {
            var reservations = _context.Reservations
                .Include(e => e.Post)
                .Where(e => e.UserId == User.GetUserId());

            var model = new List<ReservationTableModel>();
            foreach (var r in reservations)
            {
                bool canComment = false;
                Comment postComment = null;
                if (r.State == ReservationState.Finished)
                {
                    postComment = _context.Comments.FirstOrDefault(e => e.ReservationId == r.Id);
                    if (postComment == null) canComment = true;
                }

                model.Add(new ReservationTableModel
                {
                    Id = r.Id,
                    Post = r.Post,
                    TotalPrice = r.TotalPrice,
                    RatingUser = r.RatingUser,
                    RatingPost = r.RatingPost,
                    StartDate = r.StartDate,
                    EndDate = r.EndDate,
                    State = r.State,
                    Comment = postComment,
                    CanComment = canComment,
                });
            }

            return View(model);
        }

        public IActionResult Details(int? id)
        {
            if (id == null) return NotFound();
            var postId = id.Value;

            var post = _context.Reservations.FirstOrDefault(e => e.Id == postId && e.WorkerId == User.GetUserId());
            if (post == null) return NotFound();
            
            //var model = new ReservationModel


            return View();
        }

        public class ReservationModel : IValidatableObject
        {
            public int PostId { get; set; }
            public int Price { get; set; }

            [Display(Name = "Start of Reservation")]
            [DataType(DataType.Date)]
            public DateTime StartDate { get; set; } = DateTime.Today;
            [DataType(DataType.Date)]
            [Display(Name = "End of Reservation")]
            public DateTime EndDate { get; set; } = DateTime.Today.AddDays(5);

            public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
            {
                int result = DateTime.Compare(StartDate, EndDate);
                if (result >= 0)
                    yield return new ValidationResult("End date must be after start date!");
            }
        }

        public IActionResult Create(int? id)
        {
            if (id == null) return NotFound();
            var postId = id.Value;

            var post = _context.Posts.FirstOrDefault(e => e.Id == postId);
            if (post == null) return NotFound();

            var model = new ReservationModel { PostId = postId, Price = post.Price };
            //TODO atualizar preço ao mudar as datas
            return View(model);
        }

        [HttpPost]
        public IActionResult Create(int? id, ReservationModel reservation)
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
