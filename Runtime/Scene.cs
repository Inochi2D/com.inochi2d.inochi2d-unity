using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;

namespace Inochi2D {
    [AddComponentMenu("Inochi2D Scene")]
    public class Scene : MonoBehaviour {

        [MenuItem("GameObject/Inochi2D/Scene", false, 10)]
        static void Create(MenuCommand cmd) {
            var obj = new GameObject("Scene");
            obj.AddComponent<Scene>();

            GameObjectUtility.SetParentAndAlign(obj, cmd.context as GameObject);
            Undo.RegisterCreatedObjectUndo(obj, "Create " + obj.name);
            Selection.activeObject = obj;
        }

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start() {

        }

        // Update is called once per frame
        void Update() {

        }
    }
}