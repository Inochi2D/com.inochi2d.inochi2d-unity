using System.Collections;
using UnityEngine;
using Inochi2D.Nodes;

namespace Inochi2D {
    public static class Inochi2D {
        private static bool mHasInitialized = false;

        public static void Init() {
            if (mHasInitialized) return;

            mHasInitialized = true;

            // Register all the base types
            NodeFactoryRegistry.Register("Node", (Node parent) => { return new Node(0, parent); });
        }
    }
}