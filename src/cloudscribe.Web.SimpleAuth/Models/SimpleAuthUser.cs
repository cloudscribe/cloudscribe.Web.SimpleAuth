// Copyright (c) Source Tree Solutions, LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// Author:                  Joe Audette
// Created:                 2016-02-09
// Last Modified:           2016-02-09
// 

using System.Collections.Generic;

namespace cloudscribe.Web.SimpleAuth.Models
{
    public class SimpleAuthUser
    {

       
        public string UserName { get; set; }
        public string Password { get; set; }
        public bool PasswordIsHashed { get; set; }
        public List<SimpleAuthClaim> Claims { get; set; }
    }
}
