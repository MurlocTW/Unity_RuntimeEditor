using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Battlehub.RTCommon
{
    public interface IRTEGraphics
    {
        IRTECamera CreateCamera(Camera camera);
        
        void RegisterCamera(Camera camera);
        void UnregisterCamera(Camera camera);

        IRenderersCache CreateRenderersCache(CameraEvent cameraEvent);
        void Destroy(IRenderersCache cache);

        IMeshesCache CreateMeshesCache(CameraEvent cameraEvent);
        void Destroy(IMeshesCache cache);
    }

    [DefaultExecutionOrder(-60)]
    public class RTEGraphics : MonoBehaviour, IRTEGraphics
    {
        private void Awake()
        {
            IOC.RegisterFallback<IRTEGraphics>(this);
        }

        private void OnDestroy()
        {
            IOC.UnregisterFallback<IRTEGraphics>(this);
        }

        public IRTECamera CreateCamera(Camera camera)
        {
            RenderersCache renderersCache = camera.gameObject.AddComponent<RenderersCache>();
            MeshesCache meshesCache = camera.gameObject.AddComponent<MeshesCache>();
            meshesCache.RefreshMode = CacheRefreshMode.Manual;

            RTECamera rteCamera = camera.gameObject.AddComponent<RTECamera>();
            rteCamera.RenderersCache = renderersCache;
            rteCamera.MeshesCache = meshesCache;

            if(RenderPipelineInfo.Type == RPType.Standard)
            {
                rteCamera.Event = CameraEvent.BeforeImageEffects;
            }
            else
            {
                rteCamera.Event = CameraEvent.AfterImageEffectsOpaque;
            }
            
            return rteCamera;
        }
    
        private class Data
        {
            public MonoBehaviour MonoBehaviour;
            public List<RTECamera> RTECameras;
            public CameraEvent Event;
            public Data(MonoBehaviour behaviour, CameraEvent cameraEvent, List<RTECamera> cameras)
            {
                MonoBehaviour = behaviour;
                Event = cameraEvent;
                RTECameras = cameras;
            }
        }

        private readonly List<GameObject> m_cameras = new List<GameObject>();
        private readonly Dictionary<IMeshesCache, Data> m_meshesCache = new Dictionary<IMeshesCache, Data>();
        private readonly Dictionary<IRenderersCache, Data> m_renderersCache = new Dictionary<IRenderersCache, Data>();

        public IMeshesCache CreateMeshesCache(CameraEvent cameraEvent)
        {
            MeshesCache cache = gameObject.AddComponent<MeshesCache>();
            cache.RefreshMode = CacheRefreshMode.Manual;

            List<RTECamera> rteCameras = new List<RTECamera>();
            foreach (GameObject camera in m_cameras)
            {
                CreateRTECamera(camera, cameraEvent, cache, rteCameras);
            }

            m_meshesCache.Add(cache, new Data(cache, cameraEvent, rteCameras));
            return cache;
        }

        public IRenderersCache CreateRenderersCache(CameraEvent cameraEvent)
        {
            RenderersCache cache = gameObject.AddComponent<RenderersCache>();
            List<RTECamera> rteCameras = new List<RTECamera>();
            foreach (GameObject camera in m_cameras)
            {
                CreateRTECamera(camera, cameraEvent, cache, rteCameras);
            }

            m_renderersCache.Add(cache, new Data(cache, cameraEvent, rteCameras));
            return cache;
        }

        public void Destroy(IRenderersCache cache)
        {
            Data tuple;
            if(m_renderersCache.TryGetValue(cache, out tuple))
            {
                Destroy(tuple.MonoBehaviour);
                for (int i = 0; i < tuple.RTECameras.Count; ++i)
                {
                    Destroy(tuple.RTECameras[i]);
                }
                m_renderersCache.Remove(cache);
            }
        }

        public void Destroy(IMeshesCache cache)
        {
            Data tuple;
            if (m_meshesCache.TryGetValue(cache, out tuple))
            {
                Destroy(tuple.MonoBehaviour);
                for (int i = 0; i < tuple.RTECameras.Count; ++i)
                {
                    Destroy(tuple.RTECameras[i]);
                }
                m_meshesCache.Remove(cache);
            }
        }

        public void RegisterCamera(Camera camera)
        {
            m_cameras.Add(camera.gameObject);

            foreach(KeyValuePair<IMeshesCache, Data> kvp in m_meshesCache)
            {
                IMeshesCache cache = kvp.Key;
                Data data = kvp.Value;
                CreateRTECamera(camera.gameObject, data.Event, cache, data.RTECameras);
            }

            foreach (KeyValuePair<IRenderersCache, Data> kvp in m_renderersCache)
            {
                IRenderersCache cache = kvp.Key;
                Data data = kvp.Value;
                CreateRTECamera(camera.gameObject, data.Event, cache, data.RTECameras);
            }
        }

        public void UnregisterCamera(Camera camera)
        {
            m_cameras.Remove(camera.gameObject);

            foreach (KeyValuePair<IMeshesCache, Data> kvp in m_meshesCache)
            {
                Data data = kvp.Value;

                DestroyRTECameras(camera, data);
            }

            foreach (KeyValuePair<IRenderersCache, Data> kvp in m_renderersCache)
            {
                Data data = kvp.Value;
                DestroyRTECameras(camera, data);
            }
        }

        private static void DestroyRTECameras(Camera camera, Data data)
        {
            List<RTECamera> rteCameras = data.RTECameras;
            for (int i = rteCameras.Count - 1; i >= 0; i--)
            {
                RTECamera rteCamera = rteCameras[i];
                if (rteCamera != null && rteCamera.gameObject == camera.gameObject)
                {
                    Destroy(rteCameras[i]);
                    rteCameras.RemoveAt(i);
                }
            }
        }

        private static void CreateRTECamera(GameObject camera, CameraEvent cameraEvent, IMeshesCache cache, List<RTECamera> rteCameras)
        {
            RTECamera rteCamera = camera.AddComponent<RTECamera>();
            rteCamera.Event = cameraEvent;
            rteCamera.MeshesCache = cache;
            rteCameras.Add(rteCamera);
        }

        private static void CreateRTECamera(GameObject camera, CameraEvent cameraEvent, IRenderersCache cache, List<RTECamera> rteCameras)
        {
            RTECamera rteCamera = camera.AddComponent<RTECamera>();
            rteCamera.Event = cameraEvent;
            rteCamera.RenderersCache = cache;
            rteCameras.Add(rteCamera);
        }
    }

}
