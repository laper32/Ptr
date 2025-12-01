using System.Runtime.CompilerServices;

namespace Ptr.Shared.Extensions;

public static class GenericExtensions
{

    extension(object self)
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public TDest UnsafeCast<TDest>() where TDest : class
        {
            return Unsafe.As<TDest>(self);
        }

        /// <summary>
        ///     Casts an object to the specified type.
        /// </summary>
        /// <typeparam name="TDest">Destination type</typeparam>
        /// <returns>The object cast to type T.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public TDest Cast<TDest>()
        {
            return (TDest)Convert.ChangeType(self, typeof(TDest));
        }
    }
}