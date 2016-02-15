// Copyright (c) Source Tree Solutions, LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// Author:                  Joe Audette
// Created:                 2016-02-15
// Last Modified:           2016-02-15
// 

using cloudscribe.Web.SimpleAuth.Models;
using Microsoft.AspNet.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.OptionsModel;
using System.Collections.Generic;
using System.Security.Claims;

namespace cloudscribe.Web.SimpleAuth.Services
{
    public class SignInManager
    {
        public SignInManager(
            IOptions<SimpleAuthSettings> settingsAccessor,
            IOptions<List<SimpleAuthUser>> usersAccessor,
            IPasswordHasher<SimpleAuthUser> passwordHasher,
            ILogger<SignInManager> logger)
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

        public SimpleAuthSettings AuthSettings
        {
            get { return authSettings; }
        }

        public SimpleAuthUser GetUser(string userName)
        {
            foreach (SimpleAuthUser u in allUsers)
            {
                if (u.UserName == userName) { return u; }
            }

            return null;
        }

        public bool ValidatePassword(SimpleAuthUser authUser, string providedPassword)
        {
            bool result = false;
            if (authUser.PasswordIsHashed)
            {
                var hashResult = passwordHasher.VerifyHashedPassword(authUser, authUser.Password, providedPassword);
                result = (hashResult == PasswordVerificationResult.Success);

            }
            else
            {
                result = authUser.Password == providedPassword;
            }

            return result;
        }

        public ClaimsPrincipal GetClaimsPrincipal(SimpleAuthUser authUser)
        {
            var identity = new ClaimsIdentity(authSettings.AuthenticationScheme);
            identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, "1"));
            identity.AddClaim(new Claim(ClaimTypes.Name, authUser.UserName));

            foreach (SimpleAuthClaim c in authUser.Claims)
            {
                if (c.ClaimType == "Email")
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

            var prince = new ClaimsPrincipal(identity);

            return prince;
        }

        public string HashPassword(string inputPassword)
        {
            var fakeUser = new SimpleAuthUser();
            return passwordHasher.HashPassword(fakeUser, inputPassword);
        }


    }
}
