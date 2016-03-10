﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Authentication.Cookies;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Hosting;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Data.Entity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.OptionsModel;
using Microsoft.Extensions.Logging;
using Microsoft.AspNet.Identity; // only used for password hasher
using cloudscribe.Web.SimpleAuth.Models;
using cloudscribe.Web.SimpleAuth.Services;


namespace basic.WebApp
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
                builder.AddUserSecrets();
            }

            builder.AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; set; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<SimpleAuthSettings>(Configuration.GetSection("SimpleAuthSettings"));
            services.Configure<List<SimpleAuthUser>>(Configuration.GetSection("Users"));
            services.AddScoped<IPasswordHasher<SimpleAuthUser>, PasswordHasher<SimpleAuthUser>>();
            services.AddScoped<IAuthSettingsResolver, DefaultAuthSettingsResolver>();
            services.AddScoped<IUserLookupProvider, DefaultUserLookupProvider>(); // single tenant
            services.AddScoped<SignInManager, SignInManager>();


            services.AddMvc();

            
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(
            IApplicationBuilder app, 
            IHostingEnvironment env, 
            ILoggerFactory loggerFactory,
            IOptions<SimpleAuthSettings> authSettingsAccessor
            )
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            if (env.IsDevelopment())
            {
                app.UseBrowserLink();
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
  
            }

            app.UseIISPlatformHandler(options => options.AuthenticationDescriptions.Clear());

            app.UseStaticFiles();

            // Add cookie-based authentication to the request pipeline

            SimpleAuthSettings authSettings = authSettingsAccessor.Value;

            var ApplicationCookie = new CookieAuthenticationOptions
            {
                AuthenticationScheme = authSettings.AuthenticationScheme,
                CookieName = authSettings.AuthenticationScheme,
                AutomaticAuthenticate = true,
                AutomaticChallenge = true,
                LoginPath = new PathString("/Login/Index")
                
            };

            app.UseCookieAuthentication(ApplicationCookie);


            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }

        // Entry point for the application.
        public static void Main(string[] args) => WebApplication.Run<Startup>(args);
    }
}
