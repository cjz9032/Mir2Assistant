using System.Collections;
using System.Collections.Generic;

namespace Mir2Assistant.Common.Utils;

/// <summary>
/// 一个固定大小的字典，当超出容量时会自动移除最早添加的元素
/// </summary>
/// <typeparam name="TKey">键类型</typeparam>
/// <typeparam name="TValue">值类型</typeparam>
public class LimitedDictionary<TKey, TValue> : IDictionary<TKey, TValue> where TKey : notnull
{
    private readonly int _maxSize;
    private readonly Dictionary<TKey, TValue> _dict;
    private Queue<TKey> _queue;

    public LimitedDictionary(int maxSize = 1000)
    {
        if (maxSize <= 0)
            throw new ArgumentException("最大容量必须大于0", nameof(maxSize));

        _maxSize = maxSize;
        _dict = new Dictionary<TKey, TValue>();
        _queue = new Queue<TKey>();
    }

    public TValue this[TKey key]
    {
        get => _dict[key];
        set
        {
            if (!_dict.ContainsKey(key))
            {
                _queue.Enqueue(key);
                if (_dict.Count >= _maxSize)
                {
                    var oldest = _queue.Dequeue();
                    _dict.Remove(oldest);
                }
            }
            _dict[key] = value;
        }
    }

    public ICollection<TKey> Keys => _dict.Keys;
    public ICollection<TValue> Values => _dict.Values;
    public int Count => _dict.Count;
    public bool IsReadOnly => false;

    public void Add(TKey key, TValue value)
    {
        if (!_dict.ContainsKey(key))
        {
            _queue.Enqueue(key);
            if (_dict.Count >= _maxSize)
            {
                var oldest = _queue.Dequeue();
                _dict.Remove(oldest);
            }
        }
        _dict.Add(key, value);
    }

    public void Add(KeyValuePair<TKey, TValue> item) => Add(item.Key, item.Value);

    public void Clear()
    {
        _dict.Clear();
        _queue.Clear();
    }

    public bool Contains(KeyValuePair<TKey, TValue> item) => 
        _dict.TryGetValue(item.Key, out var value) && 
        EqualityComparer<TValue>.Default.Equals(value, item.Value);

    public bool ContainsKey(TKey key) => _dict.ContainsKey(key);

    public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
    {
        ((ICollection<KeyValuePair<TKey, TValue>>)_dict).CopyTo(array, arrayIndex);
    }

    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => _dict.GetEnumerator();

    public bool Remove(TKey key)
    {
        if (_dict.Remove(key))
        {
            var tempQueue = new Queue<TKey>();
            while (_queue.Count > 0)
            {
                var current = _queue.Dequeue();
                if (!EqualityComparer<TKey>.Default.Equals(current, key))
                {
                    tempQueue.Enqueue(current);
                }
            }
            _queue = tempQueue;
            return true;
        }
        return false;
    }

    public bool Remove(KeyValuePair<TKey, TValue> item)
    {
        if (Contains(item))
        {
            return Remove(item.Key);
        }
        return false;
    }

    public bool TryGetValue(TKey key, out TValue value) => _dict.TryGetValue(key, out value!);

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
} 