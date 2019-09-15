using System;
using System.Collections.Concurrent;

//https://msdn.microsoft.com/zh-cn/magazine/mt814994

namespace Aksl.Core
{
    public static class Singleton<T>  where T : new()
    {
        private static ConcurrentDictionary<Type, T> _instances = new ConcurrentDictionary<Type, T>();

        public static T Instance
        {
            get
            {
                return _instances.GetOrAdd(typeof(T), (t) => new T());
            }
        }
    }
}
