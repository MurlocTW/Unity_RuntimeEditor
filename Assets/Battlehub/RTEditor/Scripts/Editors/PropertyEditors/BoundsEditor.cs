using Battlehub.RTCommon;
using Battlehub.Utils;
using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

namespace Battlehub.RTEditor
{
    public class BoundsAccessor
    {
        private PropertyEditor<Bounds> m_editor;

        public Vector3 Center
        {
            get { return GetBounds().center; }
            set
            {
                Bounds bounds = GetBounds();
                bounds.center = value;
                m_editor.SetValue(bounds);
            }
        }

        public Vector3 Extents
        {
            get { return GetBounds().extents; }
            set
            {
                Bounds bounds = GetBounds();
                bounds.extents = value;
                m_editor.SetValue(bounds);
            }
        }

        private Bounds GetBounds()
        {
            return m_editor.GetValue();
        }

        public BoundsAccessor(PropertyEditor<Bounds> editor)
        {
            m_editor = editor;
        }
    }

    public class BoundsEditor : PropertyEditor<Bounds>
    {
        [SerializeField]
        private Vector3Editor m_center = null;
        [SerializeField]
        private Vector3Editor m_extents = null;
        
        protected override void AwakeOverride()
        {
            base.AwakeOverride();
        }

        protected override void OnDestroyOverride()
        {
            base.OnDestroyOverride();
        }

        protected override void InitOverride(object target, object accessor, MemberInfo memberInfo, Action<object, object> eraseTargetCallback, string label = null)
        {
            base.InitOverride(target, accessor, memberInfo, eraseTargetCallback, label);

            ILocalization localization = IOC.Resolve<ILocalization>();

            BoundsAccessor boundsAccessor = new BoundsAccessor(this);
            m_center.Init(boundsAccessor, boundsAccessor, Strong.PropertyInfo((BoundsAccessor x) => x.Center, "Center"), null, localization.GetString("ID_RTEditor_PE_BoundsEditor_Center", "Center"), OnValueChanging, null, OnEndEdit, false);
            m_extents.Init(boundsAccessor, boundsAccessor, Strong.PropertyInfo((BoundsAccessor x) => x.Extents, "Extents"), null, localization.GetString("ID_RTEditor_PE_BoundsEditor_Extents", "Extents"), OnValueChanging, null, OnEndEdit, false);
        }

        protected override void ReloadOverride(bool force)
        {
            base.ReloadOverride(force);
            m_center.Reload();
            m_extents.Reload();
        }

        private void OnValueChanging()
        {
            BeginEdit();
        }

        private void OnEndEdit()
        {
            EndEdit();
        }
    }
}

