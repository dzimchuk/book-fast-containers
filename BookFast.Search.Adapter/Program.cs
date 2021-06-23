using System;
using Microsoft.Extensions.Configuration;
using System.IO;
using Azure.Search.Documents.Indexes;
using Azure;

namespace BookFast.Search.Adapter
{
    public class Program
    {
        public static void Main(string[] args)
        {
            if (args.Length > 0 && args[0].Equals("provision", StringComparison.OrdinalIgnoreCase))
            {
                Provision();
            }
            else
            {
                Console.WriteLine("Usage: dotnet run provision");
            }
        }

        private static void Provision()
        {
            var builder = new ConfigurationBuilder();
            builder.SetBasePath(Directory.GetCurrentDirectory());
            builder.AddUserSecrets<Program>();

            var configuration = builder.Build();

            var endpoint = new Uri(configuration["Search:ServiceEndpoint"]);
            var credential = new AzureKeyCredential(configuration["Search:AdminKey"]);
            var searchServiceClient = new SearchIndexClient(endpoint, credential);

            var index = new BookFastIndex(searchServiceClient, configuration);
            index.ProvisionAsync().Wait();
        }
    }
}
