using System;
using System.Collections.Concurrent;
using System.Linq;

namespace Mir2Assistant.Common.Utils
{
    /// <summary>
    /// 线程安全的容量限制 HashSet，基于 ConcurrentDictionary 实现
    /// </summary>
    public class LimitedHashSet<T> where T : notnull
    {
        private readonly ConcurrentDictionary<T, byte> _dict;
        private readonly int _maxCapacity;
        private readonly object _addLock = new object();

        public LimitedHashSet(int maxCapacity)
        {
            _maxCapacity = maxCapacity;
            _dict = new ConcurrentDictionary<T, byte>();
        }

        /// <summary>
        /// 添加元素，如果达到容量上限则移除一个旧元素
        /// </summary>
        public bool Add(T item)
        {
            lock (_addLock)
            {
                // 如果已达到最大容量，移除第一个元素
                if (_dict.Count >= _maxCapacity)
                {
                    var firstKey = _dict.Keys.FirstOrDefault();
                    if (firstKey != null)
                    {
                        _dict.TryRemove(firstKey, out _);
                    }
                }
                return _dict.TryAdd(item, 0);
            }
        }

        /// <summary>
        /// 检查元素是否存在（线程安全，无锁）
        /// </summary>
        public bool Contains(T item)
        {
            return _dict.ContainsKey(item);
        }

        /// <summary>
        /// 移除元素
        /// </summary>
        public bool Remove(T item)
        {
            return _dict.TryRemove(item, out _);
        }

        /// <summary>
        /// 清空所有元素
        /// </summary>
        public void Clear()
        {
            _dict.Clear();
        }

        /// <summary>
        /// 获取当前元素数量
        /// </summary>
        public int Count => _dict.Count;
    }
}