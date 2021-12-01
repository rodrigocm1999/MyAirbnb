using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using MyAirbnb.Models;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;

namespace MyAirbnb.DAL
{
    public class SiteContext : DbContext
    {
        public SiteContext() : base() { }

        public DbSet<Post> Posts { get; set; }
        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            //modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
        }
    }
}
