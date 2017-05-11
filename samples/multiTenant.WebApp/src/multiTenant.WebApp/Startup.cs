using cloudscribe.Web.SimpleAuth.Models;
using cloudscribe.Web.SimpleAuth.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Collections.Generic;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.IO;
using Microsoft.Extensions.Options;

namespace multiTenant.WebApp
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            // Set up configuration sources.
            var builder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true);

            // you can use whatever file name you like and it is probably a good idea to use a custom file name
            // just an a small extra protection in case hackers try some kind of attack based on knowing the name of the file
            // it should not be possible for anyone to get files outside of wwwroot using http requests
            // but every little thing you can do for stronger security is a good idea
            builder.AddJsonFile("simpleauthsettings.json", optional: true);

            if (env.IsDevelopment())
            {
                // For more details on using the user secret store see http://go.microsoft.com/fwlink/?LinkID=532709
                builder.AddUserSecrets<Startup>();
            }

            builder.AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; set; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<MultiTenancyOptions>(Configuration.GetSection("MultiTenancy"));
            services.AddMultitenancy<AppTenant, CachingAppTenantResolver>();
            services.AddCloudscribeSimpleAuth();

            services.AddScoped<IUserLookupProvider, AppTenantUserLookupProvider>();
            services.Configure<List<SimpleAuthUser>>(Configuration.GetSection("Users"));
            services.AddScoped<IPasswordHasher<SimpleAuthUser>, PasswordHasher<SimpleAuthUser>>();
            services.AddScoped<IAuthSettingsResolver, AppTenantAuthSettingsResolver>();
            services.AddScoped<SignInManager, SignInManager>();


            services.AddMvc();
            services.Configure<RazorViewEngineOptions>(options =>
            {
                options.ViewLocationExpanders.Add(new TenantViewLocationExpander());
            });


        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(
            IApplicationBuilder app,
            IHostingEnvironment env,
            ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            if (env.IsDevelopment())
            {
                app.UseBrowserLink();
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");


            }

            //app.UseIISPlatformHandler(options => options.AuthenticationDescriptions.Clear());

            app.UseStaticFiles();

            app.UseMultitenancy<AppTenant>();
            app.UsePerTenant<AppTenant>((ctx, builder) =>
            {
                var ApplicationCookie = new CookieAuthenticationOptions
                {
                    AuthenticationScheme = ctx.Tenant.AuthenticationScheme,
                    LoginPath = new PathString("/login/index"),
                    AccessDeniedPath = new PathString("/"),
                    AutomaticAuthenticate = true,
                    AutomaticChallenge = true,
                    CookieName = $"{ctx.Tenant.Id}.application",
                    Events = new CookieAuthenticationEvents
                    {
                        OnValidatePrincipal = SecurityStampValidator.ValidatePrincipalAsync
                    }
                };
                builder.UseCookieAuthentication(ApplicationCookie);
            });




            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }

        // Entry point for the application.
        public static void Main(string[] args)
        {
            var host = new WebHostBuilder()
                .UseKestrel()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseIISIntegration()
                .UseStartup<Startup>()
                .Build();

            host.Run();
        }
    }
}
