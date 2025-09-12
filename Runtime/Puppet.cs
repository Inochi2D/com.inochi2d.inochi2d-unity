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
        private InPuppet handle;

        /// <summary>
        /// The binary data of the puppet
        /// </summary>
        [HideInInspector]
        public TextAsset Data;

        /// <summary>
        /// The name of the puppet
        /// </summary>
        public string Name { get { return handle.Name; } }

        /// <summary>
        /// The renderer in charge of this puppet.
        /// </summary>
        public PuppetRenderer Renderer;

        /// <summary>
        /// Whether physics are enabled.
        /// </summary>
        public bool PhysicsEnabled {
            get {
                return handle.PhysicsEnabled;
            }
            set {
                handle.PhysicsEnabled = value;
            }
        }

        /// <summary>
        /// The pixel-to-meter unit mapping for the physics system.
        /// </summary>
        public float PixelsPerMeter {
            get {
                return handle.PixelsPerMeter;
            }
            set {
                handle.PixelsPerMeter = value;
            }
        }

        /// <summary>
        /// The gravity constant for the puppet.
        /// </summary>
        public float Gravity {
            get {
                return handle.Gravity;
            }
            set {
                handle.Gravity = value;
            }
        }

        /// <summary>
        /// The textures loaded for the puppet.
        /// </summary>
        [CreateProperty]
        public NativeSlice<InTexture> Textures { get { return handle.Textures; } }

        /// <summary>
        /// The puppet's parameters.
        /// </summary>
        [CreateProperty]
        public NativeSlice<InParameter> Parameters { get { return handle.Parameters; } }

        public void Awake() {
            Renderer.Awake();
            if (Data != null) {
                handle = InPuppet.LoadFromMemory(Data.GetData<byte>());
                Renderer.Setup(handle);
            }
        }

        public void OnDestroy() {
            if (!handle.IsNull)
                handle.Free();

            if (Renderer != null) {
                Renderer.Dispose();
                Renderer = null;
            }
        }

        public void Update() {
            Renderer?.Draw();
        }
    }
}
