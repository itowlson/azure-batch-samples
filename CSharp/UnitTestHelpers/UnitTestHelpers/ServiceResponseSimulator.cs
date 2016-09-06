//Copyright (c) Microsoft Corporation

using Microsoft.Azure.Batch.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Azure.Batch.Test
{
    public class ServiceResponseSimulator
    {
        private readonly RequestInterceptor _behavior;
        private readonly List<SimulationEntry> _actions = new List<SimulationEntry>();

        public ServiceResponseSimulator()
        {
            _behavior = new RequestInterceptor(Intercept);
        }

        public void OnRequest<T>(Action<T> interceptionAction)
            where T : IBatchRequest
        {
            _actions.Add(new SimulationEntry(r => r is T, r => interceptionAction((T)r)));
        }

        private void Intercept(IBatchRequest request)
        {
            foreach (var action in _actions)
            {
                action.Run(request);
            }
        }

        private struct SimulationEntry
        {
            private readonly Func<IBatchRequest, bool> _condition;
            private readonly Action<IBatchRequest> _action;

            public SimulationEntry(Func<IBatchRequest, bool> condition, Action<IBatchRequest> action)
            {
                if (condition == null)
                {
                    throw new ArgumentNullException(nameof(condition));
                }

                if (action == null)
                {
                    throw new ArgumentNullException(nameof(action));
                }

                _condition = condition;
                _action = action;
            }

            public void Run(IBatchRequest request)
            {
                if (_condition(request))
                {
                    _action(request);
                }
            }
        }

        public BatchClientBehavior Behavior => _behavior;
    }
}
