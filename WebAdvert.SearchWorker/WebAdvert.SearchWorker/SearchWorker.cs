using AdvertAPI.Models.Messages;
using Amazon.Lambda.Core;
using Amazon.Lambda.SNSEvents;
using Nest;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using WebAdvert.SearchWorker.Helpers;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]
namespace WebAdvert.SearchWorker
{
    public class SearchWorker
    {
        private readonly IElasticClient _client;

        // This always calls the next constructor and singleton instance of Elastic client is used
        // This is the replacement of AddSingleton
        public SearchWorker() : this(ElasticSearchHelper.GetInstance(ConfigurationHelper.Instance))
        {

        }
        public SearchWorker(IElasticClient client)
        {
            _client = client;
        }
        public async Task Function(SNSEvent snsEvent, ILambdaContext context)
        {

            foreach (var record in snsEvent.Records)
            {
                context.Logger.LogLine(record.Sns.Message);

                var message = JsonConvert.DeserializeObject<AdvertConfirmedMessage>(record.Sns.Message);
                var advertDocument = MappingHelper.Map(message);
                await _client.IndexDocumentAsync(advertDocument);

            }
        }
    }
}
