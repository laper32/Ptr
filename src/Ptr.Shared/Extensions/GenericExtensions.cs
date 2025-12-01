using System.Runtime.CompilerServices;

namespace Ptr.Shared.Extensions;

public static class GenericExtensions
{

    extension(object self)
    {
        /// <summary>
        ///     Performs a direct cast operation without boxing, equivalent to (TDest)self.
        ///     Works with value types, enums, and reference types.
        /// </summary>
        /// <typeparam name="TDest">Destination type</typeparam>
        /// <returns>The object cast to type TDest.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public TDest Cast<TDest>()
        {
            // Use Unsafe.As for reinterpretation without boxing
            // This works for same-size types (e.g., ushort to EconItemId enum)
            return Unsafe.As<object, TDest>(ref Unsafe.AsRef(in self));
        }

        /// <summary>
        ///     Converts an object to the specified type using Convert.ChangeType.
        ///     Use Cast() for direct casts (including enums).
        /// </summary>
        /// <typeparam name="TDest">Destination type</typeparam>
        /// <returns>The object converted to type TDest.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public TDest Convert<TDest>()
        {
            return (TDest)System.Convert.ChangeType(self, typeof(TDest));
        }
    }
}