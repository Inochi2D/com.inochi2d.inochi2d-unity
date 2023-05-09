using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Inochi2D.Nodes {
    public class Node {
        #region Private Members
        private InPuppet mPuppet;
        private Node mParent;
        private List<Node> mChildren = new List<Node>();
        private uint mUUID = 0;
        private float mZSort = 0;
        private bool mLockToRoot = false;

        private bool mRecalculateTransform = true;
        #endregion

        #region Protected Members
        protected bool pmPreProcessed;
        protected bool pmPostProcessed;
        protected float pmOffsetSort = 0f;
        protected Math.Transform pmOffsetTransform;

        /// <summary>
        /// Send mask reset request one node up
        /// </summary>
        protected void ResetMask() {
            if (mParent != null) mParent.ResetMask();
        }

        /// <summary>
        /// One-time Transform
        /// </summary>
        protected Matrix4x4 OneTimeTransform;

        /// <summary>
        /// Nullable one-time Transform
        /// </summary>
        protected Matrix4x4? OverrideTransformMatrix;

        protected delegate Tuple<Vector2[], Matrix4x4?> PreProcessFilterFunc(Vector2[] local, Vector2[] offset, Matrix4x4? matrix);
        PreProcessFilterFunc PreProcessFilter;

        protected delegate Tuple<Vector2[], Matrix4x4?> PostProcessFilterFunc(Vector2[] local, Vector2[] offset, Matrix4x4? matrix);
        PostProcessFilterFunc PostProcessFilter;

        void PreProcess() {

            // Skip if already pre-processed
            if (pmPreProcessed) return;
            pmPreProcessed = true;

            if (PreProcessFilter != null) {
                OverrideTransformMatrix = null;
                Matrix4x4 matrix = this.mParent != null ? this.mParent.Transform.Matrix : Matrix4x4.identity;
                var filterResult = PreProcessFilter(new[] { (Vector2)LocalTransform.Translation }, new[] { (Vector2)pmOffsetTransform.Translation }, matrix);
                if (filterResult.Item1 != null && filterResult.Item1.Length > 0) {
                    pmOffsetTransform.Translation = new Vector3(filterResult.Item1[0].x, filterResult.Item1[0].y, pmOffsetTransform.Translation.z);
                    TransformChanged();
                }
            }
        }

        void PostProcess() {

            // Skip if already pre-processed
            if (pmPostProcessed) return;
            pmPostProcessed = true;

            if (PostProcessFilter != null) {
                OverrideTransformMatrix = null;
                Matrix4x4 matrix = this.mParent != null ? this.mParent.Transform.Matrix : Matrix4x4.identity;
                var filterResult = PostProcessFilter(new[] { (Vector2)LocalTransform.Translation }, new[] { (Vector2)pmOffsetTransform.Translation }, matrix);
                if (filterResult.Item1 != null && filterResult.Item1.Length > 0) {
                    pmOffsetTransform.Translation = new Vector3(filterResult.Item1[0].x, filterResult.Item1[0].y, pmOffsetTransform.Translation.z);
                    TransformChanged();
                    OverrideTransformMatrix = Transform.Matrix;
                }
            }

        }
        #endregion

        #region Public Members

        /// <summary>
        /// Whether this node is enabled
        /// </summary>
        public bool Enabled;

        /// <summary>
        /// Name of this node
        /// </summary>
        public string Name;

        /// <summary>
        /// UUID of this node
        /// </summary>
        public uint UUID { get { return mUUID; } }

        /// <summary>
        /// Relative Z sorting offset.
        /// </summary>
        public float RelativeZSort { get { return mZSort; } }

        /// <summary>
        /// Basis Z sorting offset.
        /// </summary>
        public float ZSortBase { get { return mParent is not null ? mParent.ZSort : 0; } }

        /// <summary>
        /// Z sorting offset without parameter offset.
        /// </summary>
        public float ZSortNoOffset { get { return ZSortBase + RelativeZSort; } }

        /// <summary>
        /// Z sorting offset.
        /// </summary>
        public float ZSort { get { return ZSortBase + RelativeZSort + pmOffsetSort; } set { mZSort = value; } }

        /// <summary>
        /// Lock translation to root.
        /// </summary>
        public bool LockToRoot {
            get { return mLockToRoot; }
            set {
                if (value && !mLockToRoot) {
                    LocalTransform.Translation = TransformNoLock.Translation;
                } else if (!value && mLockToRoot) {
                    LocalTransform.Translation = LocalTransform.Translation - mParent.TransformNoLock.Translation;
                }

                mLockToRoot = value;
            }
        }

        /// <summary>
        /// Constructs a new node
        /// </summary>
        /// <param name="puppet">Puppet that owns the node</param>
        public Node(InPuppet puppet) {
            this.mPuppet = puppet;
        }

        /// <summary>
        /// Constructs a new node
        /// </summary>
        /// <param name="uuid">UUID of the node</param>
        /// <param name="parent">Parent node of the node</param>
        public Node(uint uuid, Node parent) {
            this.mParent = parent;
            this.mUUID = uuid;
        }


        /// <summary>
        /// Whether the node is enabled for rendering
        /// 
        /// Disabled nodes will not be drawn
        /// 
        /// This happens recursively.
        /// </summary>
        public bool RenderEnabled {
            get {
                if (mParent != null) return !mParent.RenderEnabled ? false : Enabled;
                return Enabled;
            }
        }

        /// <summary>
        /// Local transform
        /// </summary>
        public Math.Transform LocalTransform;

        /// <summary>
        /// Cached global transform
        /// </summary>
        public Math.Transform GlobalTransform;

        /// <summary>
        /// Gets the global transform
        /// </summary>
        public Math.Transform Transform {
            get {
                if (mRecalculateTransform) {
                    LocalTransform.Update();
                    pmOffsetTransform.Update();

                    if (mLockToRoot) GlobalTransform = LocalTransform.CalculateOffset(pmOffsetTransform) * mPuppet.Root.LocalTransform;
                    else if (mParent != null) GlobalTransform = LocalTransform.CalculateOffset(pmOffsetTransform) * mParent.Transform;
                    else GlobalTransform = LocalTransform.CalculateOffset(pmOffsetTransform);

                    mRecalculateTransform = false;
                }
                return GlobalTransform;
            }
        }

        /// <summary>
        /// Gets the global transform (without parameter influence)
        /// </summary>
        public Math.Transform TransformNoParam {
            get {
                if (mRecalculateTransform) {
                    LocalTransform.Update();
                    pmOffsetTransform.Update();

                    if (mLockToRoot) GlobalTransform = LocalTransform * mPuppet.Root.LocalTransform;
                    else if (mParent != null) GlobalTransform = LocalTransform * mParent.Transform;
                    else GlobalTransform = LocalTransform;

                    mRecalculateTransform = false;
                }
                return GlobalTransform;
            }
        }

        /// <summary>
        /// The transform in world space without locking
        /// </summary>
        public Math.Transform TransformLocal {
            get {
                LocalTransform.Update();
                return LocalTransform.CalculateOffset(pmOffsetTransform);
            }
        }

        /// <summary>
        /// The transform in world space without locking
        /// </summary>
        public Math.Transform TransformNoLock {
            get {
                LocalTransform.Update();

                if (mParent != null) return LocalTransform * mParent.Transform;
                return LocalTransform;
            }
        }

        /// <summary>
        /// Dynamic matrix for Mesh Groups
        /// </summary>
        public Matrix4x4 DynamicMatrix {
            get {
                if (OverrideTransformMatrix != null) return OverrideTransformMatrix.Value;
                return Transform.Matrix;
            }
        }

        /// <summary>
        /// Marks this node's transform (and its decendents') as dirty.
        /// </summary>
        public void TransformChanged() {
            mRecalculateTransform = true;

            foreach (var child in mChildren) {
                child.TransformChanged();
            }
        }
        #endregion

        #region Virtual Functions

        /// <summary>
        /// Gets whether the node supports a parameter
        /// </summary>
        /// <param name="key">Key</param>
        /// <returns>True if supported, false otherwise</returns>
        public virtual bool HasParam(string key) {
            switch(key) {
                case "zSort":
                case "transform.t.x":
                case "transform.t.y":
                case "transform.t.z":
                case "transform.r.x":
                case "transform.r.y":
                case "transform.r.z":
                case "transform.s.x":
                case "transform.s.y":
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Gets the default parameter values
        /// </summary>
        /// <param name="key">Key of the value</param>
        /// <returns>The default value</returns>
        public virtual float GetDefaultValue(string key) {
            switch (key) {
                case "zSort":
                case "transform.t.x":
                case "transform.t.y":
                case "transform.t.z":
                case "transform.r.x":
                case "transform.r.y":
                case "transform.r.z":
                    return 0;
                case "transform.s.x":
                case "transform.s.y":
                    return 1;
                default:
                    return float.NaN;
            }
        }

        /// <summary>
        /// Sets the parameter value for the specified key
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="value">Value</param>
        /// <returns>Whether it was successful</returns>
        public virtual bool SetValue(string key, float value) {
            switch (key) {
                case "zSort":
                    pmOffsetSort += value;
                    return true;

                case "transform.t.x":
                    pmOffsetTransform.Translation.x += value;
                    this.TransformChanged();
                    return true;

                case "transform.t.y":
                    pmOffsetTransform.Translation.y += value;
                    this.TransformChanged();
                    return true;

                case "transform.t.z":
                    pmOffsetTransform.Translation.z += value;
                    this.TransformChanged();
                    return true;

                case "transform.r.x":
                    pmOffsetTransform.Rotation.x += value;
                    this.TransformChanged();
                    return true;

                case "transform.r.y":
                    pmOffsetTransform.Rotation.y += value;
                    this.TransformChanged();
                    return true;

                case "transform.r.z":
                    pmOffsetTransform.Rotation.z += value;
                    this.TransformChanged();
                    return true;

                case "transform.s.x":
                    pmOffsetTransform.Rotation.x *= value;
                    this.TransformChanged();
                    return true;

                case "transform.s.y":
                    pmOffsetTransform.Rotation.y *= value;
                    this.TransformChanged();
                    return true;

                default: return false;
            }
        }

        /// <summary>
        /// Gets the parameter value for the key
        /// </summary>
        /// <param name="key">Key</param>
        /// <returns>Parameter value</returns>
        public virtual float GetValue(string key) {
            switch (key) {
                case "zSort": return pmOffsetSort;
                case "transform.t.x": return pmOffsetTransform.Translation.x;
                case "transform.t.y": return pmOffsetTransform.Translation.y;
                case "transform.t.z": return pmOffsetTransform.Translation.z;
                case "transform.r.x": return pmOffsetTransform.Rotation.x;
                case "transform.r.y": return pmOffsetTransform.Rotation.y;
                case "transform.r.z": return pmOffsetTransform.Rotation.z;
                case "transform.s.x": return pmOffsetTransform.Scale.x;
                case "transform.s.y": return pmOffsetTransform.Scale.y;
                default: return 0;
            }
        }

        /// <summary>
        /// Begins an update cycle for the node
        /// </summary>
        public virtual void BeginUpdate() {
            pmPreProcessed = false;
            pmPostProcessed = false;

            pmOffsetSort = 0;
            pmOffsetTransform.Clear();

            foreach(var child in mChildren) {
                child.BeginUpdate();
            }
        }

        /// <summary>
        /// Updates the node
        /// </summary>
        public virtual void Update() {
            PreProcess();

            if (!Enabled) return;

            foreach(var child in mChildren) {
                child.Update();
            }

            PostProcess();
        }

        /// <summary>
        /// Draws the node
        /// </summary>
        public virtual void Draw(Scene scene) {

        }

        /// <summary>
        /// Draws the node itself without iterating children
        /// </summary>
        public virtual void DrawOne(Scene scene) { }

        public virtual void Deserialize(JToken obj) {
            if (obj["uuid"] != null) mUUID = (uint)obj["uuid"];
            if (obj["name"] != null) Name = (string)obj["name"];
            if (obj["enabled"] != null) Enabled = (bool)obj["enabled"];
            if (obj["zsort"] != null) mZSort = (float)obj["zsort"];
            if (obj["lockToRoot"] != null) mLockToRoot = (bool)obj["lockToRoot"];

            if (obj["transform"] != null) {
                LocalTransform = new Math.Transform();
                LocalTransform.Deserialize(obj["transform"]);
            }

            // Iterate over children if there
            if (obj["children"] != null) {
                foreach (var child in obj["children"].Children()) {
                    string type = (string)child["type"];

                    if (!NodeFactoryRegistry.HasNodeType(type)) {
                        Node n = new Node(0, this);
                        n.Deserialize(child);
                        this.mChildren.Add(n);
                    } else {
                        Node n = NodeFactoryRegistry.CreateNode<Node>(type, this);
                        n.Deserialize(child);
                        this.mChildren.Add(n);
                    }
                }
            }
        }

        public virtual void FinalizeSerialization() {
            foreach(var child in mChildren) {
                child.FinalizeSerialization();
            }
        }

        public virtual void Reconstruct() {
            foreach (var child in mChildren) {
                child.Reconstruct();
            }
        }

        public override string ToString() {
            return Name;
        }
        #endregion
    }

    public static class NodeFactoryRegistry {
        public delegate Node CreateNodeDelegate(Node parent);
        private static Dictionary<string, CreateNodeDelegate> mNodeFactories = new Dictionary<string, CreateNodeDelegate>();

        /// <summary>
        /// Creates a node with the specified Type ID
        /// </summary>
        /// <param name="typeId">Type ID of the node to create</param>
        /// <param name="puppet">The puppet that owns the node</param>
        /// <returns>The node</returns>
        public static T CreateNode<T>(string typeId, Node parent) where T : Node {
            Node node = mNodeFactories[typeId].Invoke(parent);

            // Try to return our type T, if the types are incompatible throw an error
            // TODO: Throw custom exception
            if (node is T target) return target;
            else throw new Exception($"Tried to instantiate {typeId} as {typeof(T).Name}, but they were incompatible!");
        }

        /// <summary>
        /// Registers a new type in to the node factory registry
        /// </summary>
        /// <param name="typeId">Type Id</param>
        /// <param name="del">Factory delegate</param>
        public static void Register(string typeId, CreateNodeDelegate del) {
            mNodeFactories.Add(typeId, del);
        }

        /// <summary>
        /// Gets whether the factory registry can construct the type
        /// </summary>
        /// <param name="typeId">Type ID</param>
        /// <returns>Whether the factory registry can construct the type</returns>
        public static bool HasNodeType(string typeId) {
            return mNodeFactories.ContainsKey(typeId);
        }
    }
}