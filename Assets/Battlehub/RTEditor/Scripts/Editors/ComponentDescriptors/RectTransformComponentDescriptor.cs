using UnityEngine;
using System.Reflection;
using Battlehub.Utils;
using Battlehub.RTCommon;

namespace Battlehub.RTEditor
{
    public class RectTransformComponentDescriptor : ComponentDescriptorBase<RectTransform>
    {
        public override object CreateConverter(ComponentEditor editor)
        {
            TransformPropertyConverter converter = new TransformPropertyConverter();
            converter.ExposeToEditor = editor.Component.GetComponent<ExposeToEditor>();
            return converter;
        }

        public override PropertyDescriptor[] GetProperties(ComponentEditor editor, object converterObj)
        {
            ILocalization lc = IOC.Resolve<ILocalization>();

            TransformPropertyConverter converter = (TransformPropertyConverter)converterObj;

            MemberInfo position = Strong.PropertyInfo((Transform x) => x.localPosition, "localPosition");
            MemberInfo positionConverted = Strong.PropertyInfo((TransformPropertyConverter x) => x.LocalPosition, "LocalPosition");
            MemberInfo rotation = Strong.PropertyInfo((Transform x) => x.localRotation, "localRotation");
            MemberInfo rotationConverted = Strong.PropertyInfo((TransformPropertyConverter x) => x.LocalEuler, "LocalEulerAngles");
            MemberInfo scale = Strong.PropertyInfo((Transform x) => x.localScale, "localScale");
            MemberInfo scaleConverted = Strong.PropertyInfo((TransformPropertyConverter x) => x.LocalScale, "LocalScale");

            return new[]
                {
                    new PropertyDescriptor( lc.GetString("ID_RTEditor_CD_Transform_Position", "Position"),converter, positionConverted, position) ,
                    new PropertyDescriptor( lc.GetString("ID_RTEditor_CD_Transform_Rotation", "Rotation"), converter, rotationConverted, rotation),
                    new PropertyDescriptor( lc.GetString("ID_RTEditor_CD_Transform_Scale", "Scale"), converter, scaleConverted, scale)
                };
        }
    }
}

