using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MyAirbnb.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace MyAirbnb.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {

        public DbSet<Post> Posts { get; set; }
        public DbSet<Comodity> Comodities { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
        }
    }
}
