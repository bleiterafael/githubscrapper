using RBL.GitHub.Scrapper.Business.Notifications;
using RBL.GitHub.Scrapper.ViewModels.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace RBL.GitHub.Scrapper.Business.Interfaces
{
    public interface IScrappingInfoService
    {
        Task<ScrappingInfoViewModel> Get(int id);
        Task<IEnumerable<ScrappingInfoViewModel>> GetAll();
        Task Delete(int id);
    }
}
