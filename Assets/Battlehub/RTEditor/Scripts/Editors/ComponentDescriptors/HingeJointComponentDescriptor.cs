﻿using Battlehub.Utils;
using System;
using System.Reflection;
using UnityEngine;

namespace Battlehub.RTEditor
{
    public class HingeJointComponentDescriptor : ComponentDescriptorBase<HingeJoint>
    {
        public override PropertyDescriptor[] GetProperties(ComponentEditor editor, object converter)
        {
            MemberInfo connectedBodyInfo = Strong.PropertyInfo((HingeJoint x) => x.connectedBody, "connectedBody");
            MemberInfo anchorInfo = Strong.PropertyInfo((HingeJoint x) => x.anchor, "anchor");
            MemberInfo axisInfo = Strong.PropertyInfo((HingeJoint x) => x.axis, "axis");
            MemberInfo autoConfigAnchorInfo = Strong.PropertyInfo((HingeJoint x) => x.autoConfigureConnectedAnchor, "autoConfigureConnectedAnchor");
            MemberInfo connectedAnchorInfo = Strong.PropertyInfo((HingeJoint x) => x.connectedAnchor, "connectedAnchor");
            MemberInfo useSpringInfo = Strong.PropertyInfo((HingeJoint x) => x.useSpring, "useSpring");
            MemberInfo springInfo = Strong.PropertyInfo((HingeJoint x) => x.spring, "spring");
            MemberInfo useMotorInfo = Strong.PropertyInfo((HingeJoint x) => x.useMotor, "useMotor");
            MemberInfo motorInfo = Strong.PropertyInfo((HingeJoint x) => x.motor, "motor");
            MemberInfo useLimitsInfo = Strong.PropertyInfo((HingeJoint x) => x.useLimits, "useLimits");
            MemberInfo limitsInfo = Strong.PropertyInfo((HingeJoint x) => x.limits, "limits");
            MemberInfo breakForceInfo = Strong.PropertyInfo((HingeJoint x) => x.breakForce, "breakForce");
            MemberInfo breakTorqueInfo = Strong.PropertyInfo((HingeJoint x) => x.breakTorque, "breakTorque");
            MemberInfo enableCollisionInfo = Strong.PropertyInfo((HingeJoint x) => x.enableCollision, "enableCollision");
            MemberInfo enablePreporcessingInfo = Strong.PropertyInfo((HingeJoint x) => x.enablePreprocessing, "enablePreprocessing");

            return new[]
            {
                new PropertyDescriptor("ConnectedBody", editor.Component, connectedBodyInfo),
                new PropertyDescriptor("Anchor", editor.Component, anchorInfo, "m_Anchor"),
                new PropertyDescriptor("Axis", editor.Component, axisInfo, "m_Axis"),
                new PropertyDescriptor("Auto Configure Connected Anchor", editor.Component, autoConfigAnchorInfo, "m_AutoConfigureConnectedAnchor"),
                new PropertyDescriptor("Connected Anchor", editor.Component, connectedAnchorInfo, "m_ConnectedAnchor"),
                new PropertyDescriptor("Use Spring", editor.Component, useSpringInfo, "m_UseSpring"),
                new PropertyDescriptor("Spring", editor.Component, springInfo),
                new PropertyDescriptor("Use Motor", editor.Component, useMotorInfo, "m_UseMotor"),
                new PropertyDescriptor("Motor", editor.Component, motorInfo),
                new PropertyDescriptor("Use Limits", editor.Component, useLimitsInfo, "m_UseLimits"),
                new PropertyDescriptor("Limits", editor.Component, limitsInfo)
                {
                    ChildDesciptors = new[]
                    {
                        new PropertyDescriptor("Min", null, Strong.PropertyInfo((JointLimits x) => x.min, "min")),
                        new PropertyDescriptor("Max", null, Strong.PropertyInfo((JointLimits x) => x.max, "max")),
                        new PropertyDescriptor("Bounciness", null, Strong.PropertyInfo((JointLimits x) => x.bounciness, "bounciness")),
                        new PropertyDescriptor("Bounce Min Velocity", null, Strong.PropertyInfo((JointLimits x) => x.bounceMinVelocity, "bounceMinVelocity")),
                        new PropertyDescriptor("Contact Distance", null, Strong.PropertyInfo((JointLimits x) => x.contactDistance, "contactDistance")),
                    }
                },
                new PropertyDescriptor("Break Force", editor.Component, breakForceInfo, "m_BreakForce"),
                new PropertyDescriptor("Break Torque", editor.Component, breakTorqueInfo, "m_BreakTorque"),
                new PropertyDescriptor("Enable Collision", editor.Component, enableCollisionInfo, "m_EnableCollision"),
                new PropertyDescriptor("Enable Preprocessing", editor.Component, enablePreporcessingInfo, "m_EnablePreprocessing"),
            };            
        }
    }
}
