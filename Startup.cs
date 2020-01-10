using System;
using GreenHealth.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Identity;
using GreenHealth.Controllers;
using GreenHealth.Repositories;
using GreenHealth.Persistence.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using System.Net.Mail;
using System.Net;
using GreenHealth.Email;
using Microsoft.EntityFrameworkCore;

namespace GreenHealth
{
    public class Startup
    {
        private IConfiguration _config;
        public Startup(IConfiguration configuration)
        {
            _config = configuration;
        }

        public IConfiguration Configuration { get; }
     

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<AppDbContext>(
                options => options.UseSqlServer(_config.GetConnectionString("GreenDbConnection")));
            services.AddTransient<SmtpClient>((serviceProvider) =>
            {
                var config = serviceProvider.GetRequiredService<IConfiguration>();
                return new SmtpClient()
                {
                    Host = config.GetValue<String>("Email:Smtp:Host"),
                    Port = config.GetValue<int>("Email:Smtp:Port"),
                    Credentials = new NetworkCredential(
                            config.GetValue<String>("Email:Smtp:Username"),
                            config.GetValue<String>("Email:Smtp:Password")
                        )
                };
            });
            services.AddDistributedMemoryCache();
            services.AddSession(options =>
            {
                options.Cookie.Name = ".GreenHealth.Session";
                options.IdleTimeout = TimeSpan.FromSeconds(60 *60*24);
                options.Cookie.IsEssential = true;
            });

            services.AddSendGridEmailSender();
            services.AddScoped<IPatientRepository, PatientRepository>();
            services.AddTransient<IDoctorRepository, DoctorRepository>();
            services.AddTransient<ISpecializationRepository, SpecializationRepository>();
            services.AddTransient<IProfile, ProfileRepository>();
            services.AddTransient<IUnitOfWork, UnitOfWork>();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>(); 
            services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();
            services.AddIdentity<ApplicationUser, ApplicationRole>(options => {
                options.Password.RequireUppercase = false;
                options.Password.RequireNonAlphanumeric = false;

                //options.SignIn.RequireConfirmedEmail = true;
            }).AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultUI()
            .AddDefaultTokenProviders();
            // requires
            // using Microsoft.AspNetCore.Identity.UI.Services;
            // using WebPWrecover.Services;

            // using Microsoft.AspNetCore.Identity.UI.Services;
            //services.AddSingleton<IEmailSender, EmailSender>();

            // requires
            ////services.AddTransient<IEmailSender, SendGridEmailSender>();
            //services.Configure<AuthMessageSenderOptions>(Configuration);


            services.AddControllersWithViews();
            services.AddMvc().AddXmlSerializerFormatters();
            services.AddAuthentication()
                .AddFacebook(options =>
                {
                    options.AppId = _config["Authentication:FacebookAppId"];
                    options.AppSecret = _config["Authentication:FacebookAppSecret"];
                });
            services.AddAuthentication()
           .AddGoogle(options =>
             {
                 //options.ClientId = "477962730760-f83d8un90ithcmdr7mm2qiqfh2cclqlh.apps.googleusercontent.com";
                 //options.ClientSecret = "dp3RmnKBQcRlgoESHvKbR3kq";
                 options.ClientId = _config["Authentication:GoogleClientId"];
                 options.ClientSecret = _config["Authentication:GoogleClientSecret"];
             });


        }

        ////Role Creation Setup
        //private async Task CreateUserRoles(IServiceProvider serviceProvider)
        //{
        //    var RoleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        //    var UserManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        //    IdentityResult roleResult;
        //    //Adding Admin Role
        //    var roleCheck = await RoleManager.RoleExistsAsync("Admin");
        //    if (!roleCheck)
        //    {
        //        //create the roles and seed them to the database
        //        roleResult = await RoleManager.CreateAsync(new IdentityRole("Admin"));
        //    }
        //    //Assign Admin role to the main User here we have given our newly registered 
        //    //login id for Admin management
        //    ApplicationUser user = await UserManager.FindByEmailAsync("tanvidhupkar0728@gmail.com");
        //    var User = new ApplicationUser();
        //    await UserManager.AddToRoleAsync(user, "Admin");
        //}

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, AppDbContext context, 
            RoleManager<ApplicationRole> roleManager, UserManager<ApplicationUser> userManager, 
            SignInManager<ApplicationUser> signInManager)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseSession();
            //app.UseHttpContextItemsMiddleware();

            app.UseAuthentication();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
            //SeedData.EnsurePopulated(app);
            AdminController.Initialize(context, userManager, roleManager).Wait();
        }
    }
}
