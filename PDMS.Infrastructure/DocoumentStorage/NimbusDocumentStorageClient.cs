using Microsoft.Extensions.Configuration;
using PDMS.Application.Interfaces;
using PDMS.Domain.Entity;
using RestSharp;
using Newtonsoft.Json;

namespace PDMS.Infrastructure.Authentication
{
    public class NimbusDocumentStorageClient : IDocumentStorageClient
    {
        RestClient client;
        private const string StorageProviderSource = "Nimbus Broker";

        public NimbusDocumentStorageClient(IConfiguration configuration)
        {
            client = new RestClient(configuration.GetValue<string>("NimbusBroker:BaseUrl"));
            client.RemoteCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true;
        }

        public async Task<DocumentStorageResult> Get(string identifer)
        {
            try
            {
                var request = new RestRequest($"/SecureStorage/{identifer}", Method.GET);
                request.AddParameter("includeMetadata", true);
                request.AddParameter("includePayload", true);
                var response = client.Execute(request);
                var myDeserializedResponse = JsonConvert.DeserializeObject<dynamic>(response.Content);

                return new DocumentStorageResult()
                {
                    Source = StorageProviderSource,
                    Identifier = myDeserializedResponse.retrievedFromEndpoint,
                    ErrorMessage = null,
                    Payload = myDeserializedResponse.payload
                };
            }
            catch (Exception ex)
            {
                return new DocumentStorageResult()
                {
                    Source = StorageProviderSource,
                    Identifier = identifer,
                    ErrorMessage = ex.Message,
                    Payload = null
                };
            }
        }

        public async Task<DocumentStorageResult> Post(string payload)
        {
            try
            {
                var request = new RestRequest($"/SecureStorage", Method.POST);
                request.RequestFormat = DataFormat.Json;
                request.AddJsonBody(new
                {
                    payload = payload
                });

                var response = client.Execute(request);

                var DeserializedResponse = JsonConvert.DeserializeObject<dynamic>(response.Content);

                return new DocumentStorageResult()
                {
                    Source = StorageProviderSource,
                    Identifier = DeserializedResponse.identifer,
                    ErrorMessage = null,
                    Payload = null
                };
            }
            catch (Exception ex)
            {
                return new DocumentStorageResult()
                {
                    Source = StorageProviderSource,
                    Identifier = "identifer",
                    ErrorMessage = ex.Message,
                    Payload = null
                };
            }
        }
    }
}
