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
        /// Channels in the texture.
        /// </summary>
        public IntPtr Ptr { get => new IntPtr(ptr); }

        /// <summary>
        /// ID of the texture
        /// </summary>
        public int ID {
            get => (int)in_resource_get_id(ptr);
            set {
                in_resource_set_id(ptr, (void*)value);
            }
        }

        /// <summary>
        /// Whether the texture is null
        /// </summary>
        public readonly bool IsNull => ptr == null;

        /// <summary>
        /// Gets a span over the pixels of the texture.
        /// </summary>
        public NativeArray<byte> Pixels {
            get {
                return Inochi2D.ArrayFromOwnedMemory<byte>(in_texture_get_pixels(ptr), in_resource_get_length(ptr));
            }
        }

        /// <summary>
        /// Pre-multiplies the texture.
        /// </summary>
        public void Premultiply() {
            in_texture_premultiply(ptr);
        }

        /// <summary>
        /// Pre-multiplies the texture.
        /// </summary>
        public void Unpremultiply() {
            in_texture_unpremultiply(ptr);
        }

        /// <summary>
        /// Pads the texture with a border.
        /// </summary>
        public void Pad(uint thickness) {
            in_texture_pad(ptr, thickness);
        }

        /// <summary>
        /// Creates a Texture2D from a InTexture
        /// </summary>
        /// <returns>InTexture</returns>
        public Texture2D ToTexture2D() {
            NativeArray<byte> pixels = Pixels;
            Texture2D result = null;
            switch (Channels) {
                case 1:
                    result = new Texture2D((int)Width, (int)Height, TextureFormat.Alpha8, true);
                    break;

                case 4:
                    result = new Texture2D((int)Width, (int)Height, TextureFormat.RGBA32, true, true);
                    break;
            }

            // Set data for texture.
            if (pixels.IsCreated) {
                result.SetPixelData<byte>(pixels, 0, 0);
                result.wrapMode = TextureWrapMode.Clamp;
                result.filterMode = FilterMode.Bilinear;
                result.alphaIsTransparency = true;
                result.Apply(updateMipmaps: true);
            }
            return result;
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
        private static extern void in_texture_premultiply(void* obj);

        [DllImport("inochi2d", CallingConvention = CallingConvention.Cdecl)]
        private static extern void in_texture_unpremultiply(void* obj);

        [DllImport("inochi2d", CallingConvention = CallingConvention.Cdecl)]
        private static extern void in_texture_pad(void* obj, uint thickness);

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