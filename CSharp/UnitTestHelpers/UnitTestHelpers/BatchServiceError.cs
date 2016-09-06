//Copyright (c) Microsoft Corporation

using Microsoft.Azure.Batch.Protocol.Models;
using Microsoft.Rest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Azure.Batch.Test
{
    public static class BatchServiceError
    {
        public static Exception Simulate(HttpStatusCode statusCode, string errorCode)
        {
            return new BatchErrorException
            {
                Response = new HttpResponseMessageWrapper(new HttpResponseMessage(statusCode), null),
                Body = new Protocol.Models.BatchError { Code = errorCode },
            };
        }
    }
}
