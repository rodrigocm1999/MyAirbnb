﻿using System;
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
        private readonly UserManager<IdentityUser> _userManager;
       

        public AdminClientsController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
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
            {
                clients.Add(new ClientsAdminView() { Id = user.Id, Name = user.UserName });
            }
            return View(clients);
        }

        // GET: AdminClients/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
                return NotFound();

            var user =  _context.Users.FirstOrDefault(m => m.Id == id);
            var clientsAdminView = new ClientsAdminView() { Id = user.Id, Name = user.UserName };
            //TODO IR BUSCAR AS RESERVAS
            if (clientsAdminView == null)
                return NotFound();

            return View(clientsAdminView);
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