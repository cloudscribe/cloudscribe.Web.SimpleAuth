using cloudscribe.Web.SimpleAuth.Models;

namespace multiTenant.WebApp
{
    public class AppTenantAuthSettingsResolver : IAuthSettingsResolver
    {
        public AppTenantAuthSettingsResolver(AppTenant tenant)
        {
            this.tenant = tenant;

            authSettings = new SimpleAuthSettings();
            authSettings.AuthenticationScheme = tenant.AuthenticationScheme;
            authSettings.RecaptchaPrivateKey = tenant.RecaptchaPrivateKey;
            authSettings.RecaptchaPublicKey = tenant.RecaptchaPublicKey;
            authSettings.EnablePasswordHasherUi = tenant.EnablePasswordHasherUi;
        }

        private AppTenant tenant;
        private SimpleAuthSettings authSettings;

        public SimpleAuthSettings GetCurrentAuthSettings()
        {
            return authSettings;
        }
    }
}
