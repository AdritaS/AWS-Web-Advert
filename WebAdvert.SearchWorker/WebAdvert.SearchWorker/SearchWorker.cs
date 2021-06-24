using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AdvertAPI.Models.Messages;
using Amazon.Lambda.Core;
using Amazon.Lambda.SNSEvents;
using Nest;
using Newtonsoft.Json;
using WebAdvert.SearchWorker.Helpers;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace WebAdvert.SearchWorker
{
    public class SearchWorker
    {
        private readonly IElasticClient _client;

        // This always calls the next constructor and singleton instance of Elastic client is used
        // This is the replacement of AddSingleton
       // public SearchWorker() : this(ElasticSearchHelper.GetInstance(ConfigurationHelper.Instance))
        public SearchWorker() : this(ElasticSearchHelper.GetInstance())
        {

        }
        public SearchWorker(IElasticClient client)
        {
            _client = client;
        }

        /// <summary>
        /// Creates a new document in Elastic Search whenever it gets a message from SNS
        /// </summary>
        /// <param name="snsEvent"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task FunctionHandler(SNSEvent snsEvent, ILambdaContext context)
        {
            context.Logger.LogLine($"snsEvent: {snsEvent != null}");
            context.Logger.LogLine($"count: {snsEvent.Records.Count}");

            foreach (var record in snsEvent.Records)
            {
                context.Logger.LogLine(record.Sns.Message);

                var message = JsonConvert.DeserializeObject<AdvertConfirmedMessage>(record.Sns.Message);
                var advertDocument = MappingHelper.Map(message);
                try
                {
                    await _client.IndexDocumentAsync(advertDocument);
                }
                catch(Exception ex)
                {
                    context.Logger.LogLine($"Exception: {ex.Message}, {ex.InnerException}, {ex.StackTrace}");
                }

            }
        }
    }
}
