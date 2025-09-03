/**
    Inochi2D Math Helpers

    Copyright © 2025, Inochi2D Project
    Distributed under the 2-Clause BSD License, see LICENSE file.
    
    Authors: Luna Nielsen
*/
using System;
using System.Collections;
using System.Runtime.InteropServices;
using Unity.VisualScripting;
using UnityEngine;

namespace Inochi2D.Internal {

    /// <summary>
    /// A tightly packed 2D Vector
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct InVec2 {
        public float x;
        public float y;

        /// <summary>
        /// Converts a Inochi2D packed Vec2 to a Unity Vector2
        /// </summary>
        /// <returns>A Unity Vector2</returns>
        public Vector2 ToUnityVec2() {
            return new Vector2(x, y);
        }

        /// <summary>
        /// Converts a Unity Vector2 to a Inochi2D packed Vec2 
        /// </summary>
        /// <returns>An Inochi2D InVec2</returns>
        public static InVec2 FromUnityVec2(Vector2 vector) {
            return new InVec2 { x = vector.x, y = vector.y };
        }
    }
}