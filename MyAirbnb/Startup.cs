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

            services.AddDefaultIdentity<IdentityUser>(options =>
            {
                options.SignIn.RequireConfirmedAccount = false;
                options.SignIn.RequireConfirmedEmail = false;
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
        public async void Configure(IApplicationBuilder app, IWebHostEnvironment env, IServiceProvider serviceProvider, ApplicationDbContext context)
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
            //var adminEmail = "admin@myairbnb.com";
            //var admin = "administrator";
            //var user = new IdentityUser { UserName = admin, Email = adminEmail };
            //var result = await userManager.CreateAsync(user, admin);
            //if (result.Succeeded)
            //{
            //    var roleExists = await roleManager.RoleExistsAsync(App.AdminRole);
            //    if (!roleExists)
            //        await roleManager.CreateAsync(new IdentityRole { Name = App.AdminRole });

            //    await userManager.AddToRoleAsync(user, App.AdminRole);

            //}
            CreateRoles(serviceProvider, context);
        }
        private void CreateRoles(IServiceProvider serviceProvider, ApplicationDbContext context)
        {

            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();
            Task<IdentityResult> roleResult;
            string emailAdmin = "admin@myairbnb.com";

            //Check that there is an Administrator role and create if not
            Task<bool> hasAdminRole = roleManager.RoleExistsAsync(App.AdminRole);
            hasAdminRole.Wait();

            if (!hasAdminRole.Result)
            {
                roleResult = roleManager.CreateAsync(new IdentityRole(App.AdminRole));
                roleResult.Wait();
            }

            Task<bool> hasManagerRole = roleManager.RoleExistsAsync(App.ManagerRole);
            hasManagerRole.Wait();

            if (!hasManagerRole.Result)
            {
                roleResult = roleManager.CreateAsync(new IdentityRole(App.ManagerRole));
                roleResult.Wait();
            }

            Task<bool> hasWorkerRole = roleManager.RoleExistsAsync(App.WorkerRole);
            hasWorkerRole.Wait();

            if (!hasWorkerRole.Result)
            {
                roleResult = roleManager.CreateAsync(new IdentityRole(App.WorkerRole));
                roleResult.Wait();
            }

            //Check if the admin user exists and create it if not
            //Add to the Administrator role

            Task<IdentityUser> testUser = userManager.FindByEmailAsync(emailAdmin);
            testUser.Wait();

            if (testUser.Result == null)
            {
                IdentityUser administrator = new IdentityUser();
                administrator.Email = emailAdmin;
                administrator.UserName = App.AdminRole;

                Task<IdentityResult> newUser = userManager.CreateAsync(administrator, "_AStrongP@ssword!");
                newUser.Wait();

                if (newUser.Result.Succeeded)
                {
                    userManager.AddToRoleAsync(administrator, App.AdminRole).Wait();
                    userManager.AddToRoleAsync(administrator, App.ManagerRole).Wait();
                    userManager.AddToRoleAsync(administrator, App.WorkerRole).Wait();


                    Models.Worker worker = new Models.Worker { Id = administrator.Id };
                    context.Managers.Add(new Models.Manager { Id = administrator.Id, Workers = (new[] { worker }).ToList()});
                    context.Workers.Add(worker);
                    context.SaveChangesAsync();

                }
            }
        }
    }
}
