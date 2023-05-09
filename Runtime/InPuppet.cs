using Inochi2D.Nodes;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using UnityEngine;

namespace Inochi2D {

    /// <summary>
    /// Inochi2D Puppet Instance
    /// </summary>
    public class InPuppet {
        public Puppet Parent;
        public Node Root;

        public Math.Transform VisualTransform;

        public InPuppet() { }

        public void Update() {
            Root.BeginUpdate();
            Root.Update();
        }

        public void Draw(Scene scene) {
            Root.Draw(scene);
        }
    
        public void Deserialize(JObject obj) {

            Root = new Node(this);
            if (obj["nodes"] != null) Root.Deserialize(obj["nodes"]);

        }
    }
}