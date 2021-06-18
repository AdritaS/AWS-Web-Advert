using Amazon.Lambda.Core;
using Amazon.Lambda.SNSEvents;
using Nest;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]
namespace WebAdvert.SearchWorker
{
    public class SearchWorker
    {
        private readonly IElasticClient _client;
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

                // aws config


                //                {
                //                    "Version": "2012-10-17",
                //  "Statement": [
                //    {
                //      "Effect": "Allow",
                //      "Principal": {
                //        "AWS": [
                //          "*"
                //        ]
                //    },
                //      "Action": [
                //        "es:*"
                //      ],
                //      "Resource": "arn:aws:es:ap-south-1:143753221730:domain/advertapi/*"
                //    }
                //  ]
                //}


            }
        }
    }
}
