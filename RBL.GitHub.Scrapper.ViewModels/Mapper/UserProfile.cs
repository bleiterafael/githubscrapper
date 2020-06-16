using AutoMapper;
using RBL.GitHub.Scrapper.Domain;
using RBL.GitHub.Scrapper.ViewModels.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace RBL.GitHub.Scrapper.ViewModels.Mapper
{
    public class UserProfile : Profile
    {
        public UserProfile()
        {
            CreateMap<Extension, ExtensionViewModel>().ReverseMap();
            CreateMap<GitHubItem, GitHubItemViewModel>().ReverseMap();
            CreateMap<GitHubFile, GitHubFileViewModel>().ReverseMap();
            CreateMap<GitHubFolder, GitHubFolderViewModel>().ReverseMap();
        }
    }
}
