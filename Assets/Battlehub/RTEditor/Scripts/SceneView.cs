using Battlehub.RTCommon;
using UnityEngine;

namespace Battlehub.RTEditor
{
    public class SceneView : RuntimeWindow
    {
        protected override void AwakeOverride()
        {
            ActivateOnAnyKey = true;
            WindowType = RuntimeWindowType.Scene;
            base.AwakeOverride();   

            if(RenderPipelineInfo.Type == RPType.URP)
            {
                RTEGraphicsLayer graphicsLayer = GetComponent<RTEGraphicsLayer>();
                DestroyImmediate(graphicsLayer);
            }
        }

        protected virtual void Start()
        {
            if (!GetComponent<SceneViewInput>())
            {
                gameObject.AddComponent<SceneViewInput>();
            }

            if (!GetComponent<SceneViewImpl>())
            {
                gameObject.AddComponent<SceneViewImpl>();
            }
        }
    }
}
