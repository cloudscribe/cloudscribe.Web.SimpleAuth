
using cloudscribe.Web.SimpleAuth.Models;
using Microsoft.Extensions.OptionsModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace cloudscribe.Web.SimpleAuth.Services
{
    public class DefaultUserLookupProvider : IUserLookupProvider
    {
        public DefaultUserLookupProvider(IOptions<List<SimpleAuthUser>> usersAccessor)
        {
            allUsers = usersAccessor.Value;
        }

        private List<SimpleAuthUser> allUsers;

        public SimpleAuthUser GetUser(string userName)
        {
            foreach (SimpleAuthUser u in allUsers)
            {
                if (u.UserName == userName) { return u; }
            }

            return null;
        }
    }
}
