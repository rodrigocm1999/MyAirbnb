using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
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

        public WorkerReservationsController(ApplicationDbContext context)
        {
            _context = context;
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

            var userRatings = _context.Reservations
                .Where(e => e.UserId == reservation.UserId && e.RatingUser.HasValue);

            float? userRating = null;
            if (userRatings.Any())
                userRating = (float?) userRatings.Average(e => e.RatingUser.Value);

            var model = new AcceptReservationWorkerInputModel
            {
                ReservationId = reservation.Id,
                UserId = reservation.UserId,
                UserName = reservation.User.UserName,
                UserRating = userRating,
                PhoneNumber = reservation.User.PhoneNumber,
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
                .FirstOrDefault(e => e.Id == reservationId && e.WorkerId == User.GetUserId() && e.State == ReservationState.ToCheckIn);
            if (reservation == null) return NotFound();

            var checkList = reservation.Worker.Manager.CheckLists.FirstOrDefault(e => e.SpaceCategoryId == reservation.Post.SpaceCategoryId);

            var model = new CheckInWorkerInputModel
            {
                ReservationId = reservation.Id,
                CheckItems = checkList == null ? new List<string>() : ChecklistsHelper.SplitItems(checkList.CheckInItems),
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
                .FirstOrDefault(e => e.Id == reservationId && e.WorkerId == User.GetUserId() && e.State == ReservationState.ToCheckOut);
            if (reservation == null) return NotFound();

            var checkList = reservation.Worker.Manager.CheckLists.FirstOrDefault(e => e.SpaceCategoryId == reservation.Post.SpaceCategoryId);

            var model = new CheckOutWorkerInputModel
            {
                ReservationId = reservation.Id,
                CheckItems = checkList == null ? new List<string>() : ChecklistsHelper.SplitItems(checkList.CheckOutItems),
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
            reservation.State = ReservationState.Finished;
            reservation.RatingUser = model.RatingUser;

            //TODO calcular a media do user e guardar em alggum lado, talvez criar uma DdSet<Client>

            _context.SaveChanges();
            return RedirectToAction("Index");
        }
    }
}
