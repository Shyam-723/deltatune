using System;
using System.Collections.Generic;

namespace DeltaTune.DependencyManagement
{
    public class ServiceRegistry : IServiceRegistry
    {
        private readonly IDictionary<Type, object> services = new Dictionary<Type, object>();
    
        public T Get<T>()
        {
            Type type = typeof(T);
            if (services.TryGetValue(type, out var dependency))
            {
                return (T)dependency;
            }
            else
            {
                throw new ServiceNotFoundException(type);
            }
        }

        public void Register<T>(T value)
        {
            Type type = typeof(T);
            if (!services.ContainsKey(type))
            {
                services.Add(type, value);
            }
            else
            {
                throw new DuplicateServiceException(type);
            }
        }

        public void Unregister<T>()
        {
            Type type = typeof(T);
            if (!services.Remove(type))
            {
                throw new ServiceNotFoundException(type);
            }
        }
    }
}