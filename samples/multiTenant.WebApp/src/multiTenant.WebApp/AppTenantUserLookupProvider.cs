
using cloudscribe.Web.SimpleAuth.Models;

namespace multiTenant.WebApp
{
    public class AppTenantUserLookupProvider : IUserLookupProvider
    {
        public AppTenantUserLookupProvider(AppTenant tenant)
        {
            this.tenant = tenant;
        }

        private AppTenant tenant;

        public SimpleAuthUser GetUser(string userName)
        {
            foreach (SimpleAuthUser u in tenant.Users)
            {
                if (u.UserName == userName) { return u; }
            }

            return null;
        }
    }
}
