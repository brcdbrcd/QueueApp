using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.Extensions.Configuration;

namespace QueueApp
{
    class Program
    {
        private static string ConnectionString = GetConnection(); 
        
        private static string GetConnection()
        {
            var builder = new ConfigurationBuilder()
                                .SetBasePath(Directory.GetCurrentDirectory())
                                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
 
            string ConnectionString = builder.Build().GetConnectionString("myStorageAccount");
 
            return ConnectionString;
        }

        static async Task Main(string[] args)
        {
            if (args.Length > 0)
            {
                string value = String.Join(" ", args);
                await SendArticleAsync(value);
                Console.WriteLine($"Sent: {value}");
            }
            else 
            {
                string value = await ReceiveArticleAsync();
                Console.WriteLine($"Received {value}");
            }
        }

        static async Task SendArticleAsync(string newsMessage)
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(ConnectionString);

            CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();

            CloudQueue queue = queueClient.GetQueueReference("newsqueue");
            bool createdQueue = await queue.CreateIfNotExistsAsync();
            if(createdQueue)
            {
                Console.WriteLine("The queue of news articles was created.");
            }

            CloudQueueMessage articleMessage = new CloudQueueMessage(newsMessage);
            await queue.AddMessageAsync(articleMessage);
        }

        static async Task<string> ReceiveArticleAsync()
        {
            CloudQueue queue = GetQueue();
            bool exists = await queue.ExistsAsync();
            if (exists)
            {
                CloudQueueMessage retrievedArticle = await queue.GetMessageAsync();
                if (retrievedArticle != null)
                {
                    string newsMessage = retrievedArticle.AsString;
                    await queue.DeleteMessageAsync(retrievedArticle);
                    return newsMessage;
                }
            }

            return "<queue empty or not created>";
        }

        static CloudQueue GetQueue()
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(ConnectionString);

            CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();
            return queueClient.GetQueueReference("newsqueue");
        }
    }
}