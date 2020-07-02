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
    public class ScrappingInfoExtensionService : NotificationService, IScrappingInfoExtensionService
    {
        
        private readonly IMapper _mapper;
        private readonly IScrappingInfoExtensionRepository _scrappingInfoExtensionsRepository;
        public ScrappingInfoExtensionService(INotifier notifier, IMapper mapper,
            IScrappingInfoExtensionRepository scrappingInfoExtensionRepository)
            : base(notifier)
        {
            _mapper = mapper;
            _scrappingInfoExtensionsRepository = scrappingInfoExtensionRepository;
        }

        public async Task Delete(int id)
        {
            var obj = await _scrappingInfoExtensionsRepository.GetByIdAsync(id);
            if (obj == null)
                throw new Exception($"Object (id={id}) not found");
            await _scrappingInfoExtensionsRepository.DeleteAsync(obj);
        }

        public async Task<ScrappingInfoExtensionViewModel> Get(int id)
        {
            var obj = await _scrappingInfoExtensionsRepository.GetByIdAsync(id);
            return _mapper.Map<ScrappingInfoExtensionViewModel>(obj);
        }

        public async Task<IEnumerable<ScrappingInfoExtensionViewModel>> GetAll()
        {
            var list = await _scrappingInfoExtensionsRepository.GetAllAsync();
            var listVM = _mapper.Map<IEnumerable<ScrappingInfoExtensionViewModel>>(list);
            return listVM;
        }
    }
}
