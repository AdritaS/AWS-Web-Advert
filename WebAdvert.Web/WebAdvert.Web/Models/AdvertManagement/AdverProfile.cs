using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebAdvert.Web.Models.AdvertManagement;
using WebAdvert.Web.Services.ServiceModels;

namespace AdvertAPI.Models
{
    public class AdverProfile:Profile
    {
        public AdverProfile()
        {
            CreateMap<CreateAdvertModel, AdvertModel>();
            CreateMap<CreateAdvertViewModel, CreateAdvertModel>();

        }
    }
}
