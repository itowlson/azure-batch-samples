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
            public async Task IfPoolDoesNotExistThenItIsCreated_TestedUsingRawServiceRequestFunc()
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

            [Fact]
            public async Task IfPoolDoesNotExistThenItIsCreated_TestedUsingConvenienceMethods()
            {
                var createdPools = new List<string>();

                using (BatchClient batchClient = BatchResourceFactory.CreateBatchClient())
                {
                    // Arrange
                    batchClient.OnRequest<PoolGetBatchRequest>(r => r.Throw(BatchServiceError.Simulate(HttpStatusCode.NotFound, BatchErrorCodeStrings.PoolNotFound)));
                    batchClient.OnRequest<PoolAddBatchRequest>(r => r.Return(() => { createdPools.Add(r.Parameters.Id); return new AzureOperationHeaderResponse<Protocol.Models.PoolAddHeaders>(); }));

                    var poolCoordinator = new PoolCoordinator(batchClient);

                    // Act
                    await poolCoordinator.EnsureCapacity("new-pool", "A2", 40);

                    // Assert
                    Assert.Equal(1, createdPools.Count);
                    Assert.Equal("new-pool", createdPools.Single());
                }
            }

            [Fact]
            public async Task IfPoolDoesNotExistThenItIsCreated_TestedUsingCaptureHelper()
            {
                var createdPools = new List<string>();

                using (BatchClient batchClient = BatchResourceFactory.CreateBatchClient())
                {
                    // Arrange
                    batchClient.OnRequest<PoolGetBatchRequest>(r => r.Throw(BatchServiceError.Simulate(HttpStatusCode.NotFound, BatchErrorCodeStrings.PoolNotFound)));
                    batchClient.OnRequest<PoolAddBatchRequest>(r => r.Capture(r.Parameters.Id, createdPools));

                    var poolCoordinator = new PoolCoordinator(batchClient);

                    // Act
                    await poolCoordinator.EnsureCapacity("new-pool", "A2", 40);
                    await poolCoordinator.EnsureCapacity("newer-pool", "A2", 40);

                    // Assert
                    Assert.Equal(2, createdPools.Count);
                    Assert.Equal("new-pool", createdPools[0]);
                    Assert.Equal("newer-pool", createdPools[1]);
                }
            }

            [Fact]
            public async Task IfExistsThenItIsNotRecreated()
            {
                using (BatchClient batchClient = BatchResourceFactory.CreateBatchClient())
                {
                    // Arrange
                    batchClient.OnRequest<PoolGetBatchRequest>(r => r.Return(new Protocol.Models.CloudPool { TargetDedicated = 40 }));
                    batchClient.OnRequest<PoolAddBatchRequest>(r => r.Throw(new InvalidOperationException("Expected pool not to be created, but pool was created")));
                    batchClient.OnRequest<PoolResizeBatchRequest>(r => r.Throw(new InvalidOperationException("Expected pool not to be resized, but pool was resized")));

                    var poolCoordinator = new PoolCoordinator(batchClient);

                    // Act
                    await poolCoordinator.EnsureCapacity("new-pool", "A2", 40);

                    // Nothing to assert - request handler will throw if anything unexpected happens
                }
            }

            [Fact]
            public async Task IfPoolExistsButIsTooSmall_ThenItIsResized()
            {
                var resizes = new List<int>();

                using (BatchClient batchClient = BatchResourceFactory.CreateBatchClient())
                {
                    // Arrange
                    batchClient.OnRequest<PoolGetBatchRequest>(r => r.Return(new Protocol.Models.CloudPool { TargetDedicated = 37 }));
                    batchClient.OnRequest<PoolAddBatchRequest>(r => r.Throw(new InvalidOperationException("Expected pool not to be created, but pool was created")));
                    batchClient.OnRequest<PoolResizeBatchRequest>(r => r.Capture(r.Parameters.TargetDedicated, resizes));

                    var poolCoordinator = new PoolCoordinator(batchClient);

                    // Act
                    await poolCoordinator.EnsureCapacity("new-pool", "A2", 40);

                    // Assert
                    Assert.Equal(1, resizes.Count);
                    Assert.Equal(40, resizes.Single());
                }
            }

            [Fact]
            public async Task IfPoolExistsButIsTooBig_ThenItIsNotResized()
            {
                using (BatchClient batchClient = BatchResourceFactory.CreateBatchClient())
                {
                    // Arrange
                    batchClient.OnRequest<PoolGetBatchRequest>(r => r.Return(new Protocol.Models.CloudPool { TargetDedicated = 41 }));
                    batchClient.OnRequest<PoolAddBatchRequest>(r => r.Throw(new InvalidOperationException("Expected pool not to be created, but pool was created")));
                    batchClient.OnRequest<PoolResizeBatchRequest>(r => r.Throw(new InvalidOperationException("Expected pool not to be resized, but pool was resized")));

                    var poolCoordinator = new PoolCoordinator(batchClient);

                    // Act
                    await poolCoordinator.EnsureCapacity("new-pool", "A2", 40);

                    // Nothing to assert - request handler will throw if anything unexpected happens
                }
            }
        }
    }
}
