using System;

namespace DeltaTune.DependencyManagement
{
    public static class GlobalServices
    {
        public static IServiceRegistry ServiceRegistry;
    
        public static T Get<T>()
        {
            if (ServiceRegistry == null) throw new ServiceNotFoundException(typeof(IServiceRegistry));
            return ServiceRegistry.Get<T>();
        }

        public static void Register<T>(T service)
        {
            if (ServiceRegistry == null) throw new ServiceNotFoundException(typeof(IServiceRegistry));
            if (service == null) throw new ArgumentNullException(nameof(service));
            ServiceRegistry.Register<T>(service);
        }
    
        public static void Unregister<T>()
        {
            if (ServiceRegistry == null) throw new ServiceNotFoundException(typeof(IServiceRegistry));
            ServiceRegistry.Unregister<T>();
        }
    }
}