using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;

public class ObjectPool<T>
{
    public class Item<T>
    {
        public readonly T item;
        private readonly ObjectPool<T> pool;

        public Item(ObjectPool<T> pool, T item)
        {
            this.pool = pool;
            this.item = item;
        }

        ~Item()
        {
            pool.Return(item);
        }

        public static implicit operator T (Item<T> src)
        {
            return src.item;
        }
    }

    private readonly ConcurrentBag<T> _objects;
    private readonly Func<T> _objectGenerator;

    public ObjectPool(Func<T> objectGenerator)
    {
        _objectGenerator = objectGenerator ?? throw new ArgumentNullException(nameof(objectGenerator));
        _objects = new ConcurrentBag<T>();
    }

    public Item<T> Create()
    {
        T item;
        if (false == _objects.TryTake(out item))
        {
            item = _objectGenerator();
        }

        return new Item<T>(this, item);
    }

    public void Return(T item) => _objects.Add(item);
}
