using UnityEngine;

namespace Battlehub.RTCommon
{
    public class SpriteGizmo : MonoBehaviour
    {
        public Mesh Mesh;

        private Vector3 m_position;
        private Quaternion m_rotation;

        [SerializeField, HideInInspector]
        private SphereCollider m_collider;
        private SphereCollider m_destroyedCollider;

        [SerializeField]
        private float m_scale = 1.0f;
        public float Scale
        {
            get { return m_scale; }
            set
            {
                if(m_scale != value)
                {
                    m_scale = value;
                    UpdateCollider();
                }
            }
        }
        
        private void OnEnable()
        {
            m_collider = GetComponent<SphereCollider>();

            if (m_collider == null || m_collider == m_destroyedCollider)
            {
                m_collider = gameObject.AddComponent<SphereCollider>();
            }
            if (m_collider != null)
            {
                if (m_collider.hideFlags == HideFlags.None)
                {
                    m_collider.hideFlags = HideFlags.HideInInspector;
                }

                UpdateCollider();
            }
        }

        private void OnDisable()
        {
            if(m_collider != null)
            {
                Destroy(m_collider);
                m_destroyedCollider = m_collider;
                m_collider = null;
            }
        }

        private void UpdateCollider()
        {
            if(m_collider != null)
            {
                m_collider.radius = 0.25f * m_scale;
            }
        }
    }
}

