﻿using System;
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
    [Authorize(Roles = "Worker")]
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

            var waiter = reservations
                .Where(e => e.State == ReservationState.OnGoing && DateTime.Now >= e.EndDate)
                .ForEachAsync(e => e.State = ReservationState.ToCheckOut);
            var waiter2 = reservations
                .Where(e => e.State == ReservationState.Accepted && DateTime.Now >= e.StartDate)
                .ForEachAsync(e => e.State = ReservationState.ToCheckIn);

            Task.WaitAll(waiter, waiter2);

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

            var model = new AcceptReservationWorkerInputModel
            {
                ReservationId = reservation.Id,
                UserId = reservation.UserId,
                UserName = reservation.User.UserName,
                UserRating = (float)userRatings.Average(e => e.RatingUser),
                PhoneNumber = reservation.User.PhoneNumber,
            };

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
                CheckItems = checkList == null ? new List<string>() : CheckListsHelper.SplitFromDatabase(checkList.CheckInItems),
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

            reservation.CheckInItems = CheckListsHelper.JoinForDatabase(model.CheckItems, model.ItemsIndeces);
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
                CheckItems = checkList == null ? new List<string>() : CheckListsHelper.SplitFromDatabase(checkList.CheckInItems),
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

            reservation.CheckOutItems = CheckListsHelper.JoinForDatabase(model.CheckItems, model.ItemsIndeces);
            reservation.State = ReservationState.Finished;
            reservation.RatingUser = model.RatingUser;

            _context.SaveChanges();
            return RedirectToAction("Index");
        }
    }
}
