### ApplicationPackages

The ApplicationPackages sample projects demonstrate usage of the [Batch Management .NET][net_mgmt_api] and
[Batch .NET][net_batch_api] libraries to use Azure Batch application packages.  They show you how to:

* Programmatically install an application package into your Batch account
* Run programs from an application package in a task

NOTE: To run the management application successfully, you must first register it with Azure Active Directory using the Azure Management Portal. See [Integrating Applications with Azure Active Directory][aad_integrate] for more information.

#### Creating an application package

Normally you would need to create an application package containing your program executables
and dependencies. To do this simply create a zip file containing all the files you want in
the package.

For this sample, however, we will use a pre-created application package containing the Windows
`tree` program. This is purely for demonstration purposes; there's no point creating a package
containing a Windows built-in program for real purposes! You can find the pre-created package
in the SamplePackage directory.

#### ApplicationPackageManagement project

The management sample application shows how to programmatically install an application package
into your Batch account. (You can also install application packages via the Azure portal, or
using the Azure Batch PowerShell cmdlets.)  It demonstrates the following operations:

1. Acquire security token from Azure Active Directory (AAD) using [ADAL][aad_adal]
2. Create a credentials object associated with the an Azure subscription
3. Create a [BatchManagementClient][net_batchclient] using the new credentials
4. Use the [BatchManagementClient][net_batchclient] to create an application package
5. Use the [Azure Storage client library][net_storage_library] to upload the application package contents
6. Use the [BatchManagementClient][net_batchclient] to activate the application package, making it available for use in tasks

For more information on using the [Batch Management .NET][net_mgmt_api] library, please see the following article:

[Manage Azure Batch accounts and quotas with Batch Management .NET][acom_acct_article]

[aad_adal]: https://azure.microsoft.com/documentation/articles/active-directory-authentication-libraries/
[aad_integrate]: https://azure.microsoft.com/documentation/articles/active-directory-integrating-applications/
[acom_acct_article]: https://azure.microsoft.com/documentation/articles/batch-management-dotnet/
[net_batchclient]: https://msdn.microsoft.com/library/azure/microsoft.azure.management.batch.batchmanagementclient.aspx
[net_mgmt_api]: https://msdn.microsoft.com/library/azure/mt463120.aspx
[net_batch_api]: https://msdn.microsoft.com/library/mt348682.aspx
[net_mgmt_nuget]: https://www.nuget.org/packages/Microsoft.Azure.Management.Batch/
[net_resclient]: https://msdn.microsoft.com/library/azure/microsoft.azure.management.resources.resourcemanagementclient.aspx
[net_storage_library]: https://msdn.microsoft.com/en-us/library/azure/mt347887.aspx
[net_subclient]: https://msdn.microsoft.com/library/azure/microsoft.azure.subscriptions.subscriptionclient.aspx
