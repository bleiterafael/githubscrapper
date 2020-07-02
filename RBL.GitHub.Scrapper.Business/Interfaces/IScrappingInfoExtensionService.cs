using RBL.GitHub.Scrapper.Business.Notifications;
using RBL.GitHub.Scrapper.ViewModels.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace RBL.GitHub.Scrapper.Business.Interfaces
{
    public interface IScrappingInfoExtensionService
    {
        Task<ScrappingInfoExtensionViewModel> Get(int id);
        Task<IEnumerable<ScrappingInfoExtensionViewModel>> GetAll();
        Task Delete(int id);
    }
}
