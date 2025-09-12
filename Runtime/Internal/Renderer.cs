using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Experimental.Rendering;
using System.Linq;
using static UnityEngine.GraphicsBuffer;
using Unity.Collections;
using System;
using UnityEngine.UIElements;

namespace Inochi2D.Internal {

    /// <summary>
    /// A renderer instance that keeps track of the rendering state 
    /// of a parent puppet and uses it to construct buffers to send
    /// to Unity.
    /// </summary>
    [Serializable]
    public class PuppetRenderer : IDisposable {
        private Camera targetCamera;
        private Material[] _mainMats;
        private Material[] _maskedMats;
        private Material _maskMat;
        private Material _blitMat;

        /// <summary>
        /// The camera to render with
        /// </summary>
        [SerializeField]
        public Camera Camera;

        /// <summary>
        /// Viewport albedo texture.
        /// </summary>
        public Texture ViewportAlbedo => renderTargets?[0].Textures?[0];

        #region Low Level Data
        // Inochi2D stores vertex data in the format 
        // struct VtxData {
        //    vec2 vtx;
        //    vec2 uvs;
        // }
        private VertexAttributeDescriptor[] vertexAttributes = new[] {
                new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float32, 2),
                new VertexAttributeDescriptor(VertexAttribute.TexCoord0, VertexAttributeFormat.Float32, 2),
        };
        private CommandBuffer cmdbuf;
        private PuppetRenderTarget maskTarget;
        private List<PuppetRenderTarget> renderTargets;
        private List<Texture2D> inputTextures;
        #endregion

        private InPuppet puppet;
        private Mesh mesh;

        public void Dispose() {
            targetCamera?.RemoveCommandBuffer(CameraEvent.AfterEverything, cmdbuf);

            maskTarget = null;
            renderTargets.Clear();
            inputTextures.Clear();
            cmdbuf.Dispose();
        }

        public void Awake() {
            this.targetCamera = Camera == null ? Camera.main : Camera;
        }

        public void Setup(InPuppet puppet) {
            // Setup puppet
            this.puppet = puppet;
            this.inputTextures = new List<Texture2D>();

            // NOTE: Unity does not seem to properly support clamp to border color
            //       As such we pad the texture with a large transparent area instead.
            for (int i = 0; i < puppet.Textures.Length; i++) {
                InTexture tex = puppet.Textures[i];
                tex.ID = i;

                this.inputTextures.Add(tex.ToTexture2D());
            }

            // Setup materials.
            Shader mainShader = Shader.Find("Inochi2D/I2DMainShader");
            Shader maskedShader = Shader.Find("Inochi2D/I2DMaskedShader");
            Shader maskShader = Shader.Find("Inochi2D/I2DMaskShader");
            Shader blitShader = Shader.Find("Inochi2D/I2DBlitShader");
            _mainMats = new Material[(int)InBlendMode.COUNT];
            _maskedMats = new Material[(int)InBlendMode.COUNT];
            _maskMat = new Material(maskShader);
            _blitMat = new Material(blitShader);
            for (int i = 0; i < (int)InBlendMode.COUNT; i++) {
                _mainMats[i] = new Material(mainShader);
                this.setBlendingMode(_mainMats[i], (InBlendMode)i);

                _maskedMats[i] = new Material(maskedShader);
                this.setBlendingMode(_maskedMats[i], (InBlendMode)i);
            }

            // Setup command buffer
            this.cmdbuf = new CommandBuffer();
            this.cmdbuf.name = "Inochi2D Renderer";
            targetCamera.AddCommandBuffer(CameraEvent.AfterEverything, cmdbuf);

            // Setup Render Targets.
            this.renderTargets = new List<PuppetRenderTarget>();
            this.maskTarget = new PuppetRenderTarget(targetCamera.pixelWidth, targetCamera.pixelHeight)
                .Add(RenderTextureFormat.R8, 1)
                .Build();

            this.mesh = new Mesh();
            mesh.name = "Inochi2D Reused Mesh Buffer";
        }

        public unsafe void Draw() {
            if (puppet.IsNull)
                return;

            // Update the camera if the user sets a new one.
            if (Camera != null && targetCamera != Camera) {
                targetCamera.RemoveCommandBuffer(CameraEvent.AfterEverything, cmdbuf);
                targetCamera = Camera;
                targetCamera.AddCommandBuffer(CameraEvent.AfterEverything, cmdbuf);
            }

            puppet.Update(Time.deltaTime);
            var preRenderTexture = RenderTexture.active;

            // Setup Render Targets
            foreach (var target in renderTargets) {
                target.Resize(targetCamera.pixelWidth, targetCamera.pixelHeight);
            }

            var cmds = puppet.DrawList.Commands;
            var allocs = puppet.DrawList.Allocations;

            // Setup Meshes
            var vtxData = puppet.DrawList.VertexData;
            var idxData = puppet.DrawList.IndexData;
            mesh.SetIndexBufferParams(idxData.Length, IndexFormat.UInt32);
            mesh.SetVertexBufferParams(vtxData.Length, vertexAttributes);
            mesh.SetIndexBufferData<uint>(idxData, 0, 0, idxData.Length);
            mesh.SetVertexBufferData<VtxData>(vtxData, 0, 0, vtxData.Length);
            mesh.subMeshCount = allocs.Length;
            for (int i = 0; i < allocs.Length; i++) {
                mesh.SetSubMesh(i, new SubMeshDescriptor {
                    topology = MeshTopology.Triangles,
                    indexStart = (int)allocs[i].IdxOffset,
                    indexCount = (int)allocs[i].IdxCount,
                    baseVertex = (int)allocs[i].VtxOffset,
                    vertexCount = (int)allocs[i].VtxCount,
                }, MeshUpdateFlags.DontRecalculateBounds);
            }

            // Free the vertex and index data from our side.
            vtxData.Dispose();
            idxData.Dispose();

            // Start recording
            cmdbuf.Clear();
            cmdbuf.SetViewProjectionMatrices(targetCamera.worldToCameraMatrix, targetCamera.projectionMatrix);
            cmdbuf.SetViewport(targetCamera.pixelRect);

            uint substate = 0;
            uint compositeDepth = 0;
            for (int i = 0; i < cmds.Length; i++) {
                var cmd = cmds[i];
                switch (cmd.State) {
                    case DrawState.Normal:
                        if (substate != 0) {
                            cmdbuf.SetRenderTarget(preRenderTexture);
                            cmdbuf.SetViewProjectionMatrices(targetCamera.worldToCameraMatrix, targetCamera.projectionMatrix);
                            substate = 0;
                        }

                        if (!cmd.Sources[0].IsNull)
                            cmdbuf.SetGlobalTexture("_AlbedoTex", inputTextures[cmd.Sources[0].ID]);

                        if (!cmd.Sources[1].IsNull)
                            cmdbuf.SetGlobalTexture("_EmissionTex", inputTextures[cmd.Sources[1].ID]);

                        if (!cmd.Sources[2].IsNull)
                            cmdbuf.SetGlobalTexture("_BumpmapTex", inputTextures[cmd.Sources[2].ID]);

                        cmdbuf.DrawMesh(
                            mesh,
                            Matrix4x4.identity,
                            _mainMats[(int)cmd.BlendMode],
                            (int)cmd.AllocId,
                            0
                        );
                        break;

                    case DrawState.DefineMask:
                        if (substate != 1) {
                            substate = 1;
                            cmdbuf.SetRenderTarget(maskTarget.Target);
                            cmdbuf.ClearRenderTarget(false, true, new Color(0, 0, 0, 0));
                            cmdbuf.SetViewProjectionMatrices(targetCamera.worldToCameraMatrix, targetCamera.projectionMatrix);
                        }

                        if (!cmd.Sources[0].IsNull) {
                            cmdbuf.SetGlobalTexture("_MaskTex", inputTextures[cmd.Sources[0].ID]);
                        }

                        cmdbuf.DrawMesh(
                            mesh,
                            Matrix4x4.identity,
                            _maskMat,
                            (int)cmd.AllocId,
                            0
                        );
                        break;

                    case DrawState.MaskedDraw:
                        if (substate != 2) {
                            cmdbuf.SetRenderTarget(preRenderTexture);
                            cmdbuf.SetViewProjectionMatrices(targetCamera.worldToCameraMatrix, targetCamera.projectionMatrix);
                            substate = 2;
                        }
                        if (!cmd.Sources[0].IsNull)
                            cmdbuf.SetGlobalTexture("_AlbedoTex", inputTextures[cmd.Sources[0].ID]);

                        if (!cmd.Sources[1].IsNull)
                            cmdbuf.SetGlobalTexture("_EmissionTex", inputTextures[cmd.Sources[1].ID]);

                        if (!cmd.Sources[2].IsNull)
                            cmdbuf.SetGlobalTexture("_BumpmapTex", inputTextures[cmd.Sources[2].ID]);

                        cmdbuf.SetGlobalTexture("_MaskTex", maskTarget.Textures[0]);
                        cmdbuf.DrawMesh(
                            mesh,
                            Matrix4x4.identity,
                            _maskedMats[(int)cmd.BlendMode],
                            (int)cmd.AllocId,
                            0
                        );
                        break;

                    case DrawState.CompositeBegin:
                        compositeDepth++;
                        if (compositeDepth >= renderTargets.Count) {
                            this.renderTargets.Add(
                                new PuppetRenderTarget(targetCamera.pixelWidth, targetCamera.pixelHeight)
                                    .Add(RenderTextureFormat.ARGB32, 8)
                                    .Build()
                            );
                        }
                        cmdbuf.SetRenderTarget(renderTargets[(int)compositeDepth - 1].Target);
                        cmdbuf.ClearRenderTarget(RTClearFlags.All, new Color(0, 0, 0, 0));
                        cmdbuf.SetViewProjectionMatrices(targetCamera.worldToCameraMatrix, targetCamera.projectionMatrix);
                        break;

                    case DrawState.CompositeEnd:
                        compositeDepth--;
                        if (compositeDepth > 0)
                            cmdbuf.SetRenderTarget(renderTargets[(int)compositeDepth - 1].Target);
                        else
                            cmdbuf.SetRenderTarget(preRenderTexture);

                        break;

                    case DrawState.CompositeBlit:
                        var upperTarget = renderTargets[(int)compositeDepth];
                        this.setBlendingMode(_blitMat, cmd.BlendMode);
                        if (compositeDepth > 0) {
                            for (int j = 0; j < upperTarget.Target.colorRenderTargets.Length; j++) {
                                cmdbuf.SetGlobalTexture("_MaskTex", maskTarget.Textures[0]);
                                cmdbuf.Blit(
                                    upperTarget.Target.colorRenderTargets[j],
                                    renderTargets[(int)compositeDepth - 1].Target.colorRenderTargets[j],
                                    _blitMat
                                );
                            }
                        } else {
                            cmdbuf.SetGlobalTexture("_MaskTex", maskTarget.Textures[0]);
                            cmdbuf.Blit(
                                upperTarget.Target.colorRenderTargets[0],
                                new RenderTargetIdentifier(preRenderTexture),
                                _blitMat
                            );
                        }
                        break;
                }
            }
        }

