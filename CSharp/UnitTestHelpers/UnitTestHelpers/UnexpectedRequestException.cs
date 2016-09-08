using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Azure.Batch.Test
{

    [Serializable]
    public class UnexpectedRequestException : Exception
    {
        public UnexpectedRequestException() : this("The code under test attempted an unexpected request to the Batch service.") { }
        public UnexpectedRequestException(Type requestType) : this($"The code under test attempted an unexpected request of type '{requestType.Name}' to the Batch service.") { }
        public UnexpectedRequestException(Type requestType, string message) : this($"{message}. The code under test attempted an unexpected request of type '{requestType.Name}' to the Batch service.") { }
        public UnexpectedRequestException(string message) : base(message) { }
        public UnexpectedRequestException(string message, Exception inner) : base(message, inner) { }
        protected UnexpectedRequestException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
