using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AdvertAPI.Models;
using AdvertAPI.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AdvertAPI.Controllers
{
    [Route("api/adverts")]
    [ApiController]
    public class AdvertController : ControllerBase
    {
        private readonly IAdvertStorageService _advertStorageService;
        public AdvertController(IAdvertStorageService advertStorageService)
        {
            _advertStorageService = advertStorageService;
        }
        [HttpPost]
        [Route("create")]
        public async Task<IActionResult> Create(AdvertModel model)
        {
            string recordId = null;
            try
            {
                recordId = await _advertStorageService.Add(model);
            }
            catch(KeyNotFoundException)
            {
                return new NotFoundResult();
            }
            catch(Exception ex)
            {
                return StatusCode(500, ex.Message);

            }
            return StatusCode(201, new AdvertResponse { Id = recordId });
        }

        [HttpPut]
        [Route("confirm")]
        public async Task<IActionResult> Confirm(ConfirmAdvertModel model)
        {
            string recordId = null;
            try
            {
                await _advertStorageService.Confirm(model);
            }
            catch (KeyNotFoundException keyEx)
            {
                return new NotFoundResult();
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);

            }
            return new OkResult();
        }
    }
}