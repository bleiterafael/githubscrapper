using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using RBL.GitHub.Scrapper.API.Controllers;
using RBL.GitHub.Scrapper.Business.Interfaces;
using RBL.GitHub.Scrapper.Business.Notifications;
using RBL.GitHub.Scrapper.Business.Services;
using System;
using Xunit;


namespace RBL.GitHub.Scrapper.ScrappingTests
{
    public class ScrappingTests : IClassFixture<Injection>
    {
        private ServiceProvider _serviceProvider;
        private ScrappingController _controller;

        public ScrappingTests(Injection injection)
        {
            _serviceProvider = injection.ServiceProvider;
            var notifier = _serviceProvider.GetService<INotifier>();
            var mapper = _serviceProvider.GetService<IMapper>();
            var scrapperService = _serviceProvider.GetService<IScrapperService>();
            _controller = new ScrappingController(notifier, scrapperService);
        }



        
        [Fact]
        public void TestScrappingInvalidRepository()
        {
            string gitHubRepository = "";

            // Act
            var okResult = _controller.Post(gitHubRepository);

            // Assert
            Assert.IsType<OkObjectResult>(okResult.Result);
        }
    }
}
