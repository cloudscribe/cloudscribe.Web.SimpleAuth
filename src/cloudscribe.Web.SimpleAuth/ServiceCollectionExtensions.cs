using cloudscribe.Web.SimpleAuth.Models;
using cloudscribe.Web.SimpleAuth.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddCloudscribeSimpleAuth(
            this IServiceCollection services
            )
        {
            services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.TryAddScoped<IUserLookupProvider, DefaultUserLookupProvider>(); // single tenant
            services.TryAddScoped<IPasswordHasher<SimpleAuthUser>, PasswordHasher<SimpleAuthUser>>();
            services.TryAddScoped<IAuthSettingsResolver, DefaultAuthSettingsResolver>();
            services.AddScoped<SignInManager, SignInManager>();

            return services;
        }
    }
}
