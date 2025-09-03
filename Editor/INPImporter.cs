using UnityEngine;
using UnityEditor.AssetImporters;
using System.IO;
using System.Collections.Generic;
using UnityEditor;
using System.Reflection;
using System;
using Inochi2D.Internal;

namespace Inochi2D.Editor {

    [ScriptedImporter(1, new[] { "inp", "inx" })]
    class INPImporter : ScriptedImporter {

        public override void OnImportAsset(AssetImportContext ctx) {
            TextAsset data = new TextAsset(File.ReadAllBytes(ctx.assetPath).AsSpan<byte>());
            Texture2D icon = Resources.Load<Texture2D>("i2d-logo");

            data.name = Path.GetFileName(ctx.assetPath);
            ctx.AddObjectToAsset("Data", data);

            GameObject obj = new GameObject("Puppet", typeof(Puppet));
            Puppet p = obj.GetComponent<Puppet>();
            p.Data = data;

            ctx.AddObjectToAsset("Puppet", obj, icon);
            ctx.SetMainObject(obj);
        }
    }

}