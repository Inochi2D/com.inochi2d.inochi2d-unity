using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Experimental.Rendering;

namespace Inochi2D {
    public class Scene : MonoBehaviour {
        
        [SerializeField]
        [InspectorName("Width")]
        private int m_Width = 1920;

        [SerializeField]
        [InspectorName("Height")]
        private int m_Height = 1080;

        /// <summary>
        /// Command Buffer
        /// </summary>
        public CommandBuffer CommandBuffer;

        /// <summary>
        /// Camera
        /// </summary>
        public Camera Camera;

        public int Width { 
            get { return m_Width; } 
            set { m_Width = value; this.ResizeBuffers(); }
        }

        public int Height {
            get { return m_Height; }
            set { m_Height = value; this.ResizeBuffers(); }
        }

        private Material mMainMaterial;
        RenderTargetBinding mMainBinding;
        private RenderTexture mMainAlbedo;
        private RenderTexture mMainEmission;
        private RenderTexture mMainBumpmap;

        private Material mCompositeMaterial;
        RenderTargetBinding mCompositeBinding;
        private RenderTexture mCompositeAlbedo;
        private RenderTexture mCompositeEmission;
        private RenderTexture mCompositeBumpmap;

        public Texture TargetAlbedo;
        public Texture TargetEmission;
        public Texture TargetBumpmap;

        public void Begin() {
            CommandBuffer.Clear();
            CommandBuffer.SetRenderTarget(mMainBinding);
            CommandBuffer.SetViewport(new Rect(0, 0, (float)m_Width, (float)m_Height));
            CommandBuffer.ClearRenderTarget(true, true, new Color(1, 0, 0, 1));
            CommandBuffer.BeginSample("Puppet Begin");
        }

        public void BeginComposite() {
            CommandBuffer.BeginSample("Compositing");
            CommandBuffer.SetRenderTarget(mCompositeBinding);
            CommandBuffer.SetViewport(new Rect(0, 0, (float)m_Width, (float)m_Height));
            CommandBuffer.ClearRenderTarget(true, true, new Color(0, 0, 0, 0));
        }

        public void EndComposite() {

            // Make a mesh that'll fill the entire screen
            Mesh screenMesh = new Mesh();
            screenMesh.vertices = new[] {
                new Vector3(0, 0, 0),  // Bottom Left
                new Vector3(1, 0, 0),  // Bottom Right
                new Vector3(0, 1, 0),  // Top Left
                new Vector3(1, 1, 0),  // Top Right
            };
            screenMesh.triangles = new[] {
                0, 1, 2,
                1, 3, 2
            };

            CommandBuffer.SetRenderTarget(mMainBinding);
            CommandBuffer.DrawMesh(screenMesh, Matrix4x4.identity, mCompositeMaterial);
            CommandBuffer.EndSample("Compositing");
        }

        public void End() {
            CommandBuffer.EndSample("Puppet Begin");

            // Blit to target
            if (TargetAlbedo != null) CommandBuffer.Blit(mMainAlbedo, new RenderTargetIdentifier(TargetAlbedo));
            if (TargetEmission != null) CommandBuffer.Blit(mMainEmission, new RenderTargetIdentifier(TargetEmission));
            if (TargetBumpmap != null) CommandBuffer.Blit(mMainBumpmap, new RenderTargetIdentifier(TargetBumpmap));
        }

        // Use this for initialization
        void Start() {
            
            // Create main buffer
            mMainAlbedo = new RenderTexture(Width, Height, 32, DefaultFormat.HDR);
            mMainAlbedo.name = "Main Buffer (Albedo)";
            mMainAlbedo.Create();
            mMainEmission = new RenderTexture(Width, Height, 0, DefaultFormat.LDR);
            mMainEmission.name = "Main Buffer (Emission)";
            mMainEmission.Create();
            mMainBumpmap = new RenderTexture(Width, Height, 0, DefaultFormat.LDR);
            mMainBumpmap.name = "Main Buffer (Bumpmap)";
            mMainBumpmap.Create();
            mMainBinding = new RenderTargetBinding(new RenderTargetSetup(new[] { mMainAlbedo.colorBuffer, mMainEmission.colorBuffer, mMainBumpmap.colorBuffer }, mMainAlbedo.depthBuffer));

            mMainMaterial = new Material(Shader.Find("Standard"));
            mMainMaterial.SetTexture("_MainTex", mMainAlbedo);
            mMainMaterial.SetTexture("_EmissionMap", mMainEmission);
            mMainMaterial.SetTexture("_BumpMap", mMainBumpmap);

            // Create composite buffer
            mCompositeAlbedo = new RenderTexture(Width, Height, 32, DefaultFormat.HDR);
            mCompositeAlbedo.name = "Composite Buffer (Albedo)";
            mCompositeAlbedo.Create();
            mCompositeEmission = new RenderTexture(Width, Height, 0, DefaultFormat.LDR);
            mCompositeEmission.name = "Composite Buffer (Emission)";
            mCompositeEmission.Create();
            mCompositeBumpmap = new RenderTexture(Width, Height, 0, DefaultFormat.LDR);
            mCompositeBumpmap.name = "Composite Buffer (Bumpmap)";
            mCompositeEmission.Create();
            mCompositeBinding = new RenderTargetBinding(new RenderTargetSetup(new[] { mMainAlbedo.colorBuffer, mMainEmission.colorBuffer, mMainBumpmap.colorBuffer }, mMainAlbedo.depthBuffer));

            mCompositeMaterial = new Material(Shader.Find("Standard"));
            mCompositeMaterial.SetTexture("_MainTex", mCompositeAlbedo);
            mCompositeMaterial.SetTexture("_EmissionMap", mCompositeEmission);
            mCompositeMaterial.SetTexture("_BumpMap", mCompositeBumpmap);

            CommandBuffer = new CommandBuffer();
            CommandBuffer.name = "Inochi2D Command Buffer";
            Camera.AddCommandBuffer(CameraEvent.BeforeForwardOpaque, CommandBuffer);
        }

        public void ResizeBuffers() {
            mMainAlbedo.width = m_Width;
            mMainAlbedo.height = m_Height;
            mMainEmission.width = m_Width;
            mMainEmission.height = m_Height;
            mMainBumpmap.width = m_Width;
            mMainBumpmap.height = m_Height;

            mCompositeAlbedo.width = m_Width;
            mCompositeAlbedo.height = m_Height;
            mCompositeEmission.width = m_Width;
            mCompositeEmission.height = m_Height;
            mCompositeBumpmap.width = m_Width;
            mCompositeBumpmap.height = m_Height;
        }

        // Update is called once per frame
        void Update() {

        }

        private void OnDrawGizmos() {
            Gizmos.DrawWireCube(Camera.transform.position, new Vector3(m_Width, m_Height, 0));
        }
    }
}