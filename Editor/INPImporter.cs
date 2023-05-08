using UnityEngine;
using UnityEditor.AssetImporters;
using System.IO;
using Inochi2D.IO;
using System.Collections.Generic;
using UnityEditor;
using System.Reflection;
using System;

namespace Inochi2D {

    [ScriptedImporter(1, new[] { "inp", "inx" })]
    class INPImporter : ScriptedImporter {
        public bool ErrorOnVersionUnsupported;
        public bool IgnoreUnknownNodes;
        public bool PremultiplyTextures = true;

        public override void OnImportAsset(AssetImportContext ctx) {
            INPReader reader = new INPReader(ctx.assetPath);
            try {
                reader.ReadFile();

                // Create game object with puppet reference
                var rootObject = new GameObject();
                var puppet = rootObject.AddComponent<Puppet>();

                List<Texture2D> textures = new List<Texture2D>();
                int index = 0;
                foreach (var texture in reader.Textures) {

                    // Add Texture
                    var tex = texture.ToTexture2D(PremultiplyTextures);
                    tex.name = $"Texture{index}";
                    ctx.AddObjectToAsset(tex.name, tex, tex);
                    textures.Add(tex);

                    index++;
                }
                puppet.TextureSlots = textures.ToArray();

                ctx.AddObjectToAsset("Puppet", rootObject, Resources.Load<Texture2D>("i2d-logo"));
                ctx.SetMainObject(rootObject);

                reader.Close();
            } catch(Exception ex) {
                reader.Close();
                throw ex;
            }
        }
    }

}