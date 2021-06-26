using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebAdvert.SearchApi.Models;

namespace WebAdvert.SearchApi.Services
{
    public class SearchService : ISearchService
    {
        private readonly IElasticClient _client;
        public SearchService(IElasticClient client)
        {
            _client = client;
        }

        public async Task<List<AdvertType>> Search(string keyword)
        {
            Console.WriteLine(DateTime.Now.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss") + ", keyword: " + keyword);
            var searchResponse = await _client.SearchAsync<AdvertType>(search => search.
                Query(query => query.
                    Term(field => field.Title, keyword.ToLower())
                ));
             Console.WriteLine("DebugInformation: " + searchResponse.DebugInformation);
            return searchResponse.Hits.Select(hit => hit.Source).ToList();
        }
    }
}
