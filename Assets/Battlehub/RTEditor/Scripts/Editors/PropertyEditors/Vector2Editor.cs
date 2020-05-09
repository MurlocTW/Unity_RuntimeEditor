using Battlehub.RTCommon;
using TMPro;
using UnityEngine;

namespace Battlehub.RTEditor
{
    public class Vector2Editor : ConvertablePropertyEditor<Vector2>
    {
        [SerializeField]
        private TMP_InputField m_xInput = null;
        [SerializeField]
        private TMP_InputField m_yInput = null;
        [SerializeField]
        protected DragField[] m_dragFields = null;
        
        protected override void AwakeOverride()
        {
            base.AwakeOverride();

            m_xInput.onValueChanged.AddListener(OnXValueChanged);
            m_yInput.onValueChanged.AddListener(OnYValueChanged);
            m_xInput.onEndEdit.AddListener(OnEndEdit);
            m_yInput.onEndEdit.AddListener(OnEndEdit);

            for (int i = 0; i < m_dragFields.Length; ++i)
            {
                if (m_dragFields[i])
                {
                    m_dragFields[i].EndDrag.AddListener(OnEndDrag);
                }
            }

        }

        protected override void OnDestroyOverride()
        {
            base.OnDestroyOverride();
            if (m_xInput != null)
            {
                m_xInput.onValueChanged.RemoveListener(OnXValueChanged);
                m_xInput.onEndEdit.RemoveListener(OnEndEdit);   
            }

            if (m_yInput != null)
            {
                m_yInput.onValueChanged.RemoveListener(OnYValueChanged);
                m_yInput.onEndEdit.RemoveListener(OnEndEdit);
            }

            for (int i = 0; i < m_dragFields.Length; ++i)
            {
                if (m_dragFields[i])
                {
                    m_dragFields[i].EndDrag.RemoveListener(OnEndDrag);
                }
            }
        }

        protected override void SetInputField(Vector2 value)
        {
            m_xInput.text = FromMeters(value.x).ToString();
            m_yInput.text = FromMeters(value.y).ToString();
        }

        private void OnXValueChanged(string value)
        {
            float val;
            if (float.TryParse(value, out val))
            {
                Vector2 vector = GetValue();
                vector.x = ToMeters(val);
                SetValue(vector);
            }
        }

        private void OnYValueChanged(string value)
        {
            float val;
            if (float.TryParse(value, out val))
            {
                Vector2 vector = GetValue();
                vector.y = ToMeters(val);
                SetValue(vector);
            }
        }

        private void OnEndEdit(string value)
        {
            Vector2 vector = GetValue();
            m_xInput.text = FromMeters(vector.x).ToString();
            m_yInput.text = FromMeters(vector.y).ToString();

            EndEdit();
        }

        protected void OnEndDrag()
        {
            EndEdit();
        } 
    }
}
