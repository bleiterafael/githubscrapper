
using Microsoft.EntityFrameworkCore;
using RBL.GitHub.Scrapper.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace RBL.GitHub.Scrapper.Data.EF.Context
{
    public class ScrapperContext : DbContext
    {
        public DbSet<ScrappingInfo> ScrappingInfos { get; set; }
        public DbSet<ScrappingInfoExtension> ScrappingInfoExtension { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            options.UseSqlite("Data Source=scrapping.db");
            options.UseLazyLoadingProxies();
        }
    }
}
