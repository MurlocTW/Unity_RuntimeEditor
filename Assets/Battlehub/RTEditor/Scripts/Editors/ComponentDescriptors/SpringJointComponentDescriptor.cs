using Battlehub.RTCommon;
using Battlehub.Utils;
using System;
using System.Reflection;
using UnityEngine;

namespace Battlehub.RTEditor
{
    public class SpringJointComponentDescriptor : ComponentDescriptorBase<SpringJoint>
    {
        public override PropertyDescriptor[] GetProperties(ComponentEditor editor, object converter)
        {
            ILocalization lc = IOC.Resolve<ILocalization>();

            MemberInfo connectedBodyInfo = Strong.PropertyInfo((SpringJoint x) => x.connectedBody, "connectedBody");
            MemberInfo anchorInfo = Strong.PropertyInfo((SpringJoint x) => x.anchor, "anchor");
            MemberInfo autoConfigAnchorInfo = Strong.PropertyInfo((SpringJoint x) => x.autoConfigureConnectedAnchor, "autoConfigureConnectedAnchor");
            MemberInfo connectedAnchorInfo = Strong.PropertyInfo((SpringJoint x) => x.connectedAnchor, "connectedAnchor");
            MemberInfo springInfo = Strong.PropertyInfo((SpringJoint x) => x.spring, "spring");
            MemberInfo damperInfo = Strong.PropertyInfo((SpringJoint x) => x.damper, "damper");
            MemberInfo minDistanceInfo = Strong.PropertyInfo((SpringJoint x) => x.minDistance, "minDistance");
            MemberInfo maxDistanceInfo = Strong.PropertyInfo((SpringJoint x) => x.maxDistance, "maxDistance");
            MemberInfo toleranceInfo = Strong.PropertyInfo((SpringJoint x) => x.tolerance, "tolerance");
            MemberInfo breakForceInfo = Strong.PropertyInfo((SpringJoint x) => x.breakForce, "breakForce");
            MemberInfo breakTorqueInfo = Strong.PropertyInfo((SpringJoint x) => x.breakTorque, "breakTorque");
            MemberInfo enableCollisionInfo = Strong.PropertyInfo((SpringJoint x) => x.enableCollision, "enableCollision");
            MemberInfo enablePreporcessingInfo = Strong.PropertyInfo((SpringJoint x) => x.enablePreprocessing, "enablePreprocessing");

            return new[]
            {
                new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_SpringJoint_ConnectedBody", "ConnectedBody"), editor.Component, connectedBodyInfo),
                new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_SpringJoint_Anchor", "Anchor"), editor.Component, anchorInfo, "m_Anchor"),
                new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_SpringJoint_AutoConfigConnectedAnchor", "Auto Configure Connected Anchor"), editor.Component, autoConfigAnchorInfo, "m_AutoConfigureConnectedAnchor"),
                new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_SpringJoint_ConnectAnchor", "Connected Anchor"), editor.Component, connectedAnchorInfo, "m_ConnectedAnchor"),
                new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_SpringJoint_Spring", "Spring"), editor.Component, springInfo, "m_Spring"),
                new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_SpringJoint_Damper", "Damper"), editor.Component, damperInfo, "m_Damper"),
                new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_SpringJoint_MinDistance", "MinDistance"), editor.Component, minDistanceInfo, "m_MinDistance"),
                new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_SpringJoint_MaxDistance", "MaxDistance"), editor.Component, maxDistanceInfo, "m_MaxDistance"),
                new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_SpringJoint_Tolerance", "Tolerance"), editor.Component, toleranceInfo, "m_Tolerance"),
                new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_SpringJoint_BreakForce", "Break Force"), editor.Component, breakForceInfo, "m_BreakForce"),
                new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_SpringJoint_BreakTorque", "Break Torque"), editor.Component, breakTorqueInfo, "m_BreakTorque"),
                new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_SpringJoint_EnableCollision", "Enable Collision"), editor.Component, enableCollisionInfo, "m_EnableCollision"),
                new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_SpringJoint_EnablePreprocessing", "Enable Preprocessing"), editor.Component, enablePreporcessingInfo, "m_EnablePreprocessing"),
            };            
        }
    }
}
