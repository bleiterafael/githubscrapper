using Microsoft.EntityFrameworkCore;
using RBL.GitHub.Scrapper.Data.EF.Context;
using RBL.GitHub.Scrapper.Data.EF.IRepositories;
using RBL.GitHub.Scrapper.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace RBL.GitHub.Scrapper.Data.EF.Repositories
{
    public class ScrappingInfoExtensionRepository : RepositoryBase<ScrappingInfoExtension>, IScrappingInfoExtensionRepository
    {
        
    }
}
