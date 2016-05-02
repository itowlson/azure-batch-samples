// Copyright (c) Microsoft Corporation

namespace Microsoft.Azure.Batch.Samples.ApplicationPackageManagement
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.Azure;
    using Microsoft.Azure.Batch.Samples.Common;
    using Microsoft.Azure.Management.Batch;
    using Microsoft.Azure.Management.Batch.Models;
    using Microsoft.Azure.Management.Resources;
    using Microsoft.Azure.Management.Resources.Models;
    using Microsoft.Azure.Subscriptions;
    using Microsoft.Azure.Subscriptions.Models;
    using Microsoft.IdentityModel.Clients.ActiveDirectory;
    using Microsoft.WindowsAzure.Storage.Blob;
    using System.IO;

    public class BatchApplicationPackageManagementSample
    {
        // This sample uses the Active Directory Authentication Library (ADAL) to discover
        // subscriptions in your account and obtain TokenCloudCredentials required by the
        // Batch Management and Resource Management clients. It then creates a Resource
        // Group, performs Batch account operations, and then deletes the Resource Group.

        // These endpoints are used during authentication and authorization with AAD.
        private const string AuthorityUri = "https://login.microsoftonline.com/common"; // Azure Active Directory "common" endpoint
        private const string ResourceUri  = "https://management.core.windows.net/";     // Azure service management resource

        // The URI to which Azure AD will redirect in response to an OAuth 2.0 request. This value is
        // specified by you when you register an application with AAD (see ClientId comment). It does not
        // need to be a real endpoint, but must be a valid URI (e.g. https://accountmgmtsampleapp).
        private const string RedirectUri = "[specify-your-redirect-uri-here]";

        // Specify the unique identifier (the "Client ID") for your application. This is required so that your
        // native client application (i.e. this sample) can access the Microsoft Azure AD Graph API. For information
        // about registering an application in Azure Active Directory, please see "Adding an Application" here:
        // https://azure.microsoft.com/documentation/articles/active-directory-integrating-applications/
        private const string ClientId = "[specify-your-client-id-here]";

        // The name of your Batch account.
        private const string BatchAccountName = "[specify-your-batch-account-here]";

        // The name of the Resource Group containing your Batch account.
        private const string ResourceGroupName = "[specify-your-resource-group-here]";

        // The ID of the Azure subscription containing your Batch account.
        private const string SubscriptionId = "[specify-your-subscription-id-here]";

        // The application ID and version
        private const string SampleApplicationId = "tree";
        private const string SampleApplicationVersion = "1.0";
        private const string SamplePackagePath = "tree.zip";

        public static void Main(string[] args)
        {
            try
            {
                // Call the asynchronous version of the Main() method. This is done so that we can await various
                // calls to async methods within the "Main" method of this console application.
                MainAsync().Wait();
            }
            catch (AggregateException ae)
            {
                Console.WriteLine();
                Console.WriteLine("One or more exceptions occurred.");
                Console.WriteLine();

                SampleHelpers.PrintAggregateException(ae.Flatten());
            }
            finally
            {
                Console.WriteLine();
                Console.WriteLine("Sample complete, hit ENTER to exit...");
                Console.ReadLine();
            }
        }

        private static async Task MainAsync()
        {
            // Obtain an access token using the "common" AAD resource. This allows the application
            // to query AAD for information that lies outside the application's tenant (such as for
            // querying subscription information in your Azure account).
            AuthenticationContext authContext = new AuthenticationContext(AuthorityUri);
            AuthenticationResult authResult = authContext.AcquireToken(ResourceUri,
                                                                       ClientId,
                                                                       new Uri(RedirectUri),
                                                                       PromptBehavior.Auto);

            // Create credentials for accessing the Azure subscription using the access token.
            TokenCloudCredentials creds = new TokenCloudCredentials(SubscriptionId, authResult.AccessToken);

            using (BatchManagementClient batchManagementClient = new BatchManagementClient(creds))
            {
                // Create an application package record.  Initially the package is in 'pending' state
                // and has no content associated with it, and cannot be used in tasks.
                var package = await batchManagementClient.Applications.AddApplicationPackageAsync(
                    ResourceGroupName,
                    BatchAccountName,
                    SampleApplicationId,
                    SampleApplicationVersion
                    );

                // The Add Application Package call assigns a URL in blob storage to the package.
                // Upload the package file to this URL.
                var packageStorageUrl = new Uri(package.StorageUrl);
                var packageStorageBlob = new CloudBlockBlob(packageStorageUrl);
                await packageStorageBlob.UploadFromFileAsync(SamplePackagePath, FileMode.Open);

                // Now that the package content has been uploaded, activate the package, making it available
                // for use in tasks.
                await batchManagementClient.Applications.ActivateApplicationPackageAsync(
                    ResourceGroupName,
                    BatchAccountName,
                    SampleApplicationId,
                    SampleApplicationVersion,
                    new ActivateApplicationPackageParameters { Format = "zip" }
                    );
            }
        }
    }
}
