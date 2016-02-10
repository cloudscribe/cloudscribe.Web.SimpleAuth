// Copyright (c) Source Tree Solutions, LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// Author:                  Joe Audette
// Created:                 2016-02-09
// Last Modified:           2016-02-09
// 


using System.Collections.Generic;

namespace cloudscribe.Web.SimpleAuth.Models
{
    public class SimpleAuthSettings
    {
        //public List<SimpleAuthUser> Users { get; set; }

        /// <summary>
        /// if true the /Login/Hasher will be available to use for generating a hash, 
        /// that can then be stored in settings instead of clear textpassword
        /// </summary>
        public bool EnablePasswordHasherUi { get; set; } = false;

        public string RecaptchaPublicKey { get; set; }
        public string RecaptchaPrivateKey { get; set; }
        public string AuthenticationScheme { get; set; } = "application";
    }
}
