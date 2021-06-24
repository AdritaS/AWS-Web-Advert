using Microsoft.Extensions.Configuration;
using Nest;
using System;
using System.Collections.Generic;
using System.Text;
using WebAdvert.SearchWorker.Models;

namespace WebAdvert.SearchWorker
{
    public static class ElasticSearchHelper
    {
        private static IElasticClient _client;

        //public static IElasticClient GetInstance(IConfiguration config)
        public static IElasticClient GetInstance()

        {
            if (_client == null)
            {
                // var url = config.GetSection("ES").GetValue<string>("url");
                var url = "https://WebAdvertUsers:Test$1234@search-advertisement-5hh5fnp54jmarpmgfekggmwzwa.us-east-1.es.amazonaws.com/";

                var settings = new ConnectionSettings(new Uri(url))
                    .DefaultIndex("adverts")
                    .DefaultMappingFor<AdvertType>(m => m.IdProperty(x => x.Id));
                _client = new ElasticClient(settings);
            }

            return _client;
        }
    }
}
