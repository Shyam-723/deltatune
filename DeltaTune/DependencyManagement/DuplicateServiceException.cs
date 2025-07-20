using System;

namespace DeltaTune.DependencyManagement
{
    public class DuplicateServiceException : Exception
    {
        public Type ServiceType { get; set; }

        public DuplicateServiceException(Type serviceType) : base($"A service of type ${serviceType.FullName} is already registered.")
        {
            ServiceType = serviceType;
        }
    }
}