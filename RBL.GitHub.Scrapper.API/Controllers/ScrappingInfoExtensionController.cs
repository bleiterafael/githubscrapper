using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RBL.GitHub.Scrapper.Business.Interfaces;

namespace RBL.GitHub.Scrapper.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ScrappingInfoExtensionController : MainController
    {
        private readonly IScrappingInfoExtensionService _scrappingService;
        public ScrappingInfoExtensionController(INotifier notifier,
            IScrappingInfoExtensionService scrappingService)
            : base(notifier)
        {
            _scrappingService = scrappingService;
        }

        [HttpGet("[action]/{id}")]
        public async Task<ActionResult> Get(int id)
        {
            try
            {
                var info = await _scrappingService.Get(id);
                return CustomResponse(info);
            }
            catch (Exception error)
            {
                NotifyError(error.Message);
                return CustomResponse();
            }
        }

        [HttpGet("[action]")]
        public async Task<ActionResult> GetAll()
        {
            try
            {
                var list = await _scrappingService.GetAll();
                return CustomResponse(list);
            }
            catch (Exception error)
            {
                NotifyError(error.Message);
                return CustomResponse();
            }
        }

        [HttpDelete("[action]/{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            try
            {
                await _scrappingService.Delete(id);
                return CustomResponse($"Object (id={id}) deleted");
            }
            catch (Exception error)
            {
                NotifyError(error.Message);
                return CustomResponse();
            }
        }
    }
}