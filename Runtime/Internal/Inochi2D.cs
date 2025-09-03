using System;
using System.Collections;
using System.Runtime.InteropServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.VisualScripting;
using UnityEngine;

namespace Inochi2D.Internal {

    /// <summary>
    /// Wraps reference counted types in Inochi2D.
    /// </summary>
    public abstract class InRefCounted {
        protected UIntPtr ptr;

        // Destructor
        ~InRefCounted() {
            in_release(ptr);
            this.ptr = UIntPtr.Zero;
        }

        /// <summary>
        /// Constructs a new refcounted object
        /// </summary>
        /// <param name="obj">Pointer to object.</param>
        protected InRefCounted(UIntPtr obj) {
            in_retain(obj);
            this.ptr = obj;
        }

        #region C FFI
        [DllImport("inochi2d", CallingConvention = CallingConvention.Cdecl)]
        private static extern UIntPtr in_retain(UIntPtr obj);

        [DllImport("inochi2d", CallingConvention = CallingConvention.Cdecl)]
        private static extern UIntPtr in_release(UIntPtr obj);
        #endregion
    }

    public static class Inochi2D {

        /// <summary>
        /// Helper which converts an Inochi2D pointer to a
        /// NativeSlice for better performance.
        /// </summary>
        /// <typeparam name="T">The type of the basic Inochi2D Type</typeparam>
        /// <param name="ptr">Pointer to the array (inochi2d_xyz_t**)</param>
        /// <param name="count">The count of elements</param>
        /// <returns>The type converted to a slice</returns>
        public static unsafe NativeSlice<T> SliceFromOwnedMemory<T>(void* ptr, uint count) where T : struct {
            var _data = NativeSliceUnsafeUtility.ConvertExistingDataToNativeSlice<T>(ptr, Marshal.SizeOf<UIntPtr>(), (int)count);
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            NativeSliceUnsafeUtility.SetAtomicSafetyHandle(ref _data, AtomicSafetyHandle.GetTempUnsafePtrSliceHandle());
#endif

            return _data;
        }

        /// <summary>
        /// Helper which converts an Inochi2D pointer to a
        /// NativeArray for better performance.
        /// </summary>
        /// <typeparam name="T">The type of the basic Inochi2D Type</typeparam>
        /// <param name="ptr">Pointer to the array (inochi2d_xyz_t**)</param>
        /// <param name="count">The count of elements</param>
        /// <returns>The type converted to an array</returns>
        public static unsafe NativeArray<T> ArrayFromOwnedMemory<T>(void* ptr, uint count) where T : struct {
            var _data = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<T>(ptr, (int)count, Allocator.None);
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            NativeArrayUnsafeUtility.SetAtomicSafetyHandle(ref _data, AtomicSafetyHandle.GetTempUnsafePtrSliceHandle());
#endif
            return _data;
        }
    }
}