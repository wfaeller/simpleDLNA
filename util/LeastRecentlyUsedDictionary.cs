﻿using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace NMaier.SimpleDlna.Utilities
{
  public sealed class LeastRecentlyUsedDictionary<TKey, TValue>
    : IDictionary<TKey, TValue>
  {
    private readonly uint capacity;

    private readonly ConcurrentDictionary<TKey, LinkedListNode<KeyValuePair<TKey, TValue>>> items =
      new ConcurrentDictionary<TKey, LinkedListNode<KeyValuePair<TKey, TValue>>>();

    private readonly LinkedList<KeyValuePair<TKey, TValue>> order =
      new LinkedList<KeyValuePair<TKey, TValue>>();

    private readonly uint toDrop;

    [CLSCompliant(false)]
    public LeastRecentlyUsedDictionary(uint capacity)
    {
      this.capacity = capacity;
      toDrop = Math.Min(10, (uint)(capacity * 0.07));
    }

    public LeastRecentlyUsedDictionary(int capacity)
      : this((uint)capacity)
    {
    }

    [CLSCompliant(false)]
    public uint Capacity
    {
      get
      {
        return capacity;
      }
    }

    public int Count
    {
      get
      {
        return items.Count;
      }
    }

    public bool IsReadOnly
    {
      get
      {
        return false;
      }
    }

    public ICollection<TKey> Keys
    {
      get
      {
        return items.Keys;
      }
    }

    public ICollection<TValue> Values
    {
      get
      {
        return (from i in items.Values
                select i.Value.Value).ToList();
      }
    }

    public TValue this[TKey key]
    {
      get
      {
        return items[key].Value.Value;
      }
      [MethodImpl(MethodImplOptions.Synchronized)]
      set
      {
        Remove(key);
        Add(key, value);
      }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return items.GetEnumerator();
    }

    private TValue MaybeDropSome()
    {
      if (Count <= capacity) {
        return default(TValue);
      }
      lock (order) {
        LinkedListNode<KeyValuePair<TKey, TValue>> item;
        TValue rv = default(TValue);
        for (var i = 0; i < toDrop; ++i) {
          if (items.TryRemove(order.Last.Value.Key, out item)) {
            rv = item.Value.Value;
          }
          order.RemoveLast();
        }
        return rv;
      }
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    public TValue AddAndPop(KeyValuePair<TKey, TValue> item)
    {
      LinkedListNode<KeyValuePair<TKey, TValue>> node;
      lock (order) {
        node = order.AddFirst(item);
      }
      items.TryAdd(item.Key, node);
      return MaybeDropSome();
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    public void Add(KeyValuePair<TKey, TValue> item)
    {
      AddAndPop(item);
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    public TValue AddAndPop(TKey key, TValue value)
    {
      return AddAndPop(new KeyValuePair<TKey, TValue>(key, value));
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    public void Add(TKey key, TValue value)
    {
      AddAndPop(new KeyValuePair<TKey, TValue>(key, value));
    }


    [MethodImpl(MethodImplOptions.Synchronized)]
    public void Clear()
    {
      items.Clear();
      lock (order) {
        order.Clear();
      }
    }

    public bool Contains(KeyValuePair<TKey, TValue> item)
    {
      return items.ContainsKey(item.Key);
    }

    public bool ContainsKey(TKey key)
    {
      return items.ContainsKey(key);
    }

    public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
    {
      throw new NotImplementedException();
    }

    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
    {
      foreach (var i in items) {
        yield return i.Value.Value;
      }
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    public bool Remove(TKey key)
    {
      LinkedListNode<KeyValuePair<TKey, TValue>> node;
      if (items.TryRemove(key, out node)) {
        lock (order) {
          order.Remove(node);
        }
        return true;
      }
      return false;
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    public bool Remove(KeyValuePair<TKey, TValue> item)
    {
      LinkedListNode<KeyValuePair<TKey, TValue>> node;
      if (items.TryRemove(item.Key, out node)) {
        lock (order) {
          order.Remove(node);
        }
        return true;
      }
      return false;
    }

    public bool TryGetValue(TKey key, out TValue value)
    {
      LinkedListNode<KeyValuePair<TKey, TValue>> node;
      if (items.TryGetValue(key, out node)) {
        value = node.Value.Value;
        return true;
      }
      value = default(TValue);
      return false;
    }
  }
}
