using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.UI;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MyAirbnb.Data;
using MyAirbnb.Models;
using MyAirbnb.Other;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace MyAirbnb
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("pt-PT");

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));
            services.AddDatabaseDeveloperPageExceptionFilter();

            services.AddDefaultIdentity<ApplicationUser>(options =>
            {
                options.SignIn.RequireConfirmedAccount = false;
                options.SignIn.RequireConfirmedEmail = false;
                options.SignIn.RequireConfirmedPhoneNumber = false;
                options.Password.RequireDigit = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireLowercase = false;
            })
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>();

            services.AddControllersWithViews().AddRazorRuntimeCompilation();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IServiceProvider serviceProvider, ApplicationDbContext context)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseMigrationsEndPoint();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
                endpoints.MapRazorPages();
            });

            CreateRoles(serviceProvider, context);
        }
        private void CreateRoles(IServiceProvider serviceProvider, ApplicationDbContext context)
        {

            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            //Check that there is an Administrator role and create if not, etc

            var rolesToCreate = new[] { App.AdminRole, App.ManagerRole, App.WorkerRole, App.ClientRole };

            foreach( var role in rolesToCreate)
                if (!roleManager.RoleExistsAsync(role).Result)
                    roleManager.CreateAsync(new IdentityRole(role)).Wait();
            context.SaveChanges();

            //Check if the admin user exists and create it if not
            //Add to the Administrator role
            string emailAdmin = @"admin@myairbnb.com";
            string passwordAdmin = @"_AStrongPassword";

            if (userManager.FindByEmailAsync(emailAdmin).Result == null)
            {
                ApplicationUser administrator = new()
                {
                    Email = emailAdmin,
                    UserName = emailAdmin,
                    EmailConfirmed = true,
                    FirstName = "Admin",
                    LastName = "Person"
                };

                if (userManager.CreateAsync(administrator, passwordAdmin).Result.Succeeded)
                {
                    userManager.AddToRoleAsync(administrator, App.AdminRole).Wait();
                    //userManager.AddToRoleAsync(administrator, App.ManagerRole).Wait();
                    //userManager.AddToRoleAsync(administrator, App.WorkerRole).Wait();

                    var worker = new Worker { Id = administrator.Id };
                    context.Managers.Add(new Manager { Id = administrator.Id, Workers = new[] { worker }.ToList() });
                    context.Workers.Add(worker);
                    context.SaveChanges();
                }
            }
        }
    }
}
