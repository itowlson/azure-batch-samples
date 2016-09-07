using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Azure.Batch.UnitTestHelpers.Usage.Testee
{
    public class JobCoordinator
    {
        private readonly JobOperations _jobOperations;

        public JobCoordinator(JobOperations jobOperations)
        {
            if (jobOperations == null)
            {
                throw new ArgumentNullException(nameof(jobOperations));
            }

            _jobOperations = jobOperations;
        }

        public async Task<int> GetCompletedTaskCountAsync(string jobId)
        {
            var detailLevel = new ODATADetailLevel(
                filterClause: "state eq 'completed'",
                selectClause: "id"
            );

            return await _jobOperations.ListTasks(jobId, detailLevel).CountAsync();
        }
    }
}
