using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.ExceptionServices;
using UnityEngine;

namespace Inochi2D.IO {
    public static class PuppetLoader {

        /// <summary>
        /// Creates a new Puppet object from a path
        /// </summary>
        /// <param name="path">Path to file</param>
        /// <param name="premultiply">Whether to premultiply textures on load</param>
        /// <returns>The loaded puppet</returns>
        public static Puppet CreatePuppetFromPath(string path, bool premultiply = true) {
            return CreatePuppetFromStream(new FileStream(path, FileMode.Open), premultiply);
        }

        /// <summary>
        /// Creates a new Puppet object from a stream
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="premultiply">Whether to premultiply textures on load</param>
        /// <returns>The loaded puppet</returns>
        public static Puppet CreatePuppetFromStream(Stream stream, bool premultiply = true) {
            INPReader reader = new INPReader(stream);
            try {
                reader.ReadFile();

                var metadata = JsonConvert.DeserializeObject<PuppetMetaContainer>(reader.PayloadJSON);

                // Create game object with puppet reference
                var rootObject = new GameObject();

                // Create puppet and pass in payload
                var puppet = rootObject.AddComponent<Puppet>();
                puppet.Info = metadata.Meta;
                puppet.JSONPayload = new TextAsset(reader.PayloadJSON);

                // Load textures and pass them in to the puppet
                List<Texture2D> textures = new List<Texture2D>();
                int index = 0;
                foreach (var texture in reader.Textures) {

                    // Add Texture
                    var tex = texture.ToTexture2D(premultiply, metadata.Meta.PreservePixels);
                    tex.name = $"Texture{index}";
                    textures.Add(tex);

                    index++;
                }
                puppet.TextureSlots = textures.ToArray();

                reader.Close();
                return puppet;
            } catch (Exception ex) {
                reader.Close();
                ExceptionDispatchInfo.Capture(ex).Throw();
                throw;
            }
        }
    }
}