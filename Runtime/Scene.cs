using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;
using NUnit.Framework;
using System.Collections.Generic;

namespace Inochi2D {
    [AddComponentMenu("Inochi2D Scene")]
    public class Scene : MonoBehaviour {

        /// <summary>
        /// Active Puppets
        /// </summary>
        public List<Puppet> Puppets;

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

            // Make a copy of the puppets since we'll essentially be overwriting them.
            var startPuppets = new List<Puppet>(Puppets);

            // Instantiate any puppet prefabs that were added to the scene.
            // Move any which were referenced.
            foreach(var puppet in startPuppets) {
                if (PrefabUtility.GetPrefabAssetType(puppet) != PrefabAssetType.NotAPrefab) {
                    PrefabUtility.InstantiatePrefab(puppet, this.transform);
                } else {
                    puppet.transform.parent = this.transform;
                }
            }
        }

        // Update is called once per frame
        void Update() {

        }

        /// <summary>
        /// Automatically add children dragged into this scene.
        /// </summary>
        void OnTransformChildrenChanged() {
            Puppets.Clear();
            Puppets.AddRange(this.GetComponentsInChildren<Puppet>(true));
        }
    }
}