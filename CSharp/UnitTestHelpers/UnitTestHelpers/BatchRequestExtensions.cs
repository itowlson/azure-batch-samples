using Microsoft.Rest.Azure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Azure.Batch.Test
{
    public static class BatchRequestExtensions
    {
        public static void Return<TOptions, TResponse>(this Protocol.BatchRequest<TOptions, TResponse> r, Func<TResponse> response)
            where TOptions : Protocol.Models.IOptions, new()
            where TResponse : IAzureOperationResponse
        {
            r.ServiceRequestFunc = _ => Task.FromResult(response());
        }

        public static void Throw<TOptions, TResponse>(this Protocol.BatchRequest<TOptions, TResponse> r, Exception exception)
            where TOptions : Protocol.Models.IOptions, new()
            where TResponse : IAzureOperationResponse
        {
            r.ServiceRequestFunc = _ => { throw exception; };
        }

        public static void Return<TBody, TOptions, TResponse>(this Protocol.BatchRequest<TBody, TOptions, TResponse> r, Func<TResponse> response)
            where TOptions : Protocol.Models.IOptions, new()
            where TResponse : IAzureOperationResponse
        {
            r.ServiceRequestFunc = _ => Task.FromResult(response());
        }

        public static void Throw<TBody, TOptions, TResponse>(this Protocol.BatchRequest<TBody, TOptions, TResponse> r, Exception exception)
            where TOptions : Protocol.Models.IOptions, new()
            where TResponse : IAzureOperationResponse
        {
            r.ServiceRequestFunc = _ => { throw exception; };
        }
    }
}
