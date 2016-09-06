//Copyright (c) Microsoft Corporation

using Microsoft.Azure.Batch.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Azure.Batch.UnitTestHelpers.Usage.Testee
{
    public class PoolCoordinator
    {
        private readonly BatchClient _batchClient;

        public PoolCoordinator(BatchClient batchClient)
        {
            if (batchClient == null)
            {
                throw new ArgumentNullException(nameof(batchClient));
            }

            _batchClient = batchClient;
        }

        public async Task EnsureCapacity(string poolId, string virtualMachineSize, int desiredCapacity)
        {
            try
            {
                var pool = await _batchClient.PoolOperations.GetPoolAsync(poolId);

                if (pool.TargetDedicated < desiredCapacity)
                {
                    await pool.ResizeAsync(desiredCapacity);
                }
            }
            catch (BatchException ex) when (ex?.RequestInformation.BatchError.Code == Common.BatchErrorCodeStrings.PoolNotFound)
            {
                var configuration = new CloudServiceConfiguration(osFamily: "4");
                var pool = _batchClient.PoolOperations.CreatePool(poolId, virtualMachineSize, configuration, desiredCapacity);
                await pool.CommitAsync();
            }
        }
    }
}
