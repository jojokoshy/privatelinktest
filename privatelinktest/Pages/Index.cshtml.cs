using Azure.Data.AppConfiguration;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Azure.Cosmos;
//using Azure.Identity;
//using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.Options;

namespace privatelinktest.Pages
{
    public class IndexModel : PageModel
    {
        const string cosmosDbUri = "YOUR_COSMOS_DB_URI";
        const string databaseName = "YourDatabaseName";
        const string containerName = "YourContainerName";
        public Settings Settings { get; }
        private readonly ILogger<IndexModel> _logger;
        string keyVaultName = "";
        public IndexModel(ILogger<IndexModel> logger, IOptionsSnapshot<Settings> options )
        {
            Settings = options.Value;
            _logger = logger;
            keyVaultName =  Environment.GetEnvironmentVariable("KEY_VAULT_NAME"); // "adme-we-kv";//
        }
        
        //method for looping through cosmos db
        public async Task TestAsync()
        {
            CosmosClient client = new Microsoft.Azure.Cosmos.CosmosClient(cosmosDbUri, new DefaultAzureCredential());
            Database database = await client.CreateDatabaseIfNotExistsAsync(databaseName);
            Container container = await database.CreateContainerIfNotExistsAsync(containerName, "/PartitionKey");

            QueryDefinition queryDefinition = new QueryDefinition("SELECT * FROM c");
            List<object> results = new List<object>();

            using (FeedIterator<object> resultSetIterator = container.GetItemQueryIterator<object>(queryDefinition))
            {
                while (resultSetIterator.HasMoreResults)
                {
                    FeedResponse<object> response = await resultSetIterator.ReadNextAsync();
                    results.AddRange(response);
                }
            }

            foreach (object item in results)
            {
                Console.WriteLine(item);
            }
        }

        private async Task<string> GetSecret()
        {
            var kvUri = "https://" + keyVaultName + ".vault.azure.net";

            var client = new SecretClient(new Uri(kvUri), new DefaultAzureCredential());
            KeyVaultSecret secret = await client.GetSecretAsync("TestSecret");

            return secret.Value;
        }
        private async void PushSecret()
        {
            
            var kvUri = "https://" + keyVaultName + ".vault.azure.net";

            var client = new SecretClient(new Uri(kvUri), new DefaultAzureCredential());
            //Insert a new secret into the Key Vault with the name of "TestSecret" 
            //and a value of "test secret value" 
            KeyVaultSecret secret = new KeyVaultSecret("TestSecret", "This is data from secret");
            await client.SetSecretAsync(secret);




        }

        //write a function to call a url and get the response back as a string 
        public async Task<string> GetResponse(string url)
        {
            HttpClient client = new HttpClient();
            HttpResponseMessage response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();
            string responseBody = await response.Content.ReadAsStringAsync();
            return responseBody;
        }
     
        public async Task<IActionResult> OnPostAsync(string url)
        {
            // Process the input here...
            Task<string> response = GetResponse(url);
            string results =  response.Result.Substring(0,30);
          //  callConfig();
            ViewData["Message"] = results;
            PushSecret();
            ViewData["Secret"] = await GetSecret();

            return Page();
           
        }

        public async Task OnGetAsync()
        {
            //callConfig();
            
        }

        private static void callConfig()
        {
            // Connect to Azure Application Configuration using Managed Identity
             
            


            // Sample data for writing to Cosmos DB
            dynamic dataToWrite = new
            {
                id = Guid.NewGuid().ToString(),
                PartitionKey = "SamplePartitionKey",
                Property1 = "Value1",
                Property2 = "Value2"
            };
        }

         
    }
}