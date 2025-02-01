namespace Webserver.DependencyInjection
{
    public interface IServiceCollectionProvider
    {
        T GetService<T>();

        object? GetService(Type type);
    }
}
