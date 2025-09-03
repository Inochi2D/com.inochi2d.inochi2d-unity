using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Security;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.VisualScripting;
using UnityEngine;

namespace Inochi2D.Internal {

    /// <summary>
    /// An Inochi2D Parameter
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct InTexture {
        private void* ptr;

        private void finalize() {
            if (in_resource_get_id(ptr) == null) {
                in_texture_flip_vertically(ptr);

                NativeArray<byte> pixels = Pixels;
                Texture2D result = null;
                switch (Channels) {
                    case 1:
                        result = new Texture2D((int)Width, (int)Height, TextureFormat.Alpha8, true);
                        break;

                    case 4:
                        result = new Texture2D((int)Width, (int)Height, TextureFormat.RGBA32, true);
                        break;
                }

                // Set data for texture.
                if (pixels.IsCreated) {
                    result.SetPixelData<byte>(pixels, 0, 0);
                    result.Apply(updateMipmaps: true);
                }

                GCHandle handle = GCHandle.Alloc(result);
                in_resource_set_id(ptr, GCHandle.ToIntPtr(handle).ToPointer());
            }
        }

        /// <summary>
        /// Size of the data in bytes.
        /// </summary>
        public uint Length { get { return in_resource_get_length(ptr); } }

        /// <summary>
        /// Width of the texture in pixels.
        /// </summary>
        public uint Width { get { return in_texture_get_width(ptr); } }

        /// <summary>
        /// Height of the texture in pixels.
        /// </summary>
        public uint Height { get { return in_texture_get_height(ptr); } }

        /// <summary>
        /// Channels in the texture.
        /// </summary>
        public uint Channels { get { return in_texture_get_channels(ptr); } }

        /// <summary>
        /// Unity Texture that belongs to this texture.
        /// </summary>
        public Texture2D Texture {
            get {
                if (in_resource_get_id(ptr) == null)
                    this.finalize();
                
                GCHandle handle = GCHandle.FromIntPtr(new IntPtr(in_resource_get_id(ptr)));
                return handle.Target as Texture2D;
            }
        }

        /// <summary>
        /// Gets a span over the pixels of the texture.
        /// </summary>
        public NativeArray<byte> Pixels {
            get {
                return Inochi2D.ArrayFromOwnedMemory<byte>(in_texture_get_pixels(ptr), in_resource_get_length(ptr));
            }
        }

        /// <summary>
        /// Frees the texture's ID resources.
        /// </summary>
        public void Free() {
            if (in_resource_get_id(ptr) != null) {
                GCHandle handle = GCHandle.FromIntPtr(new IntPtr(in_resource_get_id(ptr)));
                handle.Free();

                in_resource_set_id(ptr, null);
            }
        }

        #region C FFI

        [DllImport("inochi2d", CallingConvention = CallingConvention.Cdecl)]
        private static extern uint in_resource_get_length(void* obj);

        [DllImport("inochi2d", CallingConvention = CallingConvention.Cdecl)]
        private static extern void* in_resource_get_id(void* obj);

        [DllImport("inochi2d", CallingConvention = CallingConvention.Cdecl)]
        private static extern void in_resource_set_id(void* obj, void* id);
        [DllImport("inochi2d", CallingConvention = CallingConvention.Cdecl)]
        private static extern uint in_texture_get_width(void* obj);

        [DllImport("inochi2d", CallingConvention = CallingConvention.Cdecl)]
        private static extern uint in_texture_get_height(void* obj);

        [DllImport("inochi2d", CallingConvention = CallingConvention.Cdecl)]
        private static extern uint in_texture_get_channels(void* obj);

        [DllImport("inochi2d", CallingConvention = CallingConvention.Cdecl)]
        private static extern void in_texture_flip_vertically(void* obj);

        [DllImport("inochi2d", CallingConvention = CallingConvention.Cdecl)]
        private static unsafe extern void* in_texture_get_pixels(void* obj);
        #endregion

    }

    /// <summary>
    /// Cache of textures.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct InTextureCache {
        private void* ptr;

        /// <summary>
        /// Gets the amount of textures stored in the cache.
        /// </summary>
        public uint Count { get { return in_texture_cache_get_size(ptr); } }

        /// <summary>
        /// Textures in the cache.
        /// </summary>
        public NativeSlice<InTexture> Textures {
            get {
                uint count = 0;
                InTexture* texptr = in_texture_cache_get_textures(ptr, ref count);
                return Inochi2D.SliceFromOwnedMemory<InTexture>(texptr, count);
            }
        }

        /// <summary>
        /// Gets a Texture from the cache.
        /// </summary>
        /// <param name="slot">The slot of the texture</param>
        /// <returns>The requested texture or null</returns>
        public InTexture Get(uint slot) {
            return in_texture_cache_get_texture(ptr, slot);
        }

        /// <summary>
        /// Prunes the texture cache.
        /// </summary>
        public void Prune() {
            in_texture_cache_prune(ptr);
        }

        #region C FFI

        [DllImport("inochi2d", CallingConvention = CallingConvention.Cdecl)]
        private static extern uint in_texture_cache_get_size(void* obj);

        [DllImport("inochi2d", CallingConvention = CallingConvention.Cdecl)]
        private static extern InTexture in_texture_cache_get_texture(void* obj, uint slot);

        [DllImport("inochi2d", CallingConvention = CallingConvention.Cdecl)]
        private static extern InTexture* in_texture_cache_get_textures(void* obj, ref uint count);

        [DllImport("inochi2d", CallingConvention = CallingConvention.Cdecl)]
        private static extern void in_texture_cache_prune(void* obj);
        #endregion

    }
}