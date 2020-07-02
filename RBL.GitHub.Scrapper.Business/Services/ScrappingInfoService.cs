using AutoMapper;
using CsQuery;
using CsQuery.ExtensionMethods;
using RBL.GitHub.Scrapper.Business.Interfaces;
using RBL.GitHub.Scrapper.Data.EF.IRepositories;
using RBL.GitHub.Scrapper.Domain;
using RBL.GitHub.Scrapper.ViewModels.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RBL.GitHub.Scrapper.Business.Services
{
    public class ScrappingInfoService : NotificationService, IScrappingInfoService
    {
        
        private readonly IMapper _mapper;
        private readonly IScrappingInfoRepository _scrappingInfoRepository;
        public ScrappingInfoService(INotifier notifier, IMapper mapper,
            IScrappingInfoRepository scrappingInfoRepository)
            : base(notifier)
        {
            _mapper = mapper;
            _scrappingInfoRepository = scrappingInfoRepository;
        }

        public async Task Delete(int id)
        {
            var obj = await _scrappingInfoRepository.GetByIdAsync(id);
            if (obj == null)
                throw new Exception($"Object (id={id}) not found");
            await _scrappingInfoRepository.DeleteAsync(obj);
        }

        public async Task<ScrappingInfoViewModel> Get(int id)
        {
            var obj = await _scrappingInfoRepository.GetByIdAsync(id);
            return _mapper.Map<ScrappingInfoViewModel>(obj);
        }

        public async Task<IEnumerable<ScrappingInfoViewModel>> GetAll()
        {
            var list = await _scrappingInfoRepository.GetAllAsync();
            
            var listVM = _mapper.Map<IEnumerable<ScrappingInfoViewModel>>(list);
            return listVM;
        }
    }
}
