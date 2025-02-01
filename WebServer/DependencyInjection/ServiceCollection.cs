namespace Webserver.DependencyInjection
{
    public class ServiceCollection
    {
        private readonly DependencyInjector di = new();

        public void AddSingleton<S>() where S : new() => di.AddSingleton<S>();

        public void AddSingleton<S>(Func<S> factory) => di.AddSingleton<S>(factory);

        public void AddSingleton<I, S>() where S : I, new() => di.AddSingleton<I, S>();

        public void AddSingleton<I, S>(Func<S> factory) where S : I => di.AddSingleton<I, S>(factory);

        public void AddScoped<I, S>() where S : I => di.AddScoped<I, S>();

        public void AddScoped<S>() => di.AddScoped<S>();

        public IServiceCollectionProvider Build()
        {
            var provider = new ServiceCollectionProvider(di);
            di.AddSingleton<IServiceCollectionProvider, ServiceCollectionProvider>(() => provider);
            return provider;
        }
    }
}
