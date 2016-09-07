//Copyright (c) Microsoft Corporation

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Azure.Batch.Test
{
    public static class BatchResourceFactory
    {
        public static BatchClient CreateBatchClient()
        {
            var restClient = new Protocol.BatchServiceClient(new FakeCredential(), new FailAllHttpHandler());
            return BatchClient.Open(restClient);
        }

        private class FailAllHttpHandler : HttpClientHandler
        {
            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                throw new InvalidOperationException($"This BatchClient should never attempt to communicate with a real service endpoint. Inject a handler for {request.Method} requests to {request.RequestUri.AbsolutePath}.");
            }
        }

        private class FakeCredential : Microsoft.Azure.Batch.Protocol.BatchCredentials
        {
            public override Task SignRequestAsync(HttpRequestMessage httpRequest, CancellationToken cancellationToken)
            {
                return Task.Delay(0);
            }
        }

    }
}
