using UnityEngine;
using System.Reflection;
using Battlehub.Utils;
using Battlehub.RTCommon;

namespace Battlehub.RTEditor
{
    public class MeshColliderComponentDescriptor : ComponentDescriptorBase<MeshCollider>
    {
        public override PropertyDescriptor[] GetProperties(ComponentEditor editor, object converter)
        {
            ILocalization lc = IOC.Resolve<ILocalization>();

            PropertyEditorCallback valueChanged = () => editor.BuildEditor();

            MemberInfo convexInfo = Strong.PropertyInfo((MeshCollider x) => x.convex, "convex");
            MemberInfo isTriggerInfo = Strong.PropertyInfo((MeshCollider x) => x.isTrigger, "isTrigger");
            MemberInfo materialInfo = Strong.PropertyInfo((MeshCollider x) => x.sharedMaterial, "sharedMaterial");
            MemberInfo meshInfo = Strong.PropertyInfo((MeshCollider x) => x.sharedMesh, "sharedMesh");

            MeshCollider collider = (MeshCollider)editor.Component;
            if (collider.convex)
            {
                return new[]
                {
                    new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_MeshCollider_Convex", "Convex"), editor.Component, convexInfo, convexInfo, valueChanged) { AnimationPropertyName = "m_Convex"},
                    new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_MeshCollider_IsTrigger", "Is Trigger"), editor.Component, isTriggerInfo, "m_IsTrigger"),
                    new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_MeshCollider_Material", "Material"), editor.Component, materialInfo, materialInfo),
                    new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_MeshCollider_Mesh", "Mesh"), editor.Component, meshInfo, meshInfo),
                };
            }
            else
            {
                return new[]
                {
                    new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_MeshCollider_Convex", "Convex"), editor.Component, convexInfo, convexInfo, valueChanged)  { AnimationPropertyName = "m_Convex"},
                    new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_MeshCollider_Material", "Material"), editor.Component, materialInfo, materialInfo),
                    new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_MeshCollider_Mesh", "Mesh"), editor.Component, meshInfo, meshInfo),
                };
            }
        }
    }

}

