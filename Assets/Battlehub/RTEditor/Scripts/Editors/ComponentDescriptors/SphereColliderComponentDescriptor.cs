using UnityEngine;
using System.Reflection;
using Battlehub.Utils;
using Battlehub.RTGizmos;
using Battlehub.RTCommon;

namespace Battlehub.RTEditor
{
    public class SphereColliderComponentDescriptor : ComponentDescriptorBase<SphereCollider, SphereColliderGizmo>
    {
        public override PropertyDescriptor[] GetProperties(ComponentEditor editor, object converter)
        {
            ILocalization lc = IOC.Resolve<ILocalization>();

            MemberInfo isTriggerInfo = Strong.PropertyInfo((SphereCollider x) => x.isTrigger, "isTrigger");
            MemberInfo materialInfo = Strong.PropertyInfo((SphereCollider x) => x.sharedMaterial, "sharedMaterial");
            MemberInfo centerInfo = Strong.PropertyInfo((SphereCollider x) => x.center, "center");
            MemberInfo radiusInfo = Strong.PropertyInfo((SphereCollider x) => x.radius, "radius");

            return new[]
            {
                new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_SphereCollider_IsTrigger", "Is Trigger"), editor.Component, isTriggerInfo, "m_IsTrigger"),
                new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_SphereCollider_Material", "Material"), editor.Component, materialInfo),
                new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_SphereCollider_Center", "Center"), editor.Component, centerInfo, "m_Center"),
                new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_SphereCollider_Radius", "Radius"), editor.Component, radiusInfo, "m_Radius")
            };
        }
    }
}

