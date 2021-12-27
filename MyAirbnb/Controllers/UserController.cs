using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyAirbnb.Data;
using MyAirbnb.Other;
using MyAirbnb.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace MyAirbnb.Controllers
{
    [Authorize()] // makes a login required
    public class UserController : Controller
    {
        public readonly ApplicationDbContext _context;

        public UserController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }


        public class ReservationModel : IValidatableObject
        {
            public int PostId { get; set; }

            [Display(Name = "Start of Reservation")]
            [DataType(DataType.Date)]
            public DateTime StartDate { get; set; } = DateTime.Today;
            [DataType(DataType.Date)]
            [Display(Name = "End of Reservation")]
            public DateTime EndDate { get; set; } = DateTime.Today.AddDays(5);

            public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
            {
                int result = DateTime.Compare(StartDate, EndDate);
                if (result <= 0)
                    yield return new ValidationResult("start date must be less than the end date!");
            }
        }

        public IActionResult CreateReservation(int? id)
        {
            if (id == null) return NotFound();
            var postId = id.Value;

            var model = new ReservationModel { PostId = postId };

            return View(model);
        }

        [HttpPost]
        //[Route("CreateReservation")]
        public IActionResult SaveReservation(int? id, ReservationModel reservation)
        {
            if (id == null) return NotFound();
            var postId = id.Value;
            //TODO é preciso mostrar quais os dias disponíveis

            var rese = new Reservation
            {
                PostId = postId,
                StartDate = reservation.StartDate,
                EndDate = reservation.EndDate,
                UserId = User.GetUserId(),
            };

            _context.Reservations.Add(rese);
            _context.SaveChanges();
            return View();
        }
    }
}
