namespace Platformer.Engine.Resources
{
    using System;
    using System.Collections.Concurrent;

    /// <summary>
    /// Class which implements a pool of objects.
    /// </summary>
    /// <typeparam name="T">The type of object handled by the pool.</typeparam>
    /// <remarks>
    /// https://msdn.microsoft.com/en-us/library/ff458671(v=vs.110).aspx
    /// </remarks>
    public class ObjectPool<T>
    {
        private ConcurrentBag<T> _objects;
        private Func<T> _objectGenerator;

        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectPool{T}"/> class.
        /// </summary>
        /// <param name="objectGenerator">A method that creates new instances of the object.</param>
        public ObjectPool(Func<T> objectGenerator)
        {
            if (objectGenerator == null)
            {
                throw new ArgumentNullException(nameof(objectGenerator));
            }

            _objects = new ConcurrentBag<T>();
            _objectGenerator = objectGenerator;
        }

        /// <summary>
        /// Gets an object from the pool. If the pool is empty a new instance is created.
        /// </summary>
        /// <returns>An object from the pool.</returns>
        public T GetObject()
        {
            T item;
            if (_objects.TryTake(out item))
            {
                return item;
            }

            return _objectGenerator();
        }

        /// <summary>
        /// Puts an object back into the pool.
        /// </summary>
        /// <param name="item">The object to place back into the pool.</param>
        public void PutObject(T item)
        {
            _objects.Add(item);
        }
    }
}
