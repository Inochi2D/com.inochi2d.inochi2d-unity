using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Inochi2D.Internal {
    
    /// <summary>
    /// An Inochi2D Parameter
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct InParameter {
        private void* ptr;

        /// <summary>
        /// Name of the parameter.
        /// </summary>
        public string Name { get { return Marshal.PtrToStringUTF8(in_parameter_get_name(ptr)); } }

        /// <summary>
        /// Whether the parameter is active.
        /// </summary>
        public bool Active { get { return in_parameter_get_active(ptr); } }

        /// <summary>
        /// How many dimensions the parameter has.
        /// </summary>
        public uint Dimensions { get { return in_parameter_get_dimensions(ptr); } }

        /// <summary>
        /// The parameter's minimum value.
        /// </summary>
        public Vector2 Min { get { return in_parameter_get_min_value(ptr).ToUnityVec2(); } }

        /// <summary>
        /// The parameter's maximum value.
        /// </summary>
        public Vector2 Max { get { return in_parameter_get_max_value(ptr).ToUnityVec2(); } }

        /// <summary>
        /// The parameter's current value.
        /// </summary>
        [SerializeField]
        public Vector2 Value {
            get {
                return in_parameter_get_value(ptr).ToUnityVec2();
            }
            set {
                in_parameter_set_value(ptr, InVec2.FromUnityVec2(value));
            }
        }

        /// <summary>
        /// The parameter's current value normalized to 
        /// a range of 0..1
        /// </summary>
        [SerializeField]
        public Vector2 Normalized {
            get {
                return in_parameter_get_normalized_value(ptr).ToUnityVec2();
            }
            set {
                in_parameter_set_normalized_value(ptr, InVec2.FromUnityVec2(value));
            }
        }

        /// <summary>
        /// Prints a text representation of the parameter.
        /// </summary>
        /// <returns></returns>
        public override string ToString() {
            return $"{Name} ({Min.x}..{Max.x}, {Min.y}..{Max.y}) <{Value.x}, {Value.y}>";
        }

        #region C FFI
        [DllImport("inochi2d", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        private static unsafe extern IntPtr in_parameter_get_name(void* ptr);

        [DllImport("inochi2d", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        private static unsafe extern bool in_parameter_get_active(void* ptr);

        [DllImport("inochi2d", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        private static unsafe extern uint in_parameter_get_dimensions(void* ptr);

        [DllImport("inochi2d", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        private static unsafe extern InVec2 in_parameter_get_min_value(void* ptr);

        [DllImport("inochi2d", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        private static unsafe extern InVec2 in_parameter_get_max_value(void* ptr);

        [DllImport("inochi2d", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        private static unsafe extern InVec2 in_parameter_get_value(void* ptr);

        [DllImport("inochi2d", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        private static unsafe extern void in_parameter_set_value(void* ptr, InVec2 value);

        [DllImport("inochi2d", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        private static unsafe extern InVec2 in_parameter_get_normalized_value(void* ptr);

        [DllImport("inochi2d", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        private static unsafe extern void in_parameter_set_normalized_value(void* ptr, InVec2 value);
        #endregion
    }
}
