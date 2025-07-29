using System;
using System.Collections.Generic;

namespace Mir2Assistant.Common.Utils
{
    public class LimitedHashSet<T> : HashSet<T>
    {
        private readonly int maxCapacity;

        public LimitedHashSet(int maxCapacity) : base()
        {
            this.maxCapacity = maxCapacity;
        }

        public new bool Add(T item)
        {
            if (Count >= maxCapacity)
            {
                // 如果已达到最大容量，移除最早添加的元素
                // 由于HashSet不保证顺序，我们只能移除任意一个元素
                using (var enumerator = GetEnumerator())
                {
                    if (enumerator.MoveNext())
                    {
                        Remove(enumerator.Current);
                    }
                }
            }
            return base.Add(item);
        }
    }
} 