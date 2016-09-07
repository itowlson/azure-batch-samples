//Copyright (c) Microsoft Corporation

using Microsoft.Azure.Batch.Common;
using Microsoft.Azure.Batch.Protocol.BatchRequests;
using Microsoft.Azure.Batch.Test;
using Microsoft.Azure.Batch.UnitTestHelpers.Usage.Testee;
using Microsoft.Rest.Azure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Azure.Batch.UnitTestHelpers.Usage.Tests
{
    public class PoolCoordinatorTests
    {
        public class EnsureCapacityMethod
        {
            [Fact]
            public async Task IfPoolDoesNotExistThenItIsCreated()
            {
                var createdPools = new List<string>();

                using (BatchClient batchClient = BatchResourceFactory.CreateBatchClient())
                {
                    // Arrange
                    batchClient.OnRequest<PoolGetBatchRequest>(r => r.ServiceRequestFunc = x => { throw BatchServiceError.Simulate(HttpStatusCode.NotFound, BatchErrorCodeStrings.PoolNotFound); });
                    batchClient.OnRequest<PoolAddBatchRequest>(r => r.ServiceRequestFunc = x => { createdPools.Add(r.Parameters.Id); return Task.FromResult(new AzureOperationHeaderResponse<Protocol.Models.PoolAddHeaders>()); });

                    var poolCoordinator = new PoolCoordinator(batchClient);

                    // Act
                    await poolCoordinator.EnsureCapacity("new-pool", "A2", 40);

                    // Assert
                    Assert.Equal(1, createdPools.Count);
                    Assert.Equal("new-pool", createdPools.Single());
                }
            }
        }
    }
}
