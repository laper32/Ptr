using Ptr.Shared.Extensions;

namespace Ptr.Shared.Hosting;

public static class ServiceProviderExtensions
{
    extension(IServiceProvider self)
    {
        public void CallInit<T>(Action<Exception> onError) where T : IBaseInterface
        {
            self.CallInit<T>(type => type is { IsInterface: true, ContainsGenericParameters: false }, onError);
        }

        public void CallInit<T>(Func<Type, bool> typeFilterPredicate, Action<Exception> onError)
            where T : IBaseInterface
        {
            foreach (var service in self.GetAllServices<T>(typeFilterPredicate))
            {
                try
                {
                    service.OnInit();
                }
                catch (Exception e)
                {
                    onError(e);
                }
            }
        }

        public void CallPostInit<T>(Action<Exception> onError) where T : IBaseInterface
        {
            self.CallPostInit<T>(type => type is { IsInterface: true, ContainsGenericParameters: false }, onError);
        }

        public void CallPostInit<T>(Func<Type, bool> typeFilterPredicate, Action<Exception> onError)
            where T : IBaseInterface
        {
            foreach (var service in self.GetAllServices<T>(typeFilterPredicate))
            {
                try
                {
                    service.OnPostInit();
                }
                catch (Exception e)
                {
                    onError(e);
                }
            }
        }

        public void CallAllModulesLoaded<T>(Action<Exception> onError) where T : IBaseInterface
        {
            self.CallAllModulesLoaded<T>(type => type is { IsInterface: true, ContainsGenericParameters: false },
                onError);
        }

        public void CallAllModulesLoaded<T>(Func<Type, bool> typeFilterPredicate, Action<Exception> onError)
            where T : IBaseInterface
        {
            foreach (var service in self.GetAllServices<T>(typeFilterPredicate))
            {
                try
                {
                    service.OnAllModulesLoaded();
                }
                catch (Exception e)
                {
                    onError(e);
                }
            }
        }

        public void CallLibraryConnected<T>(string libName, Action<Exception> onError) where T : IBaseInterface
        {
            self.CallLibraryConnected<T>(libName,
                type => type is { IsInterface: true, ContainsGenericParameters: false }, onError);
        }

        public void CallLibraryConnected<T>(string libName, Func<Type, bool> typeFilterPredicate,
            Action<Exception> onError) where T : IBaseInterface
        {
            foreach (var service in self.GetAllServices<T>(typeFilterPredicate))
            {
                try
                {
                    service.OnLibraryConnected(libName);
                }
                catch (Exception e)
                {
                    onError(e);
                }
            }
        }

        public void CallLibraryDisconnect<T>(string libName, Action<Exception> onError) where T : IBaseInterface
        {
            self.CallLibraryDisconnect<T>(libName,
                type => type is { IsInterface: true, ContainsGenericParameters: false }, onError);
        }

        public void CallLibraryDisconnect<T>(string libName, Func<Type, bool> typeFilterPredicate,
            Action<Exception> onError) where T : IBaseInterface
        {
            foreach (var service in self.GetAllServices<T>(typeFilterPredicate))
            {
                try
                {
                    service.OnLibraryDisconnect(libName);
                }
                catch (Exception e)
                {
                    onError(e);
                }
            }
        }

        public void CallShutdown<T>(Action<Exception> onError) where T : IBaseInterface
        {
            self.CallShutdown<T>(type => type is { IsInterface: true, ContainsGenericParameters: false }, onError);
        }

        public void CallShutdown<T>(Func<Type, bool> typeFilterPredicate, Action<Exception> onError)
            where T : IBaseInterface
        {
            foreach (var service in self.GetAllServices<T>(typeFilterPredicate))
            {
                try
                {
                    service.OnShutdown();
                }
                catch (Exception e)
                {
                    onError(e);
                }
            }
        }
    }
}