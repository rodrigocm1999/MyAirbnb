using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MyAirbnb.Data;
using MyAirbnb.Models;
using MyAirbnb.Other;

namespace MyAirbnb.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminClientsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
       

        public AdminClientsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: AdminClients
        public async Task<IActionResult> Index()
        {
            var result = await _userManager.GetUsersInRoleAsync(App.ClientRole);
            List<ClientsAdminView> clients = new ();
            foreach (var user in result)
                clients.Add(new ClientsAdminView() { Id = user.Id, Name = user.UserName });

            return View(clients);
        }

        // GET: AdminClients/Details/5
        public IActionResult Details(string id)
        {
            if (id == null)
                return NotFound();

            var user = _context.Users.FirstOrDefault(m => m.Id == id);
            IQueryable<Reservation> reservations = _context.Reservations.Where(m => m.UserId == id).Include(m => m.Post);
            var clientsAdminView = new ClientsAdminView() { Id = user.Id, Name = user.UserName , Reservations = reservations};
            var count = clientsAdminView.Reservations.Count();
            if (clientsAdminView == null)
                return NotFound();

            return View(clientsAdminView);
        }

        public IActionResult ClienteReservation(int? id)
        {
            if (id == null) return NotFound();
            var reservationId = id.Value;

            var reservation = _context.Reservations.Include(e => e.Post).Include(e => e.Comment).FirstOrDefault(e => e.Id == reservationId && e.WorkerId == User.GetUserId());
            if (reservation == null) return NotFound();

            //var reservations = _context.Reservations
            //    .Include(e => e.Post)
            //    .Include(e => e.Comment)
            //    .FirstOrDefault(e => e.Id == reservationId && e.UserId == User.GetUserId());

            var model = new ReservationModel
            {
                Id = reservation.Id,
                Post = reservation.Post,
                TotalPrice = reservation.TotalPrice,
                RatingUser = reservation.RatingUser,
                RatingPost = reservation.RatingPost,
                StartDate = reservation.StartDate,
                EndDate = reservation.EndDate,
                State = reservation.State,
                Comment = reservation.Comment,
            };

            return View(model);
        }

        // GET: AdminClients/Create
        //public IActionResult Create()
        //{
        //    return View();
        //}

        // POST: AdminClients/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Create([Bind("Id,Name")] ClientsAdminView clientsAdminView)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        _context.Add(clientsAdminView);
        //        await _context.SaveChangesAsync();
        //        return RedirectToAction(nameof(Index));
        //    }
        //    return View(clientsAdminView);
        //}

        //// GET: AdminClients/Edit/5
        //public async Task<IActionResult> Edit(string id)
        //{
        //    if (id == null)
        //    {
        //        return NotFound();
        //    }

        //    var clientsAdminView = clients.FirstOrDefault(m => m.Id == id);
        //    if (clientsAdminView == null)
        //    {
        //        return NotFound();
        //    }
        //    return View(clientsAdminView);
        //}

        //// POST: AdminClients/Edit/5
        //// To protect from overposting attacks, enable the specific properties you want to bind to.
        //// For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Edit(string id, [Bind("Id,Name")] ClientsAdminView clientsAdminView)
        //{
        //    if (id != clientsAdminView.Id)
        //    {
        //        return NotFound();
        //    }

        //    if (ModelState.IsValid)
        //    {
        //        try
        //        {
        //            //_context.Update(clientsAdminView);
        //            //await _context.SaveChangesAsync();
        //        }
        //        catch (DbUpdateConcurrencyException)
        //        {
        //            if (!ClientsAdminViewExists(clientsAdminView.Id))
        //            {
        //                return NotFound();
        //            }
        //            else
        //            {
        //                throw;
        //            }
        //        }
        //        return RedirectToAction(nameof(Index));
        //    }
        //    return View(clientsAdminView);
        //}

        // GET: AdminClients/Delete/5
        //public async Task<IActionResult> Delete(string id)
        //{
        //    if (id == null)
        //    {
        //        return NotFound();
        //    }

        //    var clientsAdminView = clients.FirstOrDefault(m => m.Id == id);
        //    if (clientsAdminView == null)
        //    {
        //        return NotFound();
        //    }

        //    return View(clientsAdminView);
        //}


    }
}
