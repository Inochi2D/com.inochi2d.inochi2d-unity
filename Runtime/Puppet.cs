using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Inochi2D.Internal;
using Inochi2D;
using UnityEngine;
using Unity.Collections;
using Unity.VisualScripting;
using System.Runtime.InteropServices;
using Unity.Properties;

namespace Inochi2D {

    /// <summary>
    /// A puppet
    /// </summary>
    [AddComponentMenu("Inochi2D Puppet")]
    public class Puppet : MonoBehaviour {
        private InPuppet __iHandle;

        /// <summary>
        /// The binary data of the puppet
        /// </summary>
        [HideInInspector]
        public TextAsset Data;

        /// <summary>
        /// The name of the puppet
        /// </summary>
        public string Name { get { return __iHandle.Name; } }

        /// <summary>
        /// Whether physics are enabled.
        /// </summary>
        public bool PhysicsEnabled {
            get {
                return __iHandle.PhysicsEnabled;
            }
            set {
                __iHandle.PhysicsEnabled = value;
            }
        }

        /// <summary>
        /// The pixel-to-meter unit mapping for the physics system.
        /// </summary>
        public float PixelsPerMeter {
            get {
                return __iHandle.PixelsPerMeter;
            }
            set {
                __iHandle.PixelsPerMeter = value;
            }
        }

        /// <summary>
        /// The gravity constant for the puppet.
        /// </summary>
        public float Gravity {
            get {
                return __iHandle.Gravity;
            }
            set {
                __iHandle.Gravity = value;
            }
        }

        /// <summary>
        /// The textures loaded for the puppet.
        /// </summary>
        [CreateProperty]
        public NativeSlice<InTexture> Textures { get { return __iHandle.Textures; } }

        /// <summary>
        /// The puppet's parameters.
        /// </summary>
        [CreateProperty]
        public NativeSlice<InParameter> Parameters { get { return __iHandle.Parameters; } }

        public void Awake() {
            if (Data != null) {
                __iHandle = InPuppet.LoadFromMemory(Data.GetData<byte>());
            }
        }

        public void OnDestroy() {
            if (!__iHandle.IsNull)
                __iHandle.Free();
        }

        public void Update() {
            if (!__iHandle.IsNull)
                __iHandle.Update(Time.deltaTime);
        }

    }
}
