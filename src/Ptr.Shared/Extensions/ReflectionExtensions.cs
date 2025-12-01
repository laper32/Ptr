using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace Ptr.Shared.Extensions;

public static class ReflectionExtension
{
    public static void CheckReturnAndParameters(this MethodInfo method, Type returnType, Type[] paramsType)
    {
        if (method.ReturnParameter.ParameterType != typeof(void))
        {
            throw new BadImageFormatException("Bad return value: " + returnType.Name);
        }

        var @params = method.GetParameters();
        if (@params.Length != paramsType.Length)
        {
            throw new BadImageFormatException("Parameters count mismatch");
        }

        for (var i = 0; i < paramsType.Length; i++)
        {
            var type = @params[i].ParameterType;
            if (type != paramsType[i])
            {
                throw new BadImageFormatException("Bad parameter type: " + type.Name);
            }
        }
    }

    extension(Type type)
    {
        public void SetPublicReadOnlyField<TInstance, TValue>(string name, TInstance instance,
            TValue value)
        {
            var field = type.GetField(name, BindingFlags.Instance | BindingFlags.Public)
                        ?? throw new MissingFieldException(type.FullName, name);
            field.SetValue(instance, value);
        }

        public void SetReadOnlyField<TInstance, TValue>(string name, TInstance instance,
            TValue value)
        {
            var field = type.GetField(name, BindingFlags.Instance | BindingFlags.NonPublic)
                        ?? throw new MissingFieldException(type.FullName, name);
            field.SetValue(instance, value);
        }

        public void SetStaticReadOnlyField<TValue>(string name, TValue value)
        {
            var field = type.GetField(name, BindingFlags.Static | BindingFlags.NonPublic)
                        ?? throw new MissingFieldException(type.FullName, name);
            field.SetValue(null, value);
        }

        public void SetReadonlyProperty<TInstance, TValue>(string name, TInstance instance,
            TValue value)
        {
            var fName = $"<{name}>k__BackingField";
            var field = type.GetField(fName, BindingFlags.Instance | BindingFlags.NonPublic)
                        ?? throw new MissingFieldException(type.FullName, fName);

            field.SetValue(instance, value);
        }

        public void SetStaticReadonlyProperty<TValue>(string name, TValue value)
        {
            var fName = $"<{name}>k__BackingField";
            var field = type.GetField(fName, BindingFlags.Static | BindingFlags.NonPublic)
                        ?? throw new MissingFieldException(type.FullName, fName);

            field.SetValue(null, value);
        }
    }

    extension(IServiceProvider provider)
    {
        public IEnumerable<T> GetAllServices<T>()
        {
            return provider.GetAllServices<T>(_ => true);
        }

        // https://stackoverflow.com/questions/69836192/argumentexception-optionsmanager-cant-be-converted-to-service-type-ioptions
        public IEnumerable<T> GetAllServices<T>(Func<Type, bool> predicate)
        {
            var site = typeof(ServiceProvider)
                .GetProperty("CallSiteFactory", BindingFlags.Instance | BindingFlags.NonPublic)!
                .GetValue(provider)!;
            var desc = site
                .GetType()
                .GetField("_descriptors", BindingFlags.Instance | BindingFlags.NonPublic)!
                .GetValue(site) as ServiceDescriptor[];
            return desc!.Select(s => predicate(s.ServiceType) ? provider.GetRequiredService(s.ServiceType) : null)
                .OfType<T>();
        }
    }
}