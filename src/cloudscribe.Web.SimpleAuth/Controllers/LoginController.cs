// Copyright (c) Source Tree Solutions, LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// Author:                  Joe Audette
// Created:                 2016-02-09
// Last Modified:           2016-02-09
// 


using cloudscribe.Web.SimpleAuth.Models;
using cloudscribe.Web.SimpleAuth.ViewModels;
using Microsoft.AspNet.Authorization;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Http.Authentication;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.OptionsModel;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;


namespace cloudscribe.Web.SimpleAuth.Controllers
{
    public class LoginController : Controller
    {
        public LoginController(
            IOptions<SimpleAuthSettings> settingsAccessor,
            IOptions<List<SimpleAuthUser>> usersAccessor,
            IPasswordHasher<SimpleAuthUser> passwordHasher,
            ILogger<LoginController> logger)
        {
            authSettings = settingsAccessor.Value;
            allUsers = usersAccessor.Value;
            this.passwordHasher = passwordHasher;
            log = logger;
        }

        private SimpleAuthSettings authSettings;
        private IPasswordHasher<SimpleAuthUser> passwordHasher;
        private List<SimpleAuthUser> allUsers;
        private ILogger log;


        //SimpleAuthSettings
        // GET: /Account/index
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Index()
        {
            ViewData["Title"] = "Login";

            var model = new LoginViewModel();

            if (!string.IsNullOrEmpty(authSettings.RecaptchaPublicKey))
            {
                model.RecaptchaSiteKey = authSettings.RecaptchaPublicKey;
            }

            return View(model);
        }

        //
        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(LoginViewModel model)
        {
            ViewData["Title"] = "Log in";
            
            if(!string.IsNullOrEmpty(authSettings.RecaptchaPublicKey))
            {
                model.RecaptchaSiteKey = authSettings.RecaptchaPublicKey;
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (!string.IsNullOrEmpty(authSettings.RecaptchaPublicKey))
            {
                var recpatchaSecretKey = authSettings.RecaptchaPrivateKey;
                var captchaResponse = await ValidateRecaptcha(Request, recpatchaSecretKey);

                if (!captchaResponse.Success)
                {
                    ModelState.AddModelError("recaptchaerror", "reCAPTCHA Error occured. Please try again");
                    return View(model);
                }

            }

            var authUser = GetUser(model.UserName);

            if(authUser == null)
            {
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                return View(model);
            }

            var isValid = ValidatePassword(authUser, model.Password);

            if(!isValid)
            {
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                return View(model);

            }
            
            var authProperties = new AuthenticationProperties();
            authProperties.IsPersistent = model.RememberMe;

            var claimsPrincipal = GetClaimsPrincipal(authUser);
            
            await HttpContext.Authentication.SignInAsync(
                authSettings.AuthenticationScheme, 
                claimsPrincipal, 
                authProperties);
            
            return LocalRedirect("/");
        }

        // a ui for hashing a password so it can be stored as hashed in simpleauth.json
        [HttpGet]
        [AllowAnonymous]
        public IActionResult HashPassword()
        {
            if (!authSettings.EnablePasswordHasherUi)
            {
                log.LogInformation("returning 404 because EnablePasswordHasherUi is false");
                Response.StatusCode = 404;
                return new EmptyResult();
            }

            var model = new HashPasswordViewModel();

            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        public IActionResult HashPassword(HashPasswordViewModel model)
        {
            if (!authSettings.EnablePasswordHasherUi)
            {
                log.LogInformation("returning 404 because EnablePasswordHasherUi is false");
                Response.StatusCode = 404;
                return new EmptyResult();
            }

            if (model.InputPassword.Length > 0)
            {
                var fakeUser = new SimpleAuthUser();
                model.OutputHash = passwordHasher.HashPassword(fakeUser, model.InputPassword);
            }

            return View(model);
        }


        // POST: /Account/LogOff
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LogOff()
        {
            await HttpContext.Authentication.SignOutAsync(authSettings.AuthenticationScheme);
 
            return LocalRedirect("/");
        }

        private bool ValidatePassword(SimpleAuthUser authUser, string providedPassword)
        {
            if (authUser.PasswordIsHashed)
            {
                var result = passwordHasher.VerifyHashedPassword(authUser, authUser.Password, providedPassword);   
                return (result == PasswordVerificationResult.Success);
            }
            else
            {
                return authUser.Password == providedPassword;
            }

        }

        private SimpleAuthUser GetUser(string userName)
        {
            foreach (SimpleAuthUser u in allUsers)
            {
                if (u.UserName == userName) { return u; }
            }

            return null;
        }

        private async Task<RecaptchaResponse> ValidateRecaptcha(
            HttpRequest request,
            string secretKey)
        {
            var response = request.Form["g-recaptcha-response"];
            var client = new HttpClient();
            string result = await client.GetStringAsync(
                string.Format("https://www.google.com/recaptcha/api/siteverify?secret={0}&response={1}",
                    secretKey,
                    response)
                    );

            var captchaResponse = JsonConvert.DeserializeObject<RecaptchaResponse>(result);

            return captchaResponse;
        }

        private ClaimsPrincipal GetClaimsPrincipal(SimpleAuthUser authUser)
        {
            var identity = new ClaimsIdentity(authSettings.AuthenticationScheme);
            identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, "1"));
            identity.AddClaim(new Claim(ClaimTypes.Name, authUser.UserName));

            foreach(SimpleAuthClaim c in authUser.Claims)
            {
                if(c.ClaimType == "Email")
                {
                    identity.AddClaim(new Claim(ClaimTypes.Email, c.ClaimValue));
                }
                else if (c.ClaimType == "Role")
                {
                    identity.AddClaim(new Claim(ClaimTypes.Role, c.ClaimValue));
                }
                else
                {
                    identity.AddClaim(new Claim(c.ClaimType, c.ClaimValue));
                }
            }
            
            return new ClaimsPrincipal(identity);
        }
    }
}
