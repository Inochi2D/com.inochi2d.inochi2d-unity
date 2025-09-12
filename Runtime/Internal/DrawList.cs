using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Unity.Collections;
using UnityEngine;
using Unity.Collections.LowLevel.Unsafe;

namespace Inochi2D.Internal {

    /// <summary>
    /// Draw State flags
    /// </summary>
    public enum DrawState : uint {
        Normal = 0,
        DefineMask = 1,
        MaskedDraw = 2,
        CompositeBegin = 3,
        CompositeEnd = 4,
        CompositeBlit = 5
    }

    /// <summary>
    /// Masking modes
    /// </summary>
    public enum MaskMode : uint {
        Mask = 0,
        Dodge = 1
    }

    /// <summary>
    /// Blending modes
    /// </summary>
    public enum InBlendMode : uint {
        Normal      = 0x00,
        Multiply    = 0x01,
        Screen      = 0x02,
        Overlay     = 0x03,
        Darken      = 0x04,
        Lighten     = 0x05,
        ColorDodge  = 0x06,
        LinearDodge = 0x07,
        AddGlow     = 0x08,
        ColorBurn   = 0x09,
        HardLight   = 0x0A,
        SoftLight   = 0x0B,
        Difference  = 0x0C,
        Exclusion   = 0x0D,
        Subtract    = 0x0E,
        Inverse     = 0x0F,
        DestinationIn = 0x10,
        SourceIn    = 0x11,
        SourceOut   = 0x12,
        COUNT       = 0x13,
    }

    /// <summary>
    /// An Inochi2D Draw List
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct InDrawList {
        private void* ptr;

        /// <summary>
        /// Textures in the cache.
        /// </summary>
        public NativeSlice<DrawCmd> Commands {
            get {
                uint count = 0;
                void* outptr = in_drawlist_get_commands(ptr, ref count);
                return Inochi2D.SliceFromOwnedMemory<DrawCmd>(outptr, count, 160);
            }
        }

        /// <summary>
        /// Mesh allocations in the draw list
        /// </summary>
        public NativeSlice<DrawAllocation> Allocations {
            get {
                uint count = 0;
                DrawAllocation* outptr = in_drawlist_get_allocations(ptr, ref count);
                return Inochi2D.SliceFromOwnedMemory<DrawAllocation>(outptr, count);
            }
        }

        /// <summary>
        /// Textures in the cache.
        /// </summary>
        public NativeArray<VtxData> VertexData {
            get {
                uint size = 0;
                VtxData* outptr = in_drawlist_get_vertex_data(ptr, ref size);
                return Inochi2D.ArrayFromOwnedMemory<VtxData>(outptr, size / (uint)sizeof(VtxData));
            }
        }

        /// <summary>
        /// Textures in the cache.
        /// </summary>
        public NativeArray<uint> IndexData {
            get {
                uint size = 0;
                uint* outptr = in_drawlist_get_index_data(ptr, ref size);
                return Inochi2D.ArrayFromOwnedMemory<uint>(outptr, size / sizeof(uint));
            }
        }

        #region C FFI
        [DllImport("inochi2d", CallingConvention = CallingConvention.Cdecl)]
        private static unsafe extern bool in_drawlist_get_use_base_vertex(void* ptr);
        [DllImport("inochi2d", CallingConvention = CallingConvention.Cdecl)]
        private static unsafe extern void in_drawlist_set_use_base_vertex(void* ptr, bool value);
        [DllImport("inochi2d", CallingConvention = CallingConvention.Cdecl)]
        private static unsafe extern DrawCmd* in_drawlist_get_commands(void* ptr, ref uint count);
        [DllImport("inochi2d", CallingConvention = CallingConvention.Cdecl)]
        private static unsafe extern VtxData* in_drawlist_get_vertex_data(void* ptr, ref uint bytes);
        [DllImport("inochi2d", CallingConvention = CallingConvention.Cdecl)]
        private static unsafe extern uint* in_drawlist_get_index_data(void* ptr, ref uint bytes);
        [DllImport("inochi2d", CallingConvention = CallingConvention.Cdecl)]
        private static unsafe extern DrawAllocation* in_drawlist_get_allocations(void* ptr, ref uint count);
        
        #endregion
    }

    /// <summary>
    /// An Inochi2D Draw Command
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct DrawCmd {
        private InTexture source0;
        private InTexture source1;
        private InTexture source2;
        private InTexture source3;
        private InTexture source4;
        private InTexture source5;
        private InTexture source6;
        private InTexture source7;
        public DrawState State;
        public InBlendMode BlendMode;
        public MaskMode MaskMode;
        public uint AllocId;
        public uint VtxOffset;
        public uint IdxOffset;
        public uint ElemCount;
        public uint Type;
        public fixed byte Data[64];

        public Span<InTexture> Sources => new Span<InTexture>(new[] { source0, source1, source2, source3, source4, source5, source6, source7 });
    }

    /// <summary>
    /// Inochi2D Vertex Data
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct VtxData {
        public Vector2 vtx;
        public Vector2 uvs;
    }

    /// <summary>
    /// Inochi2D Vertex Data
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct DrawAllocation {
        public uint VtxOffset;
        public uint IdxOffset;
        public uint IdxCount;
        public uint VtxCount;
        public uint AllocId;
    }
}