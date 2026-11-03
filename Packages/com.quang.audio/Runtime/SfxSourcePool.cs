using System;
using System.Collections.Generic;

namespace Quang.Audio
{
    /// <summary>
    /// Fixed set of sources: <see cref="Rent"/> returns a free one, or steals the one rented longest ago.
    /// Generic so the selection rule is testable without <c>AudioSource</c>.
    /// </summary>
    public sealed class SfxSourcePool<T> where T : class
    {
        private readonly List<T> items;
        private readonly Func<T, bool> isBusy;
        private readonly long[] rentStamps;
        private long stamp;

        public SfxSourcePool(IEnumerable<T> sources, Func<T, bool> isBusy)
        {
            items = new List<T>(sources ?? throw new ArgumentNullException(nameof(sources)));
            if (items.Count == 0) throw new ArgumentException("Pool needs at least one source.", nameof(sources));
            this.isBusy = isBusy ?? throw new ArgumentNullException(nameof(isBusy));
            rentStamps = new long[items.Count];
        }

        public IReadOnlyList<T> Items => items;

        public T Rent()
        {
            int index = -1;
            for (int i = 0; i < items.Count; i++)
            {
                if (!isBusy(items[i]))
                {
                    index = i;
                    break;
                }
            }

            if (index < 0)
            {
                index = 0;
                for (int i = 1; i < items.Count; i++)
                {
                    if (rentStamps[i] < rentStamps[index]) index = i;
                }
            }

            rentStamps[index] = ++stamp;
            return items[index];
        }
    }
}
