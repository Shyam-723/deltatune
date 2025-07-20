using System;

namespace DeltaTune.DependencyManagement
{
    public class ServiceNotFoundException : Exception
    {
        public Type ServiceType { get; set; }

        public ServiceNotFoundException(Type serviceType) : base($"Could not find an object of type ${serviceType.FullName}.")
        {
            ServiceType = serviceType;
        }
    }
}