using Battlehub.RTCommon;
using Battlehub.UIControls;
using UnityEngine;
using UnityEngine.UI;

namespace Battlehub.RTEditor.URP
{
    public class URPInit : EditorExtension
    {
        [SerializeField]
        private GameObject m_prefab = null;

        private GameObject m_instance;

        private IWindowManager m_windowManager;

        private GameObject m_foregroundOutput;
        
        protected override void OnEditorExist()
        {
            base.OnEditorExist();
            if(RenderPipelineInfo.Type != RPType.URP)
            {
                return;
            }

            m_instance = Instantiate(m_prefab, transform, false);
            m_instance.name = m_prefab.name;

            IRTE rte = IOC.Resolve<IRTE>();
            Canvas foregroundCanvas = IOC.Resolve<IRTEAppearance>().UIForegroundScaler.GetComponent<Canvas>();
            Camera foregroundCamera = foregroundCanvas.worldCamera;
            if(foregroundCamera != null)
            {

                GameObject foregroundLayer = new GameObject("ForegroundLayer");
                foregroundLayer.transform.SetParent(rte.Root, false);
                foregroundCanvas = foregroundLayer.AddComponent<Canvas>();
                foregroundCanvas.renderMode = RenderMode.ScreenSpaceOverlay;

                foregroundCamera.gameObject.SetActive(false);
                foregroundCamera.backgroundColor = new Color(0, 0, 0, 0);

                m_foregroundOutput = new GameObject("Output");
                m_foregroundOutput.transform.SetParent(foregroundCanvas.transform, false);
                m_foregroundOutput.AddComponent<RectTransform>().Stretch();

                RenderTextureCamera renderTextureCamera = foregroundCamera.gameObject.AddComponent<RenderTextureCamera>();
                renderTextureCamera.OutputRoot = foregroundCanvas.gameObject.GetComponent<RectTransform>();
                renderTextureCamera.Output = m_foregroundOutput.AddComponent<RawImage>();
                renderTextureCamera.OverlayMaterial = new Material(Shader.Find("Battlehub/URP/RTEditor/UIForeground"));
                foregroundCamera.gameObject.SetActive(true);

                foregroundCanvas.sortingOrder = -1;
            }
        }

        protected override void OnEditorClosed()
        {
            base.OnEditorClosed();
            Destroy(m_instance);
        }
    }
}

