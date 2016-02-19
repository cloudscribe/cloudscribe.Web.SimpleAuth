﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace cloudscribe.Web.SimpleAuth.Models
{
    public interface IUserLookupProvider
    {
        SimpleAuthUser GetUser(string userName);
    }
}