        private void setBlendingMode(Material mat, InBlendMode mode) {
            switch (mode) {

                // If the advanced blending extension is not supported, force to Normal blending
                default:
                    mat.SetInteger("_BlendSrc", (int)BlendMode.One);
                    mat.SetInteger("_BlendDst", (int)BlendMode.OneMinusSrcAlpha);
                    break;

                case InBlendMode.Normal:
                    mat.SetInteger("_BlendSrc", (int)BlendMode.One);
                    mat.SetInteger("_BlendDst", (int)BlendMode.OneMinusSrcAlpha);
                    break;

                case InBlendMode.Multiply:
                    mat.SetInteger("_BlendSrc", (int)BlendMode.DstColor);
                    mat.SetInteger("_BlendDst", (int)BlendMode.OneMinusSrcAlpha);
                    break;

                case InBlendMode.Screen:
                    mat.SetInteger("_BlendSrc", (int)BlendMode.One);
                    mat.SetInteger("_BlendDst", (int)BlendMode.OneMinusSrcColor);
                    break;

                case InBlendMode.Lighten:
                    mat.SetInteger("_BlendSrc", (int)BlendMode.One);
                    mat.SetInteger("_BlendDst", (int)BlendMode.One);
                    break;

                case InBlendMode.ColorDodge:
                    mat.SetInteger("_BlendSrc", (int)BlendMode.DstColor);
                    mat.SetInteger("_BlendDst", (int)BlendMode.One);
                    break;

                //case InBlendMode.LinearDodge:
                //    mat.SetInteger("_SrcBlendRGB", (int)BlendMode.One);
                //    mat.SetInteger("_DstBlendRGB", (int)BlendMode.OneMinusSrcColor);
                //    mat.SetInteger("_SrcBlendAlpha", (int)BlendMode.One);
                //    mat.SetInteger("_DstBlendAlpha", (int)BlendMode.OneMinusSrcAlpha);
                //    break;

                //case InBlendMode.AddGlow:
                //    mat.SetInteger("_SrcBlendRGB", (int)BlendMode.One);
                //    mat.SetInteger("_DstBlendRGB", (int)BlendMode.One);
                //    mat.SetInteger("_SrcBlendAlpha", (int)BlendMode.One);
                //    mat.SetInteger("_DstBlendAlpha", (int)BlendMode.OneMinusSrcAlpha);
                //    break;

                //case InBlendMode.Exclusion:
                //    mat.SetInteger("_SrcBlendAlpha", (int)BlendMode.One);
                //    mat.SetInteger("_SrcBlendRGB", (int)BlendMode.OneMinusDstColor);
                //    mat.SetInteger("_DstBlendAlpha", (int)BlendMode.One);
                //    mat.SetInteger("_DstBlendRGB", (int)BlendMode.OneMinusSrcColor);
                //    break;

                case InBlendMode.Inverse:
                    mat.SetInteger("_BlendSrc", (int)BlendMode.OneMinusDstColor);
                    mat.SetInteger("_BlendDst", (int)BlendMode.OneMinusSrcAlpha);
                    break;

                case InBlendMode.DestinationIn:
                    mat.SetInteger("_BlendSrc", (int)BlendMode.Zero);
                    mat.SetInteger("_BlendDst", (int)BlendMode.SrcAlpha);
                    break;

                case InBlendMode.SourceIn:
                    mat.SetInteger("_BlendSrc", (int)BlendMode.DstAlpha);
                    mat.SetInteger("_BlendDst", (int)BlendMode.OneMinusSrcAlpha);
                    break;

                case InBlendMode.SourceOut:
                    mat.SetInteger("_BlendSrc", (int)BlendMode.Zero);
                    mat.SetInteger("_BlendDst", (int)BlendMode.OneMinusSrcAlpha);
                    break;
            }
        }
    }

    public class PuppetRenderTarget {
        private int width;
        private int height;
        private RenderTargetBinding target;
        private RenderTextureDescriptor desc;
        private List<RenderTexture> textures;

        /// <summary>
        /// The textures in this render target.
        /// </summary>
        public List<RenderTexture> Textures { get { return textures; } }

        /// <summary>
        /// The render target binding.
        /// </summary>
        public RenderTargetBinding Target { get { return target; } }

        public PuppetRenderTarget(int width, int height) {
            this.textures = new List<RenderTexture>();
            this.width = width;
            this.height = height;
            this.Resize(width, height);
        }

        public void Resize(int width, int height) {
            if (this.width == width && this.height == height)
                return;

            this.width = width;
            this.height = height;

            foreach (var texture in textures) {
                texture.Release();
                texture.width = width;
                texture.height = height;
            }
        }

        public PuppetRenderTarget Add(RenderTextureFormat format, int count = 1) {
            desc = new RenderTextureDescriptor(width, height);
            desc.mipCount = 1;
            desc.msaaSamples = 1;
            desc.sRGB = format == RenderTextureFormat.ARGB32;
            desc.colorFormat = format;
            desc.dimension = TextureDimension.Tex2D;
            desc.depthBufferBits = 0;
            desc.useDynamicScale = false;
            for (int i = 0; i < count; i++) {
                textures.Add(new RenderTexture(desc));
            }
            return this;
        }

        public PuppetRenderTarget Build() {

            // Setup render textures.
            var colorLoadActions = new RenderBufferLoadAction[textures.Count];
            var colorStoreActions = new RenderBufferStoreAction[textures.Count];
            var renderTargetIdentifiers = new RenderTargetIdentifier[textures.Count];
            for (int i = 0; i < textures.Count; i++) {
                colorLoadActions[i] = RenderBufferLoadAction.Load;
                colorStoreActions[i] = RenderBufferStoreAction.Store;
                renderTargetIdentifiers[i] = new RenderTargetIdentifier(textures[i]);
            }

            // Finalize target.
            target = new RenderTargetBinding {
                colorRenderTargets = renderTargetIdentifiers,
                colorLoadActions = colorLoadActions,
                colorStoreActions = colorStoreActions,
                depthRenderTarget = renderTargetIdentifiers[0]
            };

            return this;
        }
    }
}
