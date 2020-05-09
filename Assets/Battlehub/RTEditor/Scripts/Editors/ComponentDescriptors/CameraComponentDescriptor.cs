using Battlehub.RTCommon;
using Battlehub.Utils;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Battlehub.RTEditor
{
    public class CameraComponentDescriptor : ComponentDescriptorBase<Camera>
    {
        public enum Projection
        {
            Perspective,
            Orthographic
        }

        public class CameraPropertyConverter
        {
            public Projection Projection
            {
                get
                {
                    if (Component == null) {return Projection.Perspective; }
                    return Component.orthographic ? Projection.Orthographic : Projection.Perspective;
                }
                set
                {
                    if (Component == null) { return; }
                    Component.orthographic = value == Projection.Orthographic;
                }
            }

            public Camera Component { get; set; }
        }

        public override object CreateConverter(ComponentEditor editor)
        {
            CameraPropertyConverter converter = new CameraPropertyConverter();
            converter.Component = (Camera)editor.Component;
            return converter;
        }

        public override PropertyDescriptor[] GetProperties(ComponentEditor editor, object converter)
        {
            ILocalization lc = IOC.Resolve<ILocalization>();

            Camera camera = (Camera)editor.Component;

            PropertyEditorCallback valueChanged = () => editor.BuildEditor();
            MemberInfo projection = Strong.PropertyInfo((CameraPropertyConverter x) => x.Projection, "Projection");
            MemberInfo orthographic = Strong.PropertyInfo((Camera x) => x.orthographic, "orthographic");
            MemberInfo fov = Strong.PropertyInfo((Camera x) => x.fieldOfView, "fieldOfView");
            MemberInfo orthographicSize = Strong.PropertyInfo((Camera x) => x.orthographicSize, "orthographicSize");

            List<PropertyDescriptor> descriptors = new List<PropertyDescriptor>();
            descriptors.Add(new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_Camera_Projection", "Projection"), converter, projection, orthographic, valueChanged));
            
            if(!camera.orthographic)
            {
                descriptors.Add(new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_Camera_Fov", "Field Of View"), editor.Component, fov, "field of view"));
            }
            else
            {
                descriptors.Add(new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_Camera_Size", "Size"), editor.Component, orthographicSize, "orthographic size"));
            }
            
            return descriptors.ToArray();
        }
    }

}

