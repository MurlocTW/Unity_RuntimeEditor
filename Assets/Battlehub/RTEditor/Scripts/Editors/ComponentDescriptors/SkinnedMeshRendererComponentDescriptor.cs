//#define SIMPLIFIED_MESHRENDERER
using UnityEngine;
using System.Reflection;
using System;
using Battlehub.Utils;
using System.Collections.Generic;
using Battlehub.RTGizmos;
using Battlehub.RTCommon;

namespace Battlehub.RTEditor
{
#if SIMPLIFIED_MESHRENDERER
    public class SkinnedMeshRendererComponentDescriptor : ComponentDescriptorBase<SkinnedMeshRenderer, SkinnedMeshRendererGizmo>
    {
        public override PropertyDescriptor[] GetProperties(ComponentEditor editor, object converter)
        {
            MemberInfo materialsInfo = Strong.PropertyInfo((SkinnedMeshRenderer x) => x.sharedMaterials, "sharedMaterials");
            List<PropertyDescriptor> descriptors = new List<PropertyDescriptor>();
            descriptors.Add(new PropertyDescriptor("Materials", editor.Component, materialsInfo, materialsInfo));
            return descriptors.ToArray();
        }
    }
#else
    public class SkinnedMeshRendererComponentDescriptor : ComponentDescriptorBase<SkinnedMeshRenderer, SkinnedMeshRendererGizmo>
    {
        public override PropertyDescriptor[] GetProperties(ComponentEditor editor, object converter)
        {
            ILocalization lc = IOC.Resolve<ILocalization>();

            MemberInfo castShadowsInfo = Strong.PropertyInfo((SkinnedMeshRenderer x) => x.shadowCastingMode, "shadowCastingMode");
            MemberInfo receiveShadowsInfo = Strong.PropertyInfo((SkinnedMeshRenderer x) => x.receiveShadows, "receiveShadows");
            MemberInfo materialsInfo = Strong.PropertyInfo((SkinnedMeshRenderer x) => x.sharedMaterials, "sharedMaterials");
            MemberInfo lightProbesInfo = Strong.PropertyInfo((SkinnedMeshRenderer x) => x.lightProbeUsage, "lightProbeUsage");
            MemberInfo reflectionProbesInfo = Strong.PropertyInfo((SkinnedMeshRenderer x) => x.reflectionProbeUsage, "reflectionProbeUsage");
            MemberInfo anchorOverrideInfo = Strong.PropertyInfo((SkinnedMeshRenderer x) => x.probeAnchor, "probeAnchor");
            MemberInfo qualityInfo = Strong.PropertyInfo((SkinnedMeshRenderer x) => x.quality, "quality");
            MemberInfo updateWhenOffscreenInfo = Strong.PropertyInfo((SkinnedMeshRenderer x) => x.updateWhenOffscreen, "updateWhenOffscreen");
            MemberInfo skinnedMotionVectorsInfo = Strong.PropertyInfo((SkinnedMeshRenderer x) => x.skinnedMotionVectors, "skinnedMotionVectors");
            MemberInfo meshInfo = Strong.PropertyInfo((SkinnedMeshRenderer x) => x.sharedMesh, "sharedMesh");
            MemberInfo rootBoneInfo = Strong.PropertyInfo((SkinnedMeshRenderer x) => x.rootBone, "rootBone");
            MemberInfo boundsInfo = Strong.PropertyInfo((SkinnedMeshRenderer x) => x.localBounds, "localBounds");

            List<PropertyDescriptor> descriptors = new List<PropertyDescriptor>();

            descriptors.Add(new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_SkinnedMeshRenderer_CastShadows", "Cast Shadows"), editor.Component, castShadowsInfo));
            descriptors.Add(new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_SkinnedMeshRenderer_ReceiveShadows", "Receive Shadows"), editor.Component, receiveShadowsInfo, "m_ReceiveShadows"));
            descriptors.Add(new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_SkinnedMeshRenderer_Materials", "Materials"), editor.Component, materialsInfo, materialsInfo));
            descriptors.Add(new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_SkinnedMeshRenderer_LightProbes", "Light Probes"), editor.Component, lightProbesInfo, lightProbesInfo));
            descriptors.Add(new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_SkinnedMeshRenderer_ReflectionProbes", "Reflection Probes"), editor.Component, reflectionProbesInfo, reflectionProbesInfo));
            descriptors.Add(new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_SkinnedMeshRenderer_AnchorOverride", "Anchor Override"), editor.Component, anchorOverrideInfo, anchorOverrideInfo));
            descriptors.Add(new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_SkinnedMeshRenderer_Quality", "Quality"), editor.Component, qualityInfo, qualityInfo));
            descriptors.Add(new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_SkinnedMeshRenderer_UpdateWhenOffscreen", "Update When Offscreen"), editor.Component, updateWhenOffscreenInfo, "m_UpdateWhenOffscreen"));
            descriptors.Add(new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_SkinnedMeshRenderer_SkinnedMotionVectors", "Skinned Motion Vectors"), editor.Component, skinnedMotionVectorsInfo, skinnedMotionVectorsInfo));
            descriptors.Add(new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_SkinnedMeshRenderer_Mesh", "Mesh"), editor.Component, meshInfo, meshInfo));
            descriptors.Add(new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_SkinnedMeshRenderer_RootBone", "Root Bone"), editor.Component, rootBoneInfo, rootBoneInfo));
            descriptors.Add(new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_SkinnedMeshRenderer_Bounds", "Bounds"), editor.Component, boundsInfo, boundsInfo));

            return descriptors.ToArray();
        }
    }
#endif

}

