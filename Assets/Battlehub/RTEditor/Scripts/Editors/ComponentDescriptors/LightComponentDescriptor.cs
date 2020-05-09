using UnityEngine;
using System.Reflection;
using System;
using Battlehub.Utils;
using System.Collections.Generic;
using Battlehub.RTGizmos;
using Battlehub.RTCommon;

namespace Battlehub.RTEditor
{
    public class LightComponentDescriptor : ComponentDescriptorBase<Light, LightGizmo>
    {
        public override PropertyDescriptor[] GetProperties(ComponentEditor editor, object converter)
        {
            ILocalization lc = IOC.Resolve<ILocalization>();

            Light light = (Light)editor.Component;

            PropertyEditorCallback valueChanged = () => editor.BuildEditor();

            MemberInfo enabledInfo = Strong.PropertyInfo((Light x) => x.enabled, "enabled");
            MemberInfo lightTypeInfo = Strong.PropertyInfo((Light x) => x.type, "type");
            MemberInfo colorInfo = Strong.PropertyInfo((Light x) => x.color, "color");
            MemberInfo intensityInfo = Strong.PropertyInfo((Light x) => x.intensity, "intensity");
            MemberInfo bounceIntensityInfo = Strong.PropertyInfo((Light x) => x.bounceIntensity, "bounceIntensity");
            MemberInfo shadowTypeInfo = Strong.PropertyInfo((Light x) => x.shadows, "shadows");
            MemberInfo cookieInfo = Strong.PropertyInfo((Light x) => x.cookie, "cookie");
            MemberInfo cookieSizeInfo = Strong.PropertyInfo((Light x) => x.cookieSize, "cookieSize");
            MemberInfo flareInfo = Strong.PropertyInfo((Light x) => x.flare, "flare");
            MemberInfo renderModeInfo = Strong.PropertyInfo((Light x) => x.renderMode, "renderMode");

            List<PropertyDescriptor> descriptors = new List<PropertyDescriptor>();
            descriptors.Add(new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_Light_Enabled", "Enabled"), editor.Component, enabledInfo, "m_Enabled"));
            descriptors.Add(new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_Light_Type", "Type"), editor.Component, lightTypeInfo, lightTypeInfo, valueChanged));
            if (light.type == LightType.Point)
            {
                MemberInfo rangeInfo = Strong.PropertyInfo((Light x) => x.range, "range");
                descriptors.Add(new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_Light_Range", "Range"), editor.Component, rangeInfo, "m_Range"));
            }
            else if (light.type == LightType.Spot)
            {
                MemberInfo rangeInfo = Strong.PropertyInfo((Light x) => x.range, "range");
                MemberInfo spotAngleInfo = Strong.PropertyInfo((Light x) => x.spotAngle, "spotAngle");
                descriptors.Add(new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_Light_Range", "Range"), editor.Component, rangeInfo, "m_Range"));
                descriptors.Add(new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_Light_SpotAngle", "Spot Angle"), editor.Component, spotAngleInfo, spotAngleInfo, null, new Range(1, 179)) { AnimationPropertyName = "m_SpotAngle" } );
            }

            descriptors.Add(new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_Light_Color", "Color"), editor.Component, colorInfo, "m_Color"));
            descriptors.Add(new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_Light_Intensity", "Intensity"), editor.Component, intensityInfo, intensityInfo, null, new Range(0, 8)) { AnimationPropertyName = "m_Intensity" });
            descriptors.Add(new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_Light_BounceIntensity", "Bounce Intensity"), editor.Component, bounceIntensityInfo, bounceIntensityInfo, null, new Range(0, 8)) { AnimationPropertyName = "m_BounceIntensity" });

            if (light.type != LightType.Area)
            {
                descriptors.Add(new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_Light_ShadowType", "Shadow Type"), editor.Component, shadowTypeInfo, shadowTypeInfo, valueChanged));
                if (light.shadows == LightShadows.Soft || light.shadows == LightShadows.Hard)
                {
                    MemberInfo shadowStrengthInfo = Strong.PropertyInfo((Light x) => x.shadowStrength, "shadowStrength");
                    MemberInfo shadowResolutionInfo = Strong.PropertyInfo((Light x) => x.shadowResolution, "shadowResolution");
                    MemberInfo shadowBiasInfo = Strong.PropertyInfo((Light x) => x.shadowBias, "shadowBias");
                    MemberInfo shadowNormalBiasInfo = Strong.PropertyInfo((Light x) => x.shadowNormalBias, "shadowNormalBias");
                    MemberInfo shadowNearPlaneInfo = Strong.PropertyInfo((Light x) => x.shadowNearPlane, "shadowNearPlane");

                    descriptors.Add(new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_Light_Strength", "Strength"), editor.Component, shadowStrengthInfo, shadowStrengthInfo, null, new Range(0, 1)) { AnimationPropertyName = "m_Strength" });
                    descriptors.Add(new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_Light_Resolution", "Resoultion"), editor.Component, shadowResolutionInfo, shadowResolutionInfo));
                    descriptors.Add(new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_Light_Bias", "Bias"), editor.Component, shadowBiasInfo, shadowBiasInfo, null, new Range(0, 2)) { AnimationPropertyName = "m_Bias" });
                    descriptors.Add(new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_Light_NormalBias", "Normal Bias"), editor.Component, shadowNormalBiasInfo, shadowNormalBiasInfo, null, new Range(0, 3)) { AnimationPropertyName = "m_NormalBias" });
                    descriptors.Add(new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_Light_ShadowNearPlane", "Shadow Near Plane"), editor.Component, shadowNearPlaneInfo, shadowNearPlaneInfo, null, new Range(0, 10)) { AnimationPropertyName = "m_NearPlane" });
                }

                descriptors.Add(new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_Light_Cookie", "Cookie"), editor.Component, cookieInfo, cookieInfo));
                descriptors.Add(new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_Light_CookieSize", "Cookie Size"), editor.Component, cookieSizeInfo, cookieSizeInfo));
            }

            descriptors.Add(new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_Light_Flare", "Flare"), editor.Component, flareInfo, flareInfo));
            descriptors.Add(new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_Light_RenderMode", "Render Mode"), editor.Component, renderModeInfo, renderModeInfo));

            return descriptors.ToArray();
        }
    }
}

