using System;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace ManageStorageTiers
{
    class Program
    {
        static void Main(string[] args)
        {
            DoWorkAsync().GetAwaiter().GetResult();

            Console.WriteLine("Press any key to exit the sample application.");
            Console.ReadLine();
        }

        static async Task DoWorkAsync()
        {
            CloudBlobClient cloudBlobClient = null;
            CloudBlobContainer cloudBlobContainer = null;

            // Connect to the user's storage account and create a reference to the blob container holding the sample blobs
            try
            {
                Console.WriteLine("Connecting to blob storage");
                string storageAccountConnectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING");
                string blobContainerName = Environment.GetEnvironmentVariable("CONTAINER_NAME");

                CloudStorageAccount cloudStorageAccount = CloudStorageAccount.Parse(storageAccountConnectionString);
                cloudBlobClient = cloudStorageAccount.CreateCloudBlobClient();
                cloudBlobContainer = cloudBlobClient.GetContainerReference(blobContainerName);
                Console.WriteLine("Connected");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to connect to storage account or container: {ex.Message}");
                return;
            }

            // Display the details of the blobs, including the storage tier
            await ManageBlobMetadata(cloudBlobContainer);
        }
        private static async Task ManageBlobMetadata(CloudBlobContainer cloudBlobContainer)
        {
            try
            {
                // Find the details, including the tier level, for each blob
                Console.WriteLine("Fetching the details of all blobs");
                BlobContinuationToken blobContinuationToken = null;
                do
                {
                    // Fetch container attributes in order to populate the container's properties and metadata.
                    await cloudBlobContainer.FetchAttributesAsync();

                    // Enumerate the container's metadata.
                    Console.WriteLine("Container metadata:");
                    foreach (var metadataItem in cloudBlobContainer.Metadata)
                    {
                        Console.WriteLine("\tKey: {0}", metadataItem.Key);
                        Console.WriteLine("\tValue: {0}", metadataItem.Value);
                    }
                    Console.WriteLine("Blob metadata:");
                    var results = await cloudBlobContainer.ListBlobsSegmentedAsync(null, blobContinuationToken);
                    blobContinuationToken = results.ContinuationToken;
                    foreach (CloudBlockBlob item in results.Results)
                    {

                        //the new package supports syncronous method
                        await item.FetchAttributesAsync();

                        Console.WriteLine("\tName: {0}", item.Name);

                        foreach (var metadataItem in item.Metadata)
                        {
                            Console.WriteLine("\tKey: {0}", metadataItem.Key);
                            Console.WriteLine("\tValue: {0}", metadataItem.Value);
                        }
                    }
                } while (blobContinuationToken != null);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to retrieve details of blobs: {ex.Message}");
                return;
            }
        }
    }
}
