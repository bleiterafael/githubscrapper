using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using RBL.GitHub.Scrapper.Business.Interfaces;
using RBL.GitHub.Scrapper.Business.Notifications;
using RBL.GitHub.Scrapper.Business.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace RBL.GitHub.Scrapper.ScrappingTests
{
    public class Injection
    {
        public Injection()
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection
                .AddTransient<IMapper, Mapper>()
                .AddTransient<INotifier, Notifier>()
                .AddTransient<IScrapperService, ScrapperService>()
                ;

            ServiceProvider = serviceCollection.BuildServiceProvider();
        }

        public ServiceProvider ServiceProvider { get; private set; }
    }
}
