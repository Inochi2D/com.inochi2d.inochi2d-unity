using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inochi2D.Math;
using Inochi2D.IO;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.IO;

namespace Inochi2D {

    /// <summary>
    /// A hook for an Inochi2D Puppet instance
    /// </summary>
    public class Puppet : MonoBehaviour {

        /// <summary>
        /// Scene reference
        /// </summary>
        public Scene Scene;

        /// <summary>
        /// Instance of the internal puppet
        /// </summary>
        public InPuppet PuppetInstance;

        /// <summary>
        /// Public facing puppet information
        /// </summary>
        public PuppetMeta Info;

        [SerializeField]
        public Math.Transform VisualTransform;

        /// <summary>
        /// The Texture slots for this puppet
        /// </summary>
        [SerializeField]
        public Texture2D[] TextureSlots;

        [HideInInspector]
        public TextAsset JSONPayload;

        // Start is called before the first frame update
        void Start() {
            Inochi2D.Init();
            this.Deserialize();
        }

        private void Update() {
            // Begin the scene command buffer submission
            // We're doing this on pre-render so that the output
            // texture can be used in the actual scene.
            if (Scene != null && PuppetInstance != null) {

                Scene.Begin();
                    PuppetInstance.Update();
                    PuppetInstance.Draw(Scene);
                Scene.End();
            }
        }

        void Deserialize() {
            if (JSONPayload != null) {
                PuppetInstance = new InPuppet();
                PuppetInstance.Deserialize(JObject.Load(new JsonTextReader(new StringReader(JSONPayload.text))));
            } else {
                Debug.LogError("Malformed Puppet instance, did you use PuppetLoader or the Inspector?");
            }
        }
    }
}