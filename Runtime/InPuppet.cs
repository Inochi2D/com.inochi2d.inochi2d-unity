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
    [JsonConverter(typeof(InPuppetConverter))]
    public class InPuppet : UnityEngine.Object {
        public Puppet Parent;
        public PuppetMeta Info;
        public Node Root;

        public InPuppet() { }

        public void Update() {

        }

        public void Init() {

        }

        public void UpdateFixed(float delta) {

        }
    }

    /// <summary>
    /// Inochi2D Puppet Deserializer
    /// </summary>
    public class InPuppetConverter : JsonConverter<InPuppet> {
        // UNUSED
        public override void WriteJson(JsonWriter writer, InPuppet value, JsonSerializer serializer) => throw new NotImplementedException();

        public override InPuppet ReadJson(JsonReader reader, Type objectType, InPuppet existingValue, bool hasExistingValue, JsonSerializer serializer) {
            InPuppet puppet = new InPuppet();
            JObject obj = JObject.Load(reader);

            // Read metadata
            if (obj["meta"] != null) {
                puppet.Info = obj["meta"].ToObject<PuppetMeta>();
            }

            // Read Node structure

            // Return resulting puppet data
            return puppet;
        }

        public override bool CanWrite => false;
    }
}