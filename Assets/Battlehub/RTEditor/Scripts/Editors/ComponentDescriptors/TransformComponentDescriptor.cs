using UnityEngine;
using System.Reflection;
using Battlehub.Utils;
using Battlehub.RTCommon;

namespace Battlehub.RTEditor
{
    public class TransformComponentDescriptor : ComponentDescriptorBase<Transform>
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

    public class TransformPropertyConverter 
    {
        private ISettingsComponent m_settingsComponent = IOC.Resolve<ISettingsComponent>();

        public Vector3 LocalPosition
        {
            get
            {
                if (ExposeToEditor == null)
                {
                    return Vector3.zero;
                }

                if(m_settingsComponent != null && m_settingsComponent.SystemOfMeasurement == SystemOfMeasurement.Imperial)
                {
                    return UnitsConverter.MetersToFeet(ExposeToEditor.LocalPosition);
                }

                return ExposeToEditor.LocalPosition;
            }
            set
            {
                if (ExposeToEditor == null)
                {
                    return;
                }

                if (m_settingsComponent != null && m_settingsComponent.SystemOfMeasurement == SystemOfMeasurement.Imperial)
                {
                    ExposeToEditor.LocalPosition = UnitsConverter.FeetToMeters(value);
                }
                else
                {
                    ExposeToEditor.LocalPosition = value;
                }
            }
        }

        public Vector3 LocalEuler
        {
            get
            {
                if(ExposeToEditor == null)
                {
                    return Vector3.zero;
                }

                return ExposeToEditor.LocalEuler;
            }
            set
            {
                if(ExposeToEditor == null)
                {
                    return;
                }
                ExposeToEditor.LocalEuler = value;
            }
        }

        public Vector3 LocalScale
        {
            get
            {
                if (ExposeToEditor == null)
                {
                    return Vector3.zero;
                }

                return ExposeToEditor.LocalScale;
            }
            set
            {
                if (ExposeToEditor == null)
                {
                    return;
                }
                ExposeToEditor.LocalScale = value;
            }
        }

        public ExposeToEditor ExposeToEditor
        {
            get;
            set;
        }
    }
}

