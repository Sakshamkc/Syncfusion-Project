using ExpenseTracker.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Threading.Tasks;

namespace ExpenseTracker
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();
            services.AddRazorPages(); // required for Identity UI endpoints

            var cs = Configuration.GetConnectionString("DevConnection");

            // Identity DB
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseMySql(cs, ServerVersion.AutoDetect(cs)));

            // App tables DB (THIS WAS MISSING)
            services.AddDbContext<AppDbContext>(options =>
                options.UseMySql(cs, ServerVersion.AutoDetect(cs)));

            services.AddIdentity<IdentityUser, IdentityRole>()
                .AddEntityFrameworkStores<AppDbContext>() // Identity uses Identity DB
                .AddDefaultTokenProviders();
            services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = "/Account/Login";
                options.AccessDeniedPath = "/Account/Login";
            });
            Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense(
                "Mgo+DSMBMAY9C3t2VVhjQlFaclhJXGFWfVJpTGpQdk5xdV9DaVZUTWY/P1ZhSXxRd0ViUH5edHVWTmFVWEc=");
        }
        // Startup.cs → add this method
        private async Task CreateUser(IApplicationBuilder app)
        {
            using var scope = app.ApplicationServices.CreateScope();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

            var email = "admin@expense.com";
            var password = "Admin@123";

            if (await userManager.FindByEmailAsync(email) == null)
            {
                var user = new IdentityUser
                {
                    UserName = email,
                    Email = email,
                    EmailConfirmed = true
                };

                await userManager.CreateAsync(user, password);
            }
        }


        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            CreateUser(app).GetAwaiter().GetResult();
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
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
                    pattern: "{controller=Dashboard}/{action=Index}/{id?}");

                // Identity UI endpoints
                endpoints.MapRazorPages();
            });
        }
    }
}
