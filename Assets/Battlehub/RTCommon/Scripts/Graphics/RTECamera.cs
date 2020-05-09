using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Battlehub.RTCommon
{
    public interface IRTECamera
    {
        event Action<IRTECamera> CommandBufferRefresh;

        Camera Camera
        {
            get;
        }

        CommandBuffer CommandBuffer
        {
            get;
        }

        CameraEvent Event
        {
            get;
            set;
        }

        IRenderersCache RenderersCache
        {
            get;
        }

        IMeshesCache MeshesCache
        {
            get;
        }

        void RefreshCommandBuffer();
        void Destroy();
    }

    public class RTECamera : MonoBehaviour, IRTECamera
    {
        public event Action<IRTECamera> CommandBufferRefresh;

        private Camera m_camera;
        public Camera Camera
        {
            get { return m_camera; }
        }
        
        private CommandBuffer m_commandBuffer;
        public CommandBuffer CommandBuffer
        {
            get { return m_commandBuffer; }
        }

        [SerializeField]
        private CameraEvent m_cameraEvent = CameraEvent.BeforeImageEffects;
        public CameraEvent Event
        {
            get { return m_cameraEvent; }
            set
            {
                m_cameraEvent = value;
                RemoveCommandBuffer();
                CreateCommandBuffer();
            }
        }

        private IRenderersCache m_renderersCache;
        private IMeshesCache m_meshesCache;

        public IRenderersCache RenderersCache
        {
            get { return m_renderersCache; }
            set { m_renderersCache = value; }
        }

        public IMeshesCache MeshesCache
        {
            get { return m_meshesCache; }
            set { m_meshesCache = value; }
        }

        public void Destroy()
        {
            Destroy(this);
        }

        private void Awake()
        {
            m_camera = GetComponent<Camera>();
        }

        private void Start()
        {
            CreateCommandBuffer();

            if(m_renderersCache == null)
            {
                m_renderersCache = gameObject.GetComponent<IRenderersCache>();
            }

            if(m_meshesCache == null)
            {
                m_meshesCache = gameObject.GetComponent<IMeshesCache>();
            }
            
            RefreshCommandBuffer();

            if(m_renderersCache != null)
            {
                m_renderersCache.Refreshed += OnRefresh;
            }

            if(m_meshesCache != null)
            {
                m_meshesCache.Refreshing += OnRefresh;
            }
        }

        private void OnDestroy()
        {
            if (m_renderersCache != null)
            {
                m_renderersCache.Refreshed -= OnRefresh; 
            }

            if (m_meshesCache != null)
            {
                m_meshesCache.Refreshing -= OnRefresh;
            }

            if (m_camera != null)
            {
                RemoveCommandBuffer();
            }
        }

        private void OnRefresh()
        {
            RefreshCommandBuffer();
        }

        private void CreateCommandBuffer()
        {
            if (m_commandBuffer != null || m_camera == null)
            {
                return;
            }
            m_commandBuffer = new CommandBuffer();
            m_commandBuffer.name = "RTECameraCommandBuffer";
            m_camera.AddCommandBuffer(m_cameraEvent, m_commandBuffer);
        }

        private void RemoveCommandBuffer()
        {
            if (m_commandBuffer == null)
            {
                return;
            }
            m_camera.RemoveCommandBuffer(m_cameraEvent, m_commandBuffer);
            m_commandBuffer = null;
        }

        public void RefreshCommandBuffer()
        {
            if(m_commandBuffer == null)
            {
                return;
            }

            m_commandBuffer.Clear();
            if(m_cameraEvent == CameraEvent.AfterImageEffects || m_cameraEvent == CameraEvent.AfterImageEffectsOpaque)
            {
                m_commandBuffer.ClearRenderTarget(true, false, Color.black);
            }
   
            if(m_meshesCache != null)
            {
                IList<RenderMeshesBatch> batches = m_meshesCache.Batches;
                for (int i = 0; i < batches.Count; ++i)
                {
                    RenderMeshesBatch batch = batches[i];
                    if (batch.Material == null)
                    {
                        continue;
                    }

                    if (batch.Material.enableInstancing)
                    {
                        for (int j = 0; j < batch.Mesh.subMeshCount; ++j)
                        {
                            if (batch.Mesh != null)
                            {
                                m_commandBuffer.DrawMeshInstanced(batch.Mesh, j, batch.Material, -1, batch.Matrices, batch.Matrices.Length);
                            }
                        }
                    }
                    else
                    {
                        Matrix4x4[] matrices = batch.Matrices;
                        for (int m = 0; m < matrices.Length; ++m)
                        {
                            for (int j = 0; j < batch.Mesh.subMeshCount; ++j)
                            {
                                if (batch.Mesh != null)
                                {
                                    m_commandBuffer.DrawMesh(batch.Mesh, matrices[m], batch.Material, j, -1);
                                }
                            }
                        }
                    }
                }
            }


            if (m_renderersCache != null)
            {
                IList<Renderer> renderers = m_renderersCache.Renderers;
                for (int i = 0; i < renderers.Count; ++i)
                {
                    Renderer renderer = renderers[i];
                    Material[] materials = renderer.sharedMaterials;
                    for (int j = 0; j < materials.Length; ++j)
                    {
                        Material material = materials[j];
                        m_commandBuffer.DrawRenderer(renderer, material, j, -1);
                    }
                }
            }

            if (CommandBufferRefresh != null)
            {
                CommandBufferRefresh(this);
            }
        }
    }
}
