namespace Webserver.DependencyInjection
{
    public class ServiceCollectionProvider : IServiceCollectionProvider
    {
        private readonly DependencyInjector di;

        public ServiceCollectionProvider(DependencyInjector di)
        {
            this.di = di;
        }

        public T GetService<T>()
        {
            if (di.TryInvokeObject(typeof(T), out var instance))
            {
                return (T)instance!;
            }
            throw new InvalidOperationException($"Service of type {typeof(T)} is not registered.");
        }

        public object? GetService(Type type)
        {
            if (di.TryInvokeObject(type, out var instance))
            {
                return instance;
            }
            return null;
        }
    }
}
