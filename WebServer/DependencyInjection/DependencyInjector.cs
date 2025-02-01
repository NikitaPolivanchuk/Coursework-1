namespace Webserver.DependencyInjection
{
    public class DependencyInjector
    {
        private readonly Dictionary<Type, object> singletons = [];
        private readonly Dictionary<Type, Type> scopedServices = [];

        public void AddSingleton<S>() where S : new()
        {
            singletons[typeof(S)] = new S();
        }

        public void AddSingleton<S>(Func<S> factory)
        {
            singletons[typeof(S)] = factory()!;
        }

        public void AddSingleton<I, S>() where S : I, new()
        {
            singletons[typeof(I)] = new S();
        }

        public void AddSingleton<I, S>(Func<S> factory) where S : I
        {
            singletons[typeof(I)] = factory()!;
        }

        public void AddScoped<I, S>() where S : I
        {
            scopedServices[typeof(I)] = typeof(S);
        }

        public void AddScoped<S>()
        {
            scopedServices[typeof(S)] = typeof(S);
        }

        public bool TryInvokeObject(Type type, out object? obj)
        {
            if (singletons.TryGetValue(type, out object? singleton))
            {
                obj = singleton;
                return true;
            }

            if (scopedServices.TryGetValue(type, out Type? implementation))
            {
                return TryInvokeObject(implementation, out obj);
            }

            foreach (var constructor in type.GetConstructors())
            {
                var constructorParams = new List<object>();

                foreach (var parameter in constructor.GetParameters())
                {
                    if (singletons.ContainsKey(parameter.ParameterType))
                    {
                        constructorParams.Add(singletons[parameter.ParameterType]);
                    }
                    else if (scopedServices.ContainsKey(parameter.ParameterType))
                    {
                        Type implementationType = scopedServices[parameter.ParameterType];
                        if (TryInvokeObject(implementationType, out object? instance))
                        {
                            constructorParams.Add(instance!);
                        }
                        else
                        {
                            obj = null;
                            return false;
                        }
                    }
                    else
                    {
                        obj = null;
                        return false;
                    }
                }

                obj = constructor.Invoke(constructorParams.ToArray());
                return true;
            }
            obj = null;
            return false;
        }
    }
}
