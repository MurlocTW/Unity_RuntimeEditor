using Battlehub.RTEditor;
using Battlehub.ProBuilderIntegration;
using Battlehub.RTCommon;

namespace Battlehub.RTBuilder.URP
{
    public class ProBuilderInitURP : EditorExtension
    {
        private RenderersCache m_selectionPickerCache;

        protected override void OnEditorExist()
        {
            if(RenderPipelineInfo.Type != RPType.URP)
            {
                Destroy(this);
                return;
            }

            base.OnEditorExist();
            m_selectionPickerCache = gameObject.AddComponent<RenderersCache>();
            IOC.Register<IRenderersCache>("ProBuilder.SelectionPickerCache", m_selectionPickerCache);
            PBSelectionPicker.Renderer = new PBSelectionPickerRendererURP(m_selectionPickerCache);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            IOC.Unregister<IRenderersCache>("ProBuilder.SelectionPickerCache", m_selectionPickerCache);
            Destroy(m_selectionPickerCache);
            m_selectionPickerCache = null;
        }

        protected override void OnEditorClosed()
        {
            base.OnEditorClosed();
            IOC.Unregister<IRenderersCache>("ProBuilder.SelectionPickerCache", m_selectionPickerCache);
            Destroy(m_selectionPickerCache);
            m_selectionPickerCache = null;
        }
    }
}

