using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NUnit.Framework;
using RBL.GitHub.Scrapper.Business.Interfaces;
using RBL.GitHub.Scrapper.Business.Notifications;
using RBL.GitHub.Scrapper.Business.Services;
using System.Collections.Generic;

namespace RBL.GitHub.Scrapper.Tests
{
    [TestClass]
    public class ScrappingTests
    {
        private ServiceProvider serviceProvider { get; set; }
        [SetUp]
        public void SetUp()
        {
            var services = new ServiceCollection();
            services.AddTransient<IScrapperService, ScrapperService>();
            services.AddTransient<INotifier, Notifier>();
            services.AddTransient<IMapper, Mapper>();
            services.AddTransient<string>();

            serviceProvider = services.BuildServiceProvider();
        }


        //private readonly IScrapperService _scrapperService = ;
        [TestMethod]
        public void TestScrappingInvalidRepository()
        {
            this.SetUp();
            string gitHubRepository = "";
            var scrapperService = serviceProvider.GetService<ScrapperService>();
            var result = scrapperService.ScrapeGitHub(gitHubRepository);
            NUnit.Framework.Assert.False(result == null, "Undefined repository");
        }
    }
}
