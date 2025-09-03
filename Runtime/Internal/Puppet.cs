using System;
using System.Collections;
using System.Runtime.InteropServices;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Collections;
using Unity.VisualScripting;
using UnityEngine;
using System.Collections.Generic;

namespace Inochi2D.Internal {

    /// <summary>
    /// Internal Puppet Implementation Details
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct InPuppet {
        private void* ptr;

        /// <summary>
        /// Loads a puppet from a file.
        /// </summary>
        /// <param name="file">The file to load from</param>
        public static InPuppet LoadFromFile(string file) {
            unsafe {
                var ptr = in_puppet_load(file);
                if (ptr == null)
                    throw new Exception(Marshal.PtrToStringUTF8(in_get_last_error()));

                return new InPuppet { ptr = ptr };
            }
        }

        /// <summary>
        /// Loads a puppet from a file.
        /// </summary>
        /// <param name="file">The file to load from</param>
        public static InPuppet LoadFromMemory(NativeSlice<byte> data) {
            unsafe {
                var ptr = in_puppet_load_from_memory(data.GetUnsafePtr(), (uint)data.Length);
                if (ptr == null)
                    throw new Exception(Marshal.PtrToStringUTF8(in_get_last_error()));

                return new InPuppet { ptr = ptr };
            }
        }

        /// <summary>
        /// The name of the puppet
        /// </summary>
        public string Name { get { return Marshal.PtrToStringUTF8(in_puppet_get_name(ptr)); } }

        /// <summary>
        /// Whether physics are enabled.
        /// </summary>
        public bool PhysicsEnabled {
            get {
                return in_puppet_get_physics_enabled(ptr);
            }
            set {
                in_puppet_set_physics_enabled(ptr, value);
                in_puppet_reset_drivers(ptr);
            }
        }

        /// <summary>
        /// The pixel-to-meter unit mapping for the physics system.
        /// </summary>
        public float PixelsPerMeter {
            get {
                return in_puppet_get_pixels_per_meter(ptr);
            }
            set {
                in_puppet_set_pixels_per_meter(ptr, value);
            }
        }

        /// <summary>
        /// The gravity constant for the puppet.
        /// </summary>
        public float Gravity {
            get {
                return in_puppet_get_gravity(ptr);
            }
            set {
                in_puppet_set_gravity(ptr, value);
            }
        }

        /// <summary>
        /// The textures loaded for the puppet.
        /// </summary>
        public NativeSlice<InTexture> Textures {
            get {
                return in_puppet_get_texture_cache(ptr).Textures;
            }
        }

        /// <summary>
        /// The textures loaded for the puppet.
        /// </summary>
        public NativeSlice<InParameter> Parameters {
            get {
                uint _count = 0;
                InParameter* _data = in_puppet_get_parameters(ptr, ref _count);
                return Inochi2D.SliceFromOwnedMemory<InParameter>(_data, _count);
            }
        }

        /// <summary>
        /// Whether the puppet is null.
        /// </summary>
        public bool IsNull { get { return ptr == null; } }

        /// <summary>
        /// Updates the puppet.
        /// </summary>
        /// <param name="delta">Time since last frame</param>
        public void Update(float delta) {
            in_puppet_update(ptr, delta);
            in_puppet_draw(ptr, delta);
        }

        /// <summary>
        /// Frees the puppet.
        /// </summary>
        public void Free() {
            foreach (var texture in this.Textures) {
                texture.Free();
            }
            in_puppet_free(ptr);
            ptr = null;
        }

        #region C FFI

        [DllImport("inochi2d", CallingConvention = CallingConvention.Cdecl)]
        private static unsafe extern IntPtr in_get_last_error();
        [DllImport("inochi2d", CallingConvention = CallingConvention.Cdecl)]
        private static unsafe extern void* in_puppet_load_from_memory(void* data, uint length);

        [DllImport("inochi2d", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        private static unsafe extern void* in_puppet_load(string file);

        [DllImport("inochi2d", CallingConvention = CallingConvention.Cdecl)]
        private static extern void in_puppet_free(void* ptr);

        [DllImport("inochi2d", CallingConvention = CallingConvention.Cdecl)]
        private static extern void in_puppet_update(void* ptr, float delta);

        [DllImport("inochi2d", CallingConvention = CallingConvention.Cdecl)]
        private static extern void in_puppet_draw(void* ptr, float delta);

        [DllImport("inochi2d", CallingConvention = CallingConvention.Cdecl)]
        private static extern InTextureCache in_puppet_get_texture_cache(void* ptr);

        [DllImport("inochi2d", CallingConvention = CallingConvention.Cdecl)]
        private static unsafe extern InParameter* in_puppet_get_parameters(void* ptr, ref uint count);

        [DllImport("inochi2d", CallingConvention = CallingConvention.Cdecl)]
        private static extern void in_puppet_reset_drivers(void* ptr);

        [DllImport("inochi2d", CallingConvention = CallingConvention.Cdecl)]
        private static unsafe extern IntPtr in_puppet_get_name(void* ptr);

        [DllImport("inochi2d", CallingConvention = CallingConvention.Cdecl)]
        private static unsafe extern bool in_puppet_get_physics_enabled(void* ptr);

        [DllImport("inochi2d", CallingConvention = CallingConvention.Cdecl)]
        private static unsafe extern void in_puppet_set_physics_enabled(void* ptr, bool value);

        [DllImport("inochi2d", CallingConvention = CallingConvention.Cdecl)]
        private static unsafe extern float in_puppet_get_pixels_per_meter(void* ptr);

        [DllImport("inochi2d", CallingConvention = CallingConvention.Cdecl)]
        private static unsafe extern void in_puppet_set_pixels_per_meter(void* ptr, float value);

        [DllImport("inochi2d", CallingConvention = CallingConvention.Cdecl)]
        private static unsafe extern float in_puppet_get_gravity(void* ptr);

        [DllImport("inochi2d", CallingConvention = CallingConvention.Cdecl)]
        private static unsafe extern void in_puppet_set_gravity(void* ptr, float value);
        #endregion

    }
}