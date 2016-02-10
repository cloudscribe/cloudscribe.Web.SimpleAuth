# cloudscribe.Web.SimpleAuth

Simple Authentication for ASP.NET Core - because sometimes less is more

#### You should not use SimpleAuth if any of these requirements apply to your web application or website project:
* You need more than a very small number of users to be able to authenticate on the site
* You need users to be able to self register using a registration page

If either of the above are true of your project then you should probably look at these other projects instead of SimpleAuth:

* [ASP.NET Identity](https://github.com/aspnet/Identity)
* [IdentityServer4](https://github.com/IdentityServer/IdentityServer4)
* [cloudscribe.Core](https://github.com/joeaudette/cloudscribe) - one of my other projects :-D  

#### So what scenarios is SimpleAuth good for?
* Personal sites or blogs where only one or a few people need to login
* Brochure sites where only one or a few people need to login
* Demo sites or prototypes where you only need a few users to be able to login

#### What is the value proposition?
* No database required for user accounts
* Easy configuration - settings and users are stored in a simple json configuration file
* Built in support for [Recaptcha](https://www.google.com/recaptcha/intro/index.html) -just add your keys in configuration
* Supports custom role and claims configuration for use with [custom authorization policies](https://docs.asp.net/en/latest/security/authorization/policies.html)
* Supports hashed passwords using the PasswordHasher from ASP.NET Identity
* Supports clear text passwords so you can get started easily but you should use hashed passwords
* PasswordHashing utitlity built in at /Login/HashPassword so you can let your few users generate their own hash that you can add to the config file. The utility can be disabled and will return a 404 if disabled.
* Lightweight
* Very simple implementation without many moving parts


#### Installation - Currently supports RC1 of ASP.NET Core

The example.WebApp project in this solution is the best guide to setup and configuration, I've added some comments in the Startup.cs that should be useful, and if you have any trouble setting it up in your app, you can compare against the demo to see what is missing or different. Note that the example.WebApp also uses cloudscribe.Navigation for the menu, and you may also want to use it.

The basic installaiton steps are as follows:

- [] 1 add this in your project.json:

        "cloudscribe.Web.SimpleAuth": "1.0.0-*
	
Visual Studio 2015 should automatically pull it in from [nuget.org](https://www.nuget.org/packages/cloudscribe.Web.SimpleAuth), but you can also run dnu restore from the command line in the solution or project folder.

- [] 2 copy the simpleauthsettings.json file from the example.WebApp project into your own and use it to configure settings and users

- [] 3 add this using statement to the top of your Startup.cs

        using cloudscribe.Web.SimpleAuth.Models;
        using Microsoft.Extensions.OptionsModel;
        using Microsoft.AspNet.Identity; // this is only used for the password hasher

- [] 4 add this in the StartupMethod of your Startup.cs

        builder.AddJsonFile("simpleauthsettings.json", optional: true);

- [] 5 add this in the ConfigureServices method of your Startup.cs

        services.Configure<SimpleAuthSettings>(Configuration.GetSection("SimpleAuthSettings"));
        services.Configure<List<SimpleAuthUser>>(Configuration.GetSection("Users"));
        services.AddScoped<IPasswordHasher<SimpleAuthUser>, PasswordHasher<SimpleAuthUser>>();
	
- [] 6 change the signature of the Configure method in your Startup.cs so the DI can inject the SimpleAuthSettings accessor into that method

        public void Configure(
                IApplicationBuilder app, 
                IHostingEnvironment env, 
                ILoggerFactory loggerFactory,
                IOptions<SimpleAuthSettings> authSettingsAccessor  
                )
	 {
	    ...
		
		// Add cookie-based authentication to the request pipeline

		SimpleAuthSettings authSettings = authSettingsAccessor.Value;

		var ApplicationCookie = new CookieAuthenticationOptions
		{
			AuthenticationScheme = authSettings.AuthenticationScheme,
			CookieName = authSettings.AuthenticationScheme,
			AutomaticAuthenticate = true,
			AutomaticChallenge = true,
			LoginPath = new PathString("/Login/Index"),
			Events = new CookieAuthenticationEvents
			{
				//OnValidatePrincipal = SecurityStampValidator.ValidatePrincipalAsync
			}
		};
		
		app.UseCookieAuthentication(ApplicationCookie);
		
		// make sure authentication is configured before MVC
		app.UseMvc();
		
	    }
	
	
		
- [] 7 copy the Views/Login folder from the example.WebApp project into your own project

- [] 8 study the example.WebApp for examples of how to configure authorization policies with roles that you configure for users in the simpleauthsettings.json file

- [] 9 you will of course have to provide a link in your app to the login page, look at the _LoginPartial.cs in the Views/Shared folder of example.WebApp

In future releases when nuget supports it, we will either include the views in the package or create a separate package for views, but for now you just have to copy them from the example site.


![Screenshot](/images/screenshot-simleauth-with-recaptcha.jpg?raw=true)


##### Keep In Touch

If you are interested in hiring me for consulting or other support services related to the cloudscribe set of projects, please send an email to info [at] cloudscribe.com.

If you have questions or just want to be social, say hello in our gitter chat room. I try to monitor that room on a regular basis while I'm working, but if I'm not around you can leave a message.

[![Join the chat at https://gitter.im/joeaudette/cloudscribe](https://badges.gitter.im/joeaudette/cloudscribe.svg)](https://gitter.im/joeaudette/cloudscribe?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge)

I'm also on twitter @cloudscribeweb and @joeaudette


[![Join the chat at https://gitter.im/joeaudette/cloudscribe](https://badges.gitter.im/joeaudette/cloudscribe.svg)](https://gitter.im/joeaudette/cloudscribe?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge)
