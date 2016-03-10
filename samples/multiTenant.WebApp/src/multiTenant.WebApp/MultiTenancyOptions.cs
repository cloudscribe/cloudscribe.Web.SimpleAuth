using System.Collections.Generic;

namespace multiTenant.WebApp
{
    public class MultiTenancyOptions
    {
        public List<AppTenant> Tenants { get; set; }
    }
}
