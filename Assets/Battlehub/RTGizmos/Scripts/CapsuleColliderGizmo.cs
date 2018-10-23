﻿using System;
using Battlehub.RTCommon;
using Battlehub.Utils;
using UnityEngine;

namespace Battlehub.RTGizmos
{
    public class CapsuleColliderGizmo : CapsuleGizmo
    {
        [SerializeField]
        private CapsuleCollider m_collider;

        protected override Vector3 Center
        {
            get
            {
                if(m_collider == null)
                {
                    return Vector3.zero;
                }
                return m_collider.center;
            }
            set
            {
                if(m_collider != null)
                {
                    m_collider.center = value;
                }   
            }
        }

        protected override float Radius
        {
            get
            {
                if(m_collider == null)
                {
                    return 0;
                }

                return m_collider.radius;
            }
            set
            {
                if(m_collider != null)
                {
                    m_collider.radius = value;
                }
            }
        }

        protected override float Height
        {
            get
            {
                if (m_collider == null)
                {
                    return 0;
                }

                return m_collider.height;
            }
            set
            {
                if(m_collider != null)
                {
                    m_collider.height = value;
                }
            }
        }

        protected override int Direction
        {
            get
            {
                if(m_collider == null)
                {
                    return 0;
                }
                return m_collider.direction;
            }

            set
            {
                if(m_collider != null)
                {
                    m_collider.direction = value;
                }
            }
        }

        protected override void AwakeOverride()
        {
            if (m_collider == null)
            {
                m_collider = GetComponent<CapsuleCollider>();
            }

            if (m_collider == null)
            {
                Debug.LogError("Set Collider");
            }

            base.AwakeOverride();
        }

        protected override void RecordOverride()
        {
            base.RecordOverride();
            Window.Editor.Undo.RecordValue(m_collider, Strong.PropertyInfo((CapsuleCollider x) => x.center, "center"));
            Window.Editor.Undo.RecordValue(m_collider, Strong.PropertyInfo((CapsuleCollider x) => x.height, "height"));
            Window.Editor.Undo.RecordValue(m_collider, Strong.PropertyInfo((CapsuleCollider x) => x.direction, "direction"));
        }
    }
}

