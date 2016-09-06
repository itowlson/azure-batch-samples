//Copyright (c) Microsoft Corporation

using Microsoft.Azure.Batch.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Azure.Batch.Test
{
    public static class BatchClientExtensions
    {
        public static void OnRequest<T>(this BatchClient batchClient, Action<T> action)
            where T : IBatchRequest
        {
            batchClient.GetServiceResponseSimulator().OnRequest<T>(action);
        }

        private static ServiceResponseSimulator GetServiceResponseSimulator(this BatchClient batchClient)
        {
            var simulator = batchClient.CustomBehaviors.OfType<ServiceResponseSimulator>().FirstOrDefault();
            if (simulator == null)
            {
                simulator = new ServiceResponseSimulator();
                batchClient.CustomBehaviors.Add(simulator.Behavior);
            }

            return simulator;
        }
    }
}
