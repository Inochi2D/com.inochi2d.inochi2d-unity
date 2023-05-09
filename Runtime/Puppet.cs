using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inochi2D.Math;
using Inochi2D.IO;

namespace Inochi2D {

    /// <summary>
    /// A hook for an Inochi2D Puppet instance
    /// </summary>
    public class Puppet : MonoBehaviour {
        public Scene Scene;
        public PuppetMeta Info;

        [SerializeField]
        public Math.Transform VisualTransform;

        /// <summary>
        /// The Texture slots for this puppet
        /// </summary>
        [SerializeField]
        public Texture2D[] TextureSlots;

        public Puppet() {
        }


        // Start is called before the first frame update
        void Start() {
            Inochi2D.Init();
        }

        // Update is called once per frame
        void Update() {

        }

        void FixedUpdate() {

        }
    }
}