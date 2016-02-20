
using cloudscribe.Web.SimpleAuth.Models;
using Microsoft.Extensions.OptionsModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace cloudscribe.Web.SimpleAuth.Services
{
    public class DefaultAuthSettingsResolver : IAuthSettingsResolver
    {
        public DefaultAuthSettingsResolver(IOptions<SimpleAuthSettings> settingsAccessor)
        {
            authSettings = settingsAccessor.Value;
        }

        private SimpleAuthSettings authSettings;

        public SimpleAuthSettings GetCurrentAuthSettings()
        {
            return authSettings;
        }
    }
}
