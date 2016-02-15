// Copyright (c) Source Tree Solutions, LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// Author:                  Joe Audette
// Created:                 2016-02-09
// Last Modified:           2016-02-15
// 


using cloudscribe.Web.SimpleAuth.Models;
using cloudscribe.Web.SimpleAuth.ViewModels;
using cloudscribe.Web.SimpleAuth.Services;
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
using System.Threading;
using System.Threading.Tasks;


namespace cloudscribe.Web.SimpleAuth.Controllers
{
    public class LoginController : Controller
    {
        public LoginController(
            SignInManager signinManager,
            ILogger<LoginController> logger)
        {
            this.signinManager = signinManager;
            log = logger;
        }

        private SignInManager signinManager;
        private ILogger log;
        private CancellationToken CancellationToken => HttpContext?.RequestAborted ?? CancellationToken.None;

        // GET: /Login/index
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Index()
        {
            ViewData["Title"] = "Login";

            var model = new LoginViewModel();

            if (!string.IsNullOrEmpty(signinManager.AuthSettings.RecaptchaPublicKey))
            {
                model.RecaptchaSiteKey = signinManager.AuthSettings.RecaptchaPublicKey;
            }

            return View(model);
        }

        
        // POST: /Login/Index
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(LoginViewModel model)
        {
            ViewData["Title"] = "Log in";
            
            if(!string.IsNullOrEmpty(signinManager.AuthSettings.RecaptchaPublicKey))
            {
                model.RecaptchaSiteKey = signinManager.AuthSettings.RecaptchaPublicKey;
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (!string.IsNullOrEmpty(signinManager.AuthSettings.RecaptchaPublicKey))
            {
                var recpatchaSecretKey = signinManager.AuthSettings.RecaptchaPrivateKey;
                var captchaResponse = await ValidateRecaptcha(Request, recpatchaSecretKey);

                if (!captchaResponse.Success)
                {
                    ModelState.AddModelError("recaptchaerror", "reCAPTCHA Error occured. Please try again");
                    return View(model);
                }

            }

            var authUser = await signinManager.GetUser(
                model.UserName,
                CancellationToken
                );

            if(authUser == null)
            {
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                return View(model);
            }

            var isValid = await signinManager.ValidatePassword(
                authUser, 
                model.Password,
                CancellationToken
                );

            if(!isValid)
            {
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                return View(model);

            }
            
            var authProperties = new AuthenticationProperties();
            authProperties.IsPersistent = model.RememberMe;

            var claimsPrincipal = await signinManager.GetClaimsPrincipal(
                authUser,
                CancellationToken);
            
            await HttpContext.Authentication.SignInAsync(
                signinManager.AuthSettings.AuthenticationScheme, 
                claimsPrincipal, 
                authProperties);
            
            return LocalRedirect("/");
        }

        // a ui for hashing a password so it can be stored as hashed in simpleauth.json
        [HttpGet]
        [AllowAnonymous]
        public IActionResult HashPassword()
        {
            if (!signinManager.AuthSettings.EnablePasswordHasherUi)
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
            if (!signinManager.AuthSettings.EnablePasswordHasherUi)
            {
                log.LogInformation("returning 404 because EnablePasswordHasherUi is false");
                Response.StatusCode = 404;
                return new EmptyResult();
            }

            if (model.InputPassword.Length > 0)
            {
                var fakeUser = new SimpleAuthUser();
                model.OutputHash = signinManager.HashPassword(model.InputPassword);
            }

            return View(model);
        }


        // POST: /Login/LogOff
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LogOff()
        {
            await HttpContext.Authentication.SignOutAsync(signinManager.AuthSettings.AuthenticationScheme);
 
            return LocalRedirect("/");
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

    
    }
}
