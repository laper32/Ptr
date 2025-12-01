using Ptr.Shared.Bridge;
using Sharp.Shared.Objects;

namespace Ptr.Shared.Extensions;

public static class KeyValuesExtensions
{
    extension(IKeyValues kv)
    {
        public unsafe IKeyValues? GetNextValue()
        {
            var symbolName = OperatingSystem.IsWindows()
                ? "?GetNextValue@KeyValues@@QEAAPEAV1@XZ"
                : "_ZN9KeyValues12GetNextValueEv";
            var func = InterfaceBridge.Instance.LibraryModuleManager.Tier0.GetFunctionByName(symbolName);
            var call = (delegate* unmanaged<nint, nint>)func;
            var value = call(kv.GetAbsPtr());
            return InterfaceBridge.Instance.ModSharp.CreateNativeObject<IKeyValues>(value);
        }

        public unsafe IKeyValues? GetFirstValue()
        {
            var symbolName = OperatingSystem.IsWindows()
                ? "?GetFirstValue@KeyValues@@QEAAPEAV1@XZ"
                : "_ZN9KeyValues13GetFirstValueEv";
            var fn = InterfaceBridge.Instance.LibraryModuleManager.Tier0.GetFunctionByName(symbolName);
            var call = (delegate* unmanaged<nint, nint>)fn;
            var ret = call(kv.GetAbsPtr());
            return InterfaceBridge.Instance.ModSharp.CreateNativeObject<IKeyValues>(ret);
        }
    }
}