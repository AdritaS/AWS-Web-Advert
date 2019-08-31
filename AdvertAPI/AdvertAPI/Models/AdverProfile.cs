using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AdvertAPI.Models
{
    public class AdverProfile:Profile
    {
        public AdverProfile()
        {
            CreateMap<AdvertModel, AdvertModelDb>();
        }
    }
}
