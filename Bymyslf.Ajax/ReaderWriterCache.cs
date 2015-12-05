namespace Bymyslf.Ajax
{
    using System;
    using System.Collections.Generic;
    using System.Threading;

    //From ASP.NET source code
    internal abstract class ReaderWriterCache<TKey, TValue>
    {
        private readonly Dictionary<TKey, TValue> cache;
        private readonly ReaderWriterLockSlim readerWriterLock = new ReaderWriterLockSlim();

        protected ReaderWriterCache()
            : this(null)
        {
        }

        protected ReaderWriterCache(IEqualityComparer<TKey> comparer)
        {
            cache = new Dictionary<TKey, TValue>(comparer);
        }

        protected Dictionary<TKey, TValue> Cache
        {
            get { return cache; }
        }

        protected TValue FetchOrCreateItem(TKey key, Func<TValue> creator)
        {
            readerWriterLock.EnterReadLock();
            try
            {
                TValue existingEntry;
                if (cache.TryGetValue(key, out existingEntry))
                {
                    return existingEntry;
                }
            }
            finally
            {
                readerWriterLock.ExitReadLock();
            }

            TValue newEntry = creator();
            readerWriterLock.EnterWriteLock();
            try
            {
                TValue existingEntry;
                if (cache.TryGetValue(key, out existingEntry))
                {
                    return existingEntry;
                }

                cache[key] = newEntry;
                return newEntry;
            }
            finally
            {
                readerWriterLock.ExitWriteLock();
            }
        }
    }
}