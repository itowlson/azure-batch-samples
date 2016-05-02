using Microsoft.Azure.Batch.Auth;
using Microsoft.Azure.Batch.Common;
using Microsoft.Azure.Batch.Samples.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Azure.Batch.Samples.ApplicationPackageUsage
{
    class Program
    {
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
            // You may adjust these values to experiment with different compute resource scenarios.
            const string nodeSize = "small";
            const string osFamily = "4";
            const int nodeCount = 1;

            const string poolId = "ApplicationPackagesSamplePool";
            const string jobId = "ApplicationPackagesSampleJob";

            // Amount of time to wait before timing out long-running tasks.
            TimeSpan timeLimit = TimeSpan.FromMinutes(30);

            // Set up access to your Batch account with a BatchClient. Configure your AccountSettings in the
            // Microsoft.Azure.Batch.Samples.Common project within this solution.
            BatchSharedKeyCredentials cred = new BatchSharedKeyCredentials(AccountSettings.Default.BatchServiceUrl,
                                                                           AccountSettings.Default.BatchAccountName,
                                                                           AccountSettings.Default.BatchAccountKey);

            using (BatchClient batchClient = await BatchClient.OpenAsync(cred))
            {
                // Create the pool with an application package reference.
                Console.WriteLine("Creating pool [{0}]...", poolId);
                CloudPool unboundPool =
                    batchClient.PoolOperations.CreatePool(poolId: poolId,
                                                          cloudServiceConfiguration: new CloudServiceConfiguration(osFamily),
                                                          virtualMachineSize: nodeSize,
                                                          targetDedicated: nodeCount);
                unboundPool.ApplicationPackageReferences.Add(
                    new ApplicationPackageReference { ApplicationId = "tree", Version = "1.0" }
                );
                await unboundPool.CommitAsync();

                // Create the job and specify that it uses tasks dependencies.
                Console.WriteLine("Creating job [{0}]...", jobId);
                CloudJob unboundJob = batchClient.JobOperations.CreateJob(jobId,
                    new PoolInformation { PoolId = poolId });
                await unboundJob.CommitAsync();

                // Create a task that uses a program from the application package.
                var task = new CloudTask("printtree", "cmd.exe /c %AZ_BATCH_APP_PACKAGE_TREE#1.0\\tree.com");

                // Add the task to the job.
                await batchClient.JobOperations.AddTaskAsync(jobId, task);

                // Pause execution while we wait for the tasks to complete, and notify
                // whether the tasks completed successfully.
                Console.WriteLine("Waiting for task completion...");
                Console.WriteLine();
                CloudJob job = await batchClient.JobOperations.GetJobAsync(jobId);
                if (await batchClient.Utilities.CreateTaskStateMonitor().WhenAllAsync(
                        job.ListTasks(),
                        TaskState.Completed,
                        timeLimit))
                {
                    Console.WriteLine("Operation timed out while waiting for submitted tasks to reach state {0}",
                        TaskState.Completed);
                }
                else
                {
                    Console.WriteLine("All tasks completed successfully.");
                    Console.WriteLine();
                }

                // Clean up the resources we've created in the Batch account
                Console.Write("Delete job? [yes] no: ");
                string response = Console.ReadLine().ToLower();
                if (response != "n" && response != "no")
                {
                    await batchClient.JobOperations.DeleteJobAsync(job.Id);
                }

                Console.Write("Delete pool? [yes] no: ");
                response = Console.ReadLine().ToLower();
                if (response != "n" && response != "no")
                {
                    await batchClient.PoolOperations.DeletePoolAsync(poolId);
                }
            }
        }
    }
}
