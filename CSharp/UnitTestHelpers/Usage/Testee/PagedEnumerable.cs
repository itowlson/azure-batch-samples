//Copyright (c) Microsoft Corporation

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Azure.Batch.UnitTestHelpers.Usage.Testee
{
    internal static class PagedEnumerable
    {
        internal static async Task<int> CountAsync<T>(this IPagedEnumerable<T> source)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            int count = 0;

            var enumerator = source.GetPagedEnumerator();

            while (await enumerator.MoveNextAsync())
            {
                ++count;
            }

            return count;
        }
    }
}
