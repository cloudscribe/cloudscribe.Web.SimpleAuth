
using cloudscribe.Web.SimpleAuth.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace example.WebApp
{
    public class AppTenant
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string[] Hostnames { get; set; }
        public string Theme { get; set; }
        public string ConnectionString { get; set; }
        public List<SimpleAuthUser> Users { get; set; }

        // overrides for default simpleauthsettings
        public bool EnablePasswordHasherUi { get; set; } = false;
        public string RecaptchaPublicKey { get; set; }
        public string RecaptchaPrivateKey { get; set; }
        public string AuthenticationScheme { get; set; } = "application";

        public bool Equals(AppTenant other)
        {
            if (other == null)
            {
                return false;
            }

            return other.Name.Equals(Id);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as AppTenant);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }
}
