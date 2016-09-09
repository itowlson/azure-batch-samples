using Microsoft.Rest.Azure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Azure.Batch.Test
{
    public static class BatchRequestExtensions
    {
        // Return

        public static void Return<TOptions, TResponse>(this Protocol.BatchRequest<TOptions, TResponse> r, Func<TResponse> response)
            where TOptions : Protocol.Models.IOptions, new()
            where TResponse : IAzureOperationResponse
        {
            r.ServiceRequestFunc = _ => Task.FromResult(response());
        }

        public static void Return<TBody, TOptions, TResponse>(this Protocol.BatchRequest<TBody, TOptions, TResponse> r, Func<TResponse> response)
            where TOptions : Protocol.Models.IOptions, new()
            where TResponse : IAzureOperationResponse
        {
            r.ServiceRequestFunc = _ => Task.FromResult(response());
        }

        public static void Return<TOptions, TResponse, THeader>(this Protocol.BatchRequest<TOptions, AzureOperationResponse<TResponse, THeader>> r, TResponse response)
            where TOptions : Protocol.Models.IOptions, new()
        {
            r.ServiceRequestFunc = _ => Task.FromResult(new AzureOperationResponse<TResponse, THeader> { Body = response });
        }

        public static void Return<TOptions, TResponse, THeader>(this Protocol.BatchRequest<TOptions, AzureOperationResponse<IPage<TResponse>, THeader>> r, IEnumerable<TResponse> response)
            where TOptions : Protocol.Models.IOptions, new()
        {
            r.ServiceRequestFunc = _ => Task.FromResult(new AzureOperationResponse<IPage<TResponse>, THeader> { Body = DataPage.Single(response) });
        }

        // Error

        public static void Error<TOptions, TResponse>(this Protocol.BatchRequest<TOptions, TResponse> r, HttpStatusCode httpStatusCode, string batchErrorCode)
            where TOptions : Protocol.Models.IOptions, new()
            where TResponse : IAzureOperationResponse
        {
            r.ServiceRequestFunc = _ => { throw BatchServiceError.Simulate(httpStatusCode, batchErrorCode); };
        }

        public static void Error<TBody, TOptions, TResponse>(this Protocol.BatchRequest<TBody, TOptions, TResponse> r, HttpStatusCode httpStatusCode, string batchErrorCode)
            where TOptions : Protocol.Models.IOptions, new()
            where TResponse : IAzureOperationResponse
        {
            r.ServiceRequestFunc = _ => { throw BatchServiceError.Simulate(httpStatusCode, batchErrorCode); };
        }

        // Capture

        public static void Capture<TOptions, THeader, TResponse, TCapture>(this Protocol.BatchRequest<TOptions, AzureOperationResponse<IPage<TResponse>, THeader>> r, Func<TCapture> capture, List<TCapture> capturedValues)
            where TOptions : Protocol.Models.IOptions, new()
        {
            r.ServiceRequestFunc = _ =>
            {
                var value = capture();
                capturedValues.Add(value);
                return Task.FromResult(new AzureOperationResponse<IPage<TResponse>, THeader> { Body = DataPage.Empty<TResponse>() });
            };
        }

        public static void Capture<TParameter, TOptions, THeader, TCapture>(this Protocol.BatchRequest<TParameter, TOptions, AzureOperationHeaderResponse<THeader>> r, TCapture capture, List<TCapture> capturedValues)
            where TOptions : Protocol.Models.IOptions, new()
        {
            r.ServiceRequestFunc = _ =>
            {
                var value = capture;
                capturedValues.Add(value);
                return Task.FromResult(new AzureOperationHeaderResponse<THeader>());
            };
        }

        // Unexpected

        public static void Unexpected<TOptions, TResponse>(this Protocol.BatchRequest<TOptions, TResponse> r)
            where TOptions : Protocol.Models.IOptions, new()
            where TResponse : IAzureOperationResponse
        {
            r.Throw(new UnexpectedRequestException(r.GetType()));
        }

        public static void Unexpected<TOptions, TResponse>(this Protocol.BatchRequest<TOptions, TResponse> r, string message)
            where TOptions : Protocol.Models.IOptions, new()
            where TResponse : IAzureOperationResponse
        {
            r.Throw(new UnexpectedRequestException(r.GetType(), message));
        }

        public static void Unexpected<TBody, TOptions, TResponse>(this Protocol.BatchRequest<TBody, TOptions, TResponse> r)
            where TOptions : Protocol.Models.IOptions, new()
            where TResponse : IAzureOperationResponse
        {
            r.Throw(new UnexpectedRequestException(r.GetType()));
        }

        public static void Unexpected<TBody, TOptions, TResponse>(this Protocol.BatchRequest<TBody, TOptions, TResponse> r, string message)
            where TOptions : Protocol.Models.IOptions, new()
            where TResponse : IAzureOperationResponse
        {
            r.Throw(new UnexpectedRequestException(r.GetType(), message));
        }

        // Throw

        public static void Throw<TOptions, TResponse>(this Protocol.BatchRequest<TOptions, TResponse> r, Exception exception)
            where TOptions : Protocol.Models.IOptions, new()
            where TResponse : IAzureOperationResponse
        {
            r.ServiceRequestFunc = _ => { throw exception; };
        }

        public static void Throw<TBody, TOptions, TResponse>(this Protocol.BatchRequest<TBody, TOptions, TResponse> r, Exception exception)
            where TOptions : Protocol.Models.IOptions, new()
            where TResponse : IAzureOperationResponse
        {
            r.ServiceRequestFunc = _ => { throw exception; };
        }
    }
}
