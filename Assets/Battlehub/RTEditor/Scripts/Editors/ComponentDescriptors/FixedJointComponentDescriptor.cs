using Battlehub.RTCommon;
using Battlehub.Utils;
using System.Reflection;
using UnityEngine;

namespace Battlehub.RTEditor
{
    public class FixedJointComponentDescriptor : ComponentDescriptorBase<FixedJoint>
    {
        public override PropertyDescriptor[] GetProperties(ComponentEditor editor, object converter)
        {
            ILocalization lc = IOC.Resolve<ILocalization>();

            MemberInfo connectedBodyInfo = Strong.PropertyInfo((FixedJoint x) => x.connectedBody, "connectedBody");
            MemberInfo breakForceInfo = Strong.PropertyInfo((FixedJoint x) => x.breakForce, "breakForce");
            MemberInfo breakTorqueInfo = Strong.PropertyInfo((FixedJoint x) => x.breakTorque, "breakTorque");
            MemberInfo enableCollisionInfo = Strong.PropertyInfo((FixedJoint x) => x.enableCollision, "enableCollision");
            MemberInfo enablePreporcessingInfo = Strong.PropertyInfo((FixedJoint x) => x.enablePreprocessing, "enablePreprocessing");

            return new[]
            {
                new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_FixedJoint_ConnectedBody", "Connected Body"), editor.Component, connectedBodyInfo),
                new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_FixedJoint_BreakForce", "Break Force"), editor.Component, breakForceInfo, "m_BreakForce"),
                new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_FixedJoint_BreakTorque", "Break Torque"), editor.Component, breakTorqueInfo, "m_BreakTorque"),
                new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_FixedJoint_EnableCollision", "Enable Collision"), editor.Component, enableCollisionInfo, "m_EnableCollision"),
                new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_FixedJoint_EnablePerprocessing", "Enable Preprocessing"), editor.Component, enablePreporcessingInfo, "m_EnablePreprocessing"),
            };            
        }
    }
}
