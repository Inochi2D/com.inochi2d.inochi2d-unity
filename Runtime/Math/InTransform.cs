using Newtonsoft.Json.Linq;
using System.Collections;
using UnityEngine;

namespace Inochi2D.Math {
    public struct Transform {
        private Matrix4x4 mTRS;

        /// <summary>
        /// Translation (Position)
        /// </summary>
        [SerializeField]
        public Vector3 Translation;

        /// <summary>
        /// Rotation
        /// </summary>
        [SerializeField]
        public Vector3 Rotation;

        /// <summary>
        /// Scale
        /// </summary>
        [SerializeField]
        public Vector2 Scale;

        /// <summary>
        /// Whether the transform should be snapped to the nearest pixel
        /// </summary>
        [SerializeField]
        public bool PixelSnap;

        /// <summary>
        /// Gets the output matrix
        /// </summary>
        public Matrix4x4 Matrix { get { return mTRS; } }

        /// <summary>
        /// Constructs a new Transform
        /// </summary>
        /// <param name="translation">Translation (Position)</param>
        /// <param name="rotation">Rotation</param>
        /// <param name="scale">Scale</param>
        public Transform(Vector3 translation, Vector3 rotation, Vector2 scale) {
            this.mTRS = Matrix4x4.identity;
            this.PixelSnap = false;

            this.Translation = translation;
            this.Rotation = rotation;
            this.Scale = scale;
        }

        /// <summary>
        /// Calculates offset from transform
        /// </summary>
        /// <param name="other">transform to calculate in relation to</param>
        /// <returns>Transform</returns>
        public Transform CalculateOffset(Transform other) {
            Transform tnew = new Transform();

            tnew.Translation = this.Translation + other.Translation;
            tnew.Rotation = this.Rotation + other.Rotation;
            tnew.Scale = this.Scale + other.Scale;
            tnew.Update();

            return tnew;
        }

        /// <summary>
        /// Updates the transform
        /// </summary>
        public void Update() {
            mTRS =
                Matrix4x4.Translate(Translation) *
                Matrix4x4.Rotate(Quaternion.Euler(this.Rotation)) *
                Matrix4x4.Scale(new Vector3(Scale.x, Scale.y, 1));
        }

        /// <summary>
        /// Clears the transform
        /// </summary>
        public void Clear() {
            Translation = Vector3.zero;
            Rotation = Vector3.zero;
            Scale = Vector2.one;
            mTRS = Matrix4x4.identity;
        }

        /// <summary>
        /// Deserialize JObject in to the transform
        /// </summary>
        /// <param name="obj">Object to deserialize</param>
        public void Deserialize(JObject obj) {
            if (obj != null) {

                Translation = new Vector3((float)obj["trans"][0], (float)obj["trans"][1], ((float)obj["trans"][2]));
                Rotation = new Vector3((float)obj["rot"][0], (float)obj["rot"][1], ((float)obj["rot"][2]));
                Scale = new Vector2((float)obj["scale"][0], (float)obj["scale"][1]);
                
                this.Update();
            }
        }

        /// <summary>
        /// Allows multiplying 2 transforms together
        /// </summary>
        /// <param name="lhs">Left hand side</param>
        /// <param name="rhs">Right hand side</param>
        /// <returns>Transform</returns>
        public static Transform operator *(Transform lhs, Transform rhs) {
            Transform tnew = new Transform();

            Matrix4x4 strs = rhs.mTRS * lhs.mTRS;

            tnew.Translation = strs.GetPosition();
            tnew.Rotation = strs.rotation.eulerAngles;
            tnew.Scale = strs.lossyScale;
            tnew.mTRS = strs;
            return tnew;
        }
    }
}