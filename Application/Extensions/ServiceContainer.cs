using Application.Services;
using Application.Utils;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.DependencyInjection;
using NetcodeHub.Packages.Extensions.LocalStorage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Application.Extensions
{
    public static class ServiceContainer
    {
        public static IServiceCollection AddApplicationService(this IServiceCollection services)
        {
            services.AddScoped<IAccountService, AccountService>();
            services.AddAuthorizationCore();
            services.AddNetcodeHubLocalStorageService();
            services.AddScoped<Extensions.LocalStorageService>();
            services.AddScoped<HttpClientService>();
            services.AddScoped<AuthenticationStateProvider, CustomAuthenticationStateProvider>();
            services.AddTransient<CustomHttpHandler>();
            services.AddCascadingAuthenticationState();
            services.AddHttpClient(Constant.HttpClientName, client =>
            {
                client.BaseAddress = new Uri("https://localhost:7284");
            }).AddHttpMessageHandler<CustomHttpHandler>();

            return services;
        }
    }
}
