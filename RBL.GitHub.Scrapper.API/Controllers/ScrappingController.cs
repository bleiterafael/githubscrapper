﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RBL.GitHub.Scrapper.Business.Interfaces;

namespace RBL.GitHub.Scrapper.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ScrappingController : MainController
    {
        private readonly IScrapperService _scrapperService;
        public ScrappingController(INotifier notifier,
            IScrapperService scrapperService)
            : base(notifier)
        {
            _scrapperService = scrapperService;
        }

        [HttpGet]
        public string Get()
        {
            return $"SCRAPPING GITHUB API - Rafael Batista Leite";
        }

        [HttpPost]
        public async Task<ActionResult> Post([FromForm] string gitHubRepository=null,[FromForm] bool navigateSubFolders=true)
        {
            try
            {
                HttpRequest req = this.Request;

                if (!ModelState.IsValid) return CustomResponse(ModelState);

                var scrappingInfo = await _scrapperService.ScrapeGitHub(gitHubRepository, navigateSubFolders);

                return CustomResponse(scrappingInfo);
            }
            catch (Exception error)
            {
                NotifyError(error.Message);
                return CustomResponse();
            }
            
        }
    }
}