namespace FlaneerMediaLib
{
    /// <summary>
    /// Empty interface to allow for identification of services to put into the registry
    /// </summary>
    public interface IService { }
    
    /// <summary>
    /// Implementation of service registry pattern
    /// </summary>
    public class ServiceRegistry
    {
        /// <summary>
        /// Event that is fired when a new service is added into the registry
        /// </summary>
        internal static Action<IService>? ServiceAdded;

        private readonly Dictionary<Type, IService> registry = new();

        private static ServiceRegistry instance = null!;

        private ServiceRegistry()
        {
        }

        // 
        // ReSharper disable once ConstantNullCoalescingCondition
        private static ServiceRegistry Instance => instance ??= new ServiceRegistry();

        /// <summary>
        /// Add a new service to the registry
        /// </summary>
        public static void AddService<T>(T service) where T : IService
        {
            foreach (var ifce in service.GetType().GetInterfaces())
            {
                Instance.registry.Add(ifce, service);
                ServiceAdded?.Invoke(service);
            }
        }

        /// <summary>
        /// Attempts to get a service out of the service registry
        /// </summary>
        public static bool TryGetService<T>(out T service) where T : IService
        {
            if (Instance.registry.TryGetValue(typeof(T), out var ret))
            {
                service = (T) ret;
                return true;
            }

            service = default!;
            return false;
        }
    }
}
