using Microsoft.Azure.Batch.Protocol.BatchRequests;
using Microsoft.Azure.Batch.Test;
using Microsoft.Azure.Batch.UnitTestHelpers.Usage.Testee;
using Microsoft.Rest.Azure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Models = Microsoft.Azure.Batch.Protocol.Models;

namespace Microsoft.Azure.Batch.UnitTestHelpers.Usage.Tests
{
    public class JobCoordinatorTests
    {
        public class GetCompletedTaskCountAsyncMethod
        {
            [Fact]
            public async Task RequestsOnlyCompletedTasks()
            {
                using (BatchClient batchClient = BatchResourceFactory.CreateBatchClient())
                {
                    string filter = null;

                    var jobOperations = batchClient.JobOperations;

                    jobOperations.OnRequest<TaskListBatchRequest>(r => r.ServiceRequestFunc = _ =>
                    {
                        filter = r.Options.Filter;
                        return Task.FromResult(new AzureOperationResponse<IPage<Models.CloudTask>, Models.TaskListHeaders> { Body = DataPage.Empty<Models.CloudTask>() });
                    });

                    var jobCoordinator = new JobCoordinator(jobOperations);

                    await jobCoordinator.GetCompletedTaskCountAsync("someid");

                    Assert.Equal("state eq 'completed'", filter);
                }
            }

            [Fact]
            public async Task CountsReturnedTasks()
            {
                using (BatchClient batchClient = BatchResourceFactory.CreateBatchClient())
                {
                    var jobOperations = batchClient.JobOperations;

                    var fakeTasks = new List<Models.CloudTask>
                    {
                        new Models.CloudTask(),
                        new Models.CloudTask(),
                    };

                    jobOperations.OnRequest<TaskListBatchRequest>(r => r.ServiceRequestFunc = _ => Task.FromResult(new AzureOperationResponse<IPage<Models.CloudTask>, Models.TaskListHeaders> { Body = DataPage.Single(fakeTasks) }));

                    var jobCoordinator = new JobCoordinator(jobOperations);

                    var completedCount = await jobCoordinator.GetCompletedTaskCountAsync("someid");

                    Assert.Equal(2, completedCount);
                }
            }
        }
    }
}
