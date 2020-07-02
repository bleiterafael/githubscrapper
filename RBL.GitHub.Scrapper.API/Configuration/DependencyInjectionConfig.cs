using Microsoft.Extensions.DependencyInjection;
using RBL.GitHub.Scrapper.Business.Interfaces;
using RBL.GitHub.Scrapper.Business.Notifications;
using RBL.GitHub.Scrapper.Business.Services;
using RBL.GitHub.Scrapper.Data.EF.IRepositories;
using RBL.GitHub.Scrapper.Data.EF.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RBL.GitHub.Scrapper.API.Configuration
{
    public static class DependencyInjectionConfig
    {
        public static IServiceCollection ResolveDependencies(this IServiceCollection services)
        {
            services.AddHttpContextAccessor();

            AddNotifications(services);
            AddServicesRepositories(services);

            return services;
        }

        private static void AddServicesRepositories(this IServiceCollection services)
        {
            services.AddScoped<IScrapperService, ScrapperService>();
            services.AddScoped<IScrappingInfoRepository, ScrappingInfoRepository>();
            services.AddScoped<IScrappingInfoExtensionRepository, ScrappingInfoExtensionRepository>();
            services.AddScoped<IScrappingInfoService, ScrappingInfoService>();
            services.AddScoped<IScrappingInfoExtensionService, ScrappingInfoExtensionService>();
        }
        private static void AddNotifications(this IServiceCollection services)
        {
            services.AddScoped<INotifier, Notifier>();
        }
    }
}
