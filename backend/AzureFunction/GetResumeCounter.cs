using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Cosmos;
using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace Company.Function
{
    public class GetResumeCounter
    {
        private readonly ILogger<GetResumeCounter> _logger;

        public GetResumeCounter(ILogger<GetResumeCounter> logger)
        {
            _logger = logger;
        }

        [Function("GetResumeCounter")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequest req)
        {
            var databaseName = "AzureResume";
            var containerName = "Counter";
            var documentId = "1";

            var cosmosClient = new CosmosClient(Environment.GetEnvironmentVariable("CosmosDBConnectionString",EnvironmentVariableTarget.Process));
            var database = cosmosClient.GetDatabase(databaseName);
            var container = database.GetContainer(containerName);
            _logger.LogInformation("C# HTTP trigger function processed a request.");
            try
            {
                var documentResponse = await container.ReadItemAsync<Counter>(documentId, new PartitionKey(documentId));
                var document = documentResponse.Resource;
                _logger.LogInformation("Document has been read from CosmosDB");

                document.Count++;

                var documentUpdateResponse = await container.ReplaceItemAsync(document, documentId, new PartitionKey(documentId));

                string jsonResponse = JsonSerializer.Serialize(documentUpdateResponse.Resource);

                return new OkObjectResult(jsonResponse);
            }
            catch (CosmosException ex)
            {
                _logger.LogError($"Errore durante l'aggiornamento dell'item: {ex.Message}");
                return new OkObjectResult($"{ex.Message}");
            }
        }
    }
}
