using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Http;

namespace example.WebApp
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true);

            // you can use whatever file name you like and it is probably a good idea to use a custom file name
            // just an a small extra protection in case hackers try some kind of attack based on knowing the name of the file
            // it should not be possible for anyone to get files outside of wwwroot using http requests
            // but every little thing you can do for stronger security is a good idea
            builder.AddJsonFile("simpleauthsettings.json", optional: true);

            // this file name is ignored by gitignore in our git repo
            // so you can create it and use on your local dev machine
            // remember last config source added wins if it has the same settings
            //builder.AddJsonFile("appsettings.local.overrides.json", optional: true);

            if (env.IsDevelopment())
            {
                // For more details on using the user secret store see http://go.microsoft.com/fwlink/?LinkID=532709
                builder.AddUserSecrets();
            }

            // note that the order in which configuration sources are added is important
            // if the same settings exist in a source registered later, the later settings win
            // so for example in production or in Azure hosting you might use environment variables
            // while on your dev machine using the json file as above
            builder.AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // see this method below and configure your security policy
            ConfigureAuthPolicy(services);

            services.Configure<MultiTenancyOptions>(Configuration.GetSection("MultiTenancy"));
            services.AddMultitenancy<AppTenant, CachingAppTenantResolver>();
            
            //services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            services.Configure<cloudscribe.Web.SimpleAuth.Models.SimpleAuthSettings>(Configuration.GetSection("SimpleAuthSettings"));
            services.AddScoped<cloudscribe.Web.SimpleAuth.Models.IUserLookupProvider, AppTenantUserLookupProvider>();
            services.Configure<List<cloudscribe.Web.SimpleAuth.Models.SimpleAuthUser>>(Configuration.GetSection("Users"));
            services.AddScoped<cloudscribe.Web.SimpleAuth.Models.IAuthSettingsResolver, AppTenantAuthSettingsResolver>();
            services.AddCloudscribeSimpleAuth();


            // this demo is also using the cloudscribe.Web.Navigation library
            //https://github.com/joeaudette/cloudscribe.Web.Navigation
            services.AddCloudscribeNavigation(Configuration);

            services.AddMvc();

            services.Configure<RazorViewEngineOptions>(options =>
            {
                options.ViewLocationExpanders.Add(new TenantViewLocationExpander());
            });


        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();

            app.UseMultitenancy<AppTenant>();

            app.UsePerTenant<AppTenant>((ctx, builder) =>
            {
                var options = new CookieAuthenticationOptions();
                options.AuthenticationScheme = ctx.Tenant.AuthenticationScheme;
                options.LoginPath = new PathString("/account/login");
                options.AccessDeniedPath = new PathString("/account/forbidden");
                options.AutomaticAuthenticate = true;
                options.AutomaticChallenge = true;
                options.CookieName = $"{ctx.Tenant.Id}.application";
                //options.Events = new CookieAuthenticationEvents
                    //{
                    //    OnValidatePrincipal = SecurityStampValidator.ValidatePrincipalAsync
                    //};
                builder.UseCookieAuthentication(options);

               
            });

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }


        private void ConfigureAuthPolicy(IServiceCollection services)
        {
            // read the docs to better understand authorization policy configuration
            //https://docs.asp.net/en/latest/security/authorization/policies.html

            services.AddAuthorization(options =>
            {
                // Note that the navigation menu uses cloudscribe.Web.Navigation
                // which filters the menu based on role names not on policy names
                // so if you change the policy roles you need to update the navigation.xml file
                //

                // see the simpleauthsettings.json file to understand how to configure a users role membership

                options.AddPolicy(
                    "AdminPolicy",
                    authBuilder =>
                    {
                        authBuilder.RequireRole("Admins");
                    }
                 );

                options.AddPolicy(
                    "MembersOnlyPolicy",
                    authBuilder =>
                    {
                        authBuilder.RequireRole("Admins", "Members");
                    }
                 );

                // add other policies here 

            });

        }
    }
}
