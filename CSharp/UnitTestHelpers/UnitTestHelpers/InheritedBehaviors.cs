//Copyright (c) Microsoft Corporation

using Microsoft.Azure.Batch.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Azure.Batch.Test
{
    public static class InheritedBehaviors
    {
        public static void OnRequest<T>(this IInheritedBehaviors behaviors, Action<T> action)
            where T : IBatchRequest
        {
            behaviors.GetServiceResponseSimulator().OnRequest<T>(action);
        }

        private static ServiceResponseSimulator GetServiceResponseSimulator(this IInheritedBehaviors behaviors)
        {
            var simulator = behaviors.CustomBehaviors.OfType<ServiceResponseSimulator>().FirstOrDefault();
            if (simulator == null)
            {
                simulator = new ServiceResponseSimulator();
                behaviors.CustomBehaviors.Add(simulator.Behavior);
            }

            return simulator;
        }
    }
}
