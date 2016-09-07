//Copyright (c) Microsoft Corporation

using Microsoft.Rest.Azure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;

namespace Microsoft.Azure.Batch.Test
{
    public sealed class DataPage<T> : IPage<T>
    {
        private readonly IEnumerable<T> _data;

        public DataPage(IEnumerable<T> data, string nextPageLink)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            _data = data;

            NextPageLink = nextPageLink;
        }

        public string NextPageLink { get; }

        public IEnumerator<T> GetEnumerator()
        {
            return _data.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    public static class DataPage
    {
        public static IPage<T> Empty<T>()
        {
            return Single(Enumerable.Empty<T>());
        }

        public static IPage<T> Single<T>(IEnumerable<T> source)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            return new DataPage<T>(source, null);
        }
    }
}
