using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebAdvert.Web.Services.ServiceModels;

namespace WebAdvert.Web.Services
{
    public interface IAdvertApiClient
    {
        Task<AdvertResponse> CreateAsync(CreateAdvertModel model);
        Task<bool> ConfirmAsync(ConfirmAdvertRequest model);
        Task<List<Advertisement>> GetAllAsync();
        Task<Advertisement> GetAsync(string advertId);
    }
}
