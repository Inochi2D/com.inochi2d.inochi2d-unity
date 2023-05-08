using UnityEditor;
using UnityEditor.AssetImporters;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Inochi2D {
    [CustomEditor(typeof(INPImporter))]
    public class INPImporterEditor : ScriptedImporterEditor {

        public override void OnInspectorGUI() {
            var property = serializedObject.FindProperty("ErrorOnVersionUnsupported");
            EditorGUILayout.PropertyField(property, new GUIContent("Enforce Version Support"));

            property = serializedObject.FindProperty("PremultiplyTextures");
            EditorGUILayout.PropertyField(property, new GUIContent("Premultiply Textures"));
            base.ApplyRevertGUI();
        }
    }
}