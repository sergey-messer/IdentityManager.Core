using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;

namespace TzIdentityManager.Configuration
{
    public class IdentityManagerServiceFactory
    {
        readonly List<ServiceDescriptor> _registrations = new List<ServiceDescriptor>();

        /// <summary>
        /// Gets the a list of additional dependencies.
        /// </summary>
        /// <value>
        /// The dependencies.
        /// </value>
        public IEnumerable<ServiceDescriptor> Registrations => _registrations;

        /// <summary>
        /// Adds a registration to the dependency list
        /// </summary>
        /// <typeparam name="T">Type of the dependency</typeparam>
        /// <param name="registration">The registration.</param>
        public void Register(ServiceDescriptor registration)
        {
            _registrations.Add(registration);
        }

        public ServiceDescriptor IdentityManagerServiceDescriptor { get; set; }
    }
}
