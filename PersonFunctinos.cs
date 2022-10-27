namespace MCT.Functions
{
    public static class PersonFunctinos
    {
        [FunctionName("PersonFunctinos")]
        public static async Task<IActionResult> GetPersons(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "persons")] HttpRequest req,
            ILogger log)
        {
            try
            {
                var connectionString = Environment.GetEnvironmentVariable("CosmosDB");
                CosmosClientOptions options = new CosmosClientOptions()
                {
                    ConnectionMode = ConnectionMode.Gateway
                };

                CosmosClient client = new CosmosClient(connectionString, options);
                var container = client.GetContainer(General.COSMOS_DB, General.COSMOS_CONTAINER);

                string sql = "SELECT * FROM c";
                var iterator = container.GetItemQueryIterator<person>(sql);
                var result = new List<person>();
                while (iterator.HasMoreResults)
                {
                    var response = await iterator.ReadNextAsync();
                    result.AddRange(response.ToList());
                }

                return new OkObjectResult(result);
            }
            catch (System.Exception ex)
            {
                log.LogError(ex.Message);
                return new BadRequestObjectResult(ex.Message);
            }
        }




        [FunctionName("AddPerson")]
        public static async Task<IActionResult> AddPersons(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "persons")] HttpRequest req,
            ILogger log)
        {
            try
            {
                //body uitlezen en omzetten naar person object
                var json = await new StreamReader(req.Body).ReadToEndAsync();
                var person = JsonConvert.DeserializeObject<person>(json);

                //connectie maken met cosmosdb
                var connectionString = Environment.GetEnvironmentVariable("CosmosDB");

                CosmosClientOptions options = new CosmosClientOptions()
                {
                    ConnectionMode = ConnectionMode.Gateway
                };

                CosmosClient client = new CosmosClient(connectionString, options);
                var container = client.GetContainer(General.COSMOS_DB, General.COSMOS_CONTAINER);

                person.Id = Guid.NewGuid().ToString();
                await container.CreateItemAsync(person, new PartitionKey(person.Id));

                return new OkObjectResult(person);
            }
            catch (System.Exception ex)
            {
                log.LogError(ex.Message);
                return new BadRequestObjectResult(ex.Message);
            }
        }




        [FunctionName("DeletePerson")]
        public static async Task<IActionResult> DeletePersons(
            [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "person/{id}")] HttpRequest req, string id,
            ILogger log)
        {
            try
            {
                //connectie maken met cosmosdb
                var connectionString = Environment.GetEnvironmentVariable("CosmosDB");
                
                CosmosClientOptions options = new CosmosClientOptions()
                {
                    ConnectionMode = ConnectionMode.Gateway
                };

                CosmosClient client = new CosmosClient(connectionString, options);
                var container = client.GetContainer(General.COSMOS_DB, General.COSMOS_CONTAINER);
                await container.DeleteItemAsync<person>(id, new PartitionKey(id));

                return new OkObjectResult("Person deleted");
            }
            catch (System.Exception ex)
            {
                log.LogError(ex.Message);
                return new BadRequestObjectResult(ex.Message);
            }
        }



        [FunctionName("UpdatePerson")]
        public static async Task<IActionResult> UpdatePersons(
            [HttpTrigger(AuthorizationLevel.Anonymous, "PUT", Route = "person")] HttpRequest req,
            ILogger log)
        {
            try
            {
                //body uitlezen en omzetten naar person object
                var json = await new StreamReader(req.Body).ReadToEndAsync();
                var person = JsonConvert.DeserializeObject<person>(json);

                //connectie maken met cosmosdb
                var connectionString = Environment.GetEnvironmentVariable("CosmosDB");
                
                CosmosClientOptions options = new CosmosClientOptions()
                {
                    ConnectionMode = ConnectionMode.Gateway
                };

                CosmosClient client = new CosmosClient(connectionString, options);
                var container = client.GetContainer(General.COSMOS_DB, General.COSMOS_CONTAINER);

                await container.ReplaceItemAsync<person>(person, person.Id);

                return new OkObjectResult(person);
            }
            catch (System.Exception ex)
            {
                log.LogError(ex.Message);
                return new BadRequestObjectResult(ex.Message);
            }
        }
    }
}
