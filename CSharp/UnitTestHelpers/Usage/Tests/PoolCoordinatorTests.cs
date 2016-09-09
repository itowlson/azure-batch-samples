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
            // This example shows how to use the raw interception feature to simulate service responses.
            // It allows the test to perform any action on the request. This provides the greatest level
            // of control, but is rarely needed - in most cases you simply want to verify that certain
            // request data is sent, or fake a response, or verify that a request was *not* made, which
            // can be done more readily with the Capture, Return/Error/Throw, and Unexpected helpers.
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

            // This example shows the use of the Return and Error convenience methods to simulate
            // service responses. In this test we want to exercise the 'pool does not exist and the
            // method needs to create it' path through the EnsureCapacity method.  We therefore:
            //
            // * Respond to the 'get pool' request with the 'pool does not exist' error.
            // * Respond to the 'add pool' request by logging the pool id and returning an empty response.
            //   This can be simplified further by using the Capture methods as shown below. The library
            //   also provides a higher-level Return method to save you dealing with Azure response
            //   classes; this is demonstrated in the IfExistsThenItIsNotRecreated test.
            [Fact]
            public async Task IfPoolDoesNotExistThenItIsCreated_TestedUsingConvenienceMethods()
            {
                var createdPools = new List<string>();

                using (BatchClient batchClient = BatchResourceFactory.CreateBatchClient())
                {
                    // Arrange
                    batchClient.OnRequest<PoolGetBatchRequest>(r => r.Error(HttpStatusCode.NotFound, BatchErrorCodeStrings.PoolNotFound));
                    batchClient.OnRequest<PoolAddBatchRequest>(r => r.Return(() => { createdPools.Add(r.Parameters.Id); return new AzureOperationHeaderResponse<Protocol.Models.PoolAddHeaders>(); }));

                    var poolCoordinator = new PoolCoordinator(batchClient);

                    // Act
                    await poolCoordinator.EnsureCapacity("new-pool", "A2", 40);

                    // Assert
                    Assert.Equal(1, createdPools.Count);
                    Assert.Equal("new-pool", createdPools.Single());
                }
            }

            // This example shows the use of the Capture and Error convenience methods to simulate
            // service responses. In this test we want to exercise the 'pool does not exist and the
            // method needs to create it' path through the EnsureCapacity method.  We therefore:
            //
            // * Respond to the 'get pool' request with the 'pool does not exist' error.
            // * Respond to the 'add pool' request by capturing the pool id. The Capture method
            //   implicitly simulates a default response, which is convenient when the test cares
            //   only about checking the values sent to the service.
            [Fact]
            public async Task IfPoolDoesNotExistThenItIsCreated_TestedUsingCaptureHelper()
            {
                var createdPools = new List<string>();

                using (BatchClient batchClient = BatchResourceFactory.CreateBatchClient())
                {
                    // Arrange
                    batchClient.OnRequest<PoolGetBatchRequest>(r => r.Error(HttpStatusCode.NotFound, BatchErrorCodeStrings.PoolNotFound));
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

            // This example shows the use of the Return and Unexpected convenience methods.
            // In this test we want to exercise the 'pool does exist and no further action should be taken'
            // path through the EnsureCapacity method.  We therefore:
            //
            // * Respond to the 'get pool' request by returning a pool. Note that:
            //   - We return an object from the Protocol.Models namespace.
            //   - We do not need to fill out all the properties of the returned object, only those that
            //     the testee code cares about.
            //   - We do not need to wrap the object in an Azure operation response object, as we are
            //     using an overload of Return that handles this for us.
            // * Respond to the 'add pool' and 'resize pool' requests by throwing an exception that these
            //   requests were unexpected and should not have happened. If the testee code incorrectly
            //   made either of these requests then the exception would be thrown and the test would
            //   (correctly) fail.
            [Fact]
            public async Task IfExistsThenItIsNotRecreated()
            {
                using (BatchClient batchClient = BatchResourceFactory.CreateBatchClient())
                {
                    // Arrange
                    batchClient.OnRequest<PoolGetBatchRequest>(r => r.Return(new Protocol.Models.CloudPool { TargetDedicated = 40 }));
                    batchClient.OnRequest<PoolAddBatchRequest>(r => r.Unexpected("Expected pool not to be created, but pool was created"));
                    batchClient.OnRequest<PoolResizeBatchRequest>(r => r.Unexpected("Expected pool not to be resized, but pool was resized"));

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
                    batchClient.OnRequest<PoolAddBatchRequest>(r => r.Unexpected("Expected pool not to be created, but pool was created"));
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
                    batchClient.OnRequest<PoolAddBatchRequest>(r => r.Unexpected("Expected pool not to be created, but pool was created"));
                    batchClient.OnRequest<PoolResizeBatchRequest>(r => r.Unexpected("Expected pool not to be resized, but pool was resized"));

                    var poolCoordinator = new PoolCoordinator(batchClient);

                    // Act
                    await poolCoordinator.EnsureCapacity("new-pool", "A2", 40);

                    // Nothing to assert - request handler will throw if anything unexpected happens
                }
            }
        }
    }
}
