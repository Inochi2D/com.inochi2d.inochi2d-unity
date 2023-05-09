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

            // Make sure Inochi2D is initialized
            Inochi2D.Init();

            if (ctx.assetPath.EndsWith("inx")) Debug.LogWarning($"{ctx.assetPath} is an Inochi Creator project file, some features will be disabled...");

            Puppet puppet = PuppetLoader.CreatePuppetFromPath(ctx.assetPath);
            foreach(var tex in puppet.TextureSlots) {
                ctx.AddObjectToAsset(tex.name, tex, tex);
            }

            // Add Puppet object to asset.
            ctx.AddObjectToAsset("Puppet", puppet.gameObject, Resources.Load<Texture2D>("i2d-logo"));
            ctx.SetMainObject(puppet.gameObject);
        }
    }

}