﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Battlehub.RTHandles
{
    public class PositionHandleModel : MonoBehaviour
    {
        [SerializeField]
        private GameObject[] m_models;
        [SerializeField]
        private Transform[] m_armatures;
        [SerializeField]
        private int m_xMatIndex = 0;
        [SerializeField]
        private int m_yMatIndex = 1;
        [SerializeField]
        private int m_zMatIndex = 2;
        [SerializeField]
        private int m_xQMatIndex = 3;
        [SerializeField]
        private int m_yQMatIndex = 4;
        [SerializeField]
        private int m_zQMatIndex = 5;
        [SerializeField]
        private int m_xQuadMatIndex = 6;
        [SerializeField]
        private int m_yQuadMatIndex = 7;
        [SerializeField]
        private int m_zQuadMatIndex = 8;

        [SerializeField]
        private Color m_xColor = Color.red;
        [SerializeField]
        private Color m_yColor = Color.green;
        [SerializeField]
        private Color m_zColor = Color.blue;
        [SerializeField]
        private float m_quadTransparency = 0.5f;
        [SerializeField]
        private Color m_selectionColor = Color.yellow;
        [SerializeField]
        private Color m_higlighColor = new Color(Color.yellow.r / 2, Color.yellow.g / 2, Color.yellow.b / 2, 1.0f);

        private Material[] m_materials;

        private Transform[] m_b0;
        private Transform[] m_b1x;
        private Transform[] m_b2x;
        private Transform[] m_b3x;
        private Transform[] m_bSx;        
        private Transform[] m_b1y;
        private Transform[] m_b2y;
        private Transform[] m_b3y;        
        private Transform[] m_bSy;
        private Transform[] m_b1z;
        private Transform[] m_b2z;
        private Transform[] m_b3z;
        private Transform[] m_bSz;

        private Vector3[] m_defaultArmaturesScale;
        private Vector3[] m_defaultB3XScale;
        private Vector3[] m_defaultB3YScale;
        private Vector3[] m_defaultB3ZScale;

        private const float DefaultRadius = 0.1f;
        private const float DefaultLength = 1.0f;
        private const float DefaultArrowRadius = 0.2f;
        private const float DefaultArrowLength = 0.2f;
        private const float DefaultQuadLength = 0.2f;

        [SerializeField]
        private float m_radius = DefaultRadius;
        [SerializeField]
        private float m_length = DefaultLength;
        [SerializeField]
        private float m_arrowRadius = DefaultArrowRadius;
        [SerializeField]
        private float m_arrowLength = DefaultArrowLength;
        [SerializeField]
        private float m_quadLength = DefaultQuadLength;

        private void UpdateTransforms()
        {
            m_quadLength = Mathf.Abs(m_quadLength);
            m_radius = Mathf.Max(0.01f, m_radius);

            Vector3 right = transform.rotation * Vector3.right;
            Vector3 up = transform.rotation * Vector3.up;
            Vector3 forward = transform.rotation * Vector3.forward;
            Vector3 p = transform.position;
            float scale = m_radius / DefaultRadius;
            float arrowScale = m_arrowLength / DefaultArrowLength / scale;
            float arrowRadiusScale = m_arrowRadius / DefaultArrowRadius / scale;

            for (int i = 0; i < m_models.Length; ++i)
            {
                m_armatures[i].localScale = m_defaultArmaturesScale[i] * scale;

                m_b3x[i].position = p + right * m_length;
                m_b3y[i].position = p + up * m_length;
                m_b3z[i].position = p + forward * m_length;

                m_b2x[i].position = p + right * (m_length - m_arrowLength);
                m_b2y[i].position = p + up * (m_length - m_arrowLength);
                m_b2z[i].position = p + forward * (m_length - m_arrowLength);

                m_b3x[i].localScale = Vector3.right * arrowScale +
                    new Vector3(0, 1, 1) * arrowRadiusScale;
                m_b3y[i].localScale = Vector3.forward * arrowScale +
                    new Vector3(1, 1, 0) * arrowRadiusScale;
                m_b3z[i].localScale = Vector3.up * arrowScale +
                    new Vector3(1, 0, 1) * arrowRadiusScale;

                m_b1x[i].position = p + Mathf.Sign(Vector3.Dot(right, m_b1x[i].position - p)) * right * m_quadLength;
                m_b1y[i].position = p + Mathf.Sign(Vector3.Dot(up, m_b1y[i].position - p)) * up * m_quadLength;
                m_b1z[i].position = p + Mathf.Sign(Vector3.Dot(forward, m_b1z[i].position - p)) * forward * m_quadLength;

                m_bSx[i].position = p + (m_b1y[i].position - p) + (m_b1z[i].position - p);
                m_bSy[i].position = p + (m_b1x[i].position - p) + (m_b1z[i].position - p);
                m_bSz[i].position = p + (m_b1x[i].position - p) + (m_b1y[i].position - p);
            }
        }

        private void Awake()
        {
            m_defaultArmaturesScale = new Vector3[m_armatures.Length];
            m_defaultB3XScale = new Vector3[m_armatures.Length];
            m_defaultB3YScale = new Vector3[m_armatures.Length];
            m_defaultB3ZScale = new Vector3[m_armatures.Length];

            m_b1x = new Transform[m_armatures.Length];
            m_b1y = new Transform[m_armatures.Length];
            m_b1z = new Transform[m_armatures.Length];
            m_b2x = new Transform[m_armatures.Length];
            m_b2y = new Transform[m_armatures.Length];
            m_b2z = new Transform[m_armatures.Length];
            m_b3x = new Transform[m_armatures.Length];
            m_b3y = new Transform[m_armatures.Length];
            m_b3z = new Transform[m_armatures.Length];
            m_b0 = new Transform[m_armatures.Length];
            m_bSx = new Transform[m_armatures.Length];
            m_bSy = new Transform[m_armatures.Length];
            m_bSz = new Transform[m_armatures.Length];
            for (int i = 0; i < m_armatures.Length; ++i)
            {
                m_b1x[i] = m_armatures[i].GetChild(0);
                m_b1y[i] = m_armatures[i].GetChild(1);
                m_b1z[i] = m_armatures[i].GetChild(2);
                m_b2x[i] = m_armatures[i].GetChild(3);
                m_b2y[i] = m_armatures[i].GetChild(4);
                m_b2z[i] = m_armatures[i].GetChild(5);
                m_b3x[i] = m_armatures[i].GetChild(6);
                m_b3y[i] = m_armatures[i].GetChild(7);
                m_b3z[i] = m_armatures[i].GetChild(8);
                m_b0[i] = m_armatures[i].GetChild(9);
                m_bSx[i] = m_armatures[i].GetChild(10);
                m_bSy[i] = m_armatures[i].GetChild(11);
                m_bSz[i] = m_armatures[i].GetChild(12);

                m_defaultArmaturesScale[i] = m_armatures[i].localScale;
                m_defaultB3XScale[i] = transform.TransformVector(m_b3x[i].localScale);
                m_defaultB3YScale[i] = transform.TransformVector(m_b3y[i].localScale);
                m_defaultB3ZScale[i] = transform.TransformVector(m_b3z[i].localScale);
            }

            m_materials = m_models[0].GetComponent<Renderer>().materials;
            m_materials[m_xMatIndex].color = m_xColor;
            m_materials[m_yMatIndex].color = m_yColor;
            m_materials[m_zMatIndex].color = m_zColor;
            m_materials[m_xQMatIndex].color = m_xColor;
            m_materials[m_yQMatIndex].color = m_yColor;
            m_materials[m_zQMatIndex].color = m_zColor;

            Color xQuadColor = m_xColor; xQuadColor.a = m_quadTransparency;
            m_materials[m_xQuadMatIndex].color =  xQuadColor;

            Color yQuadColor = m_yColor; yQuadColor.a = m_quadTransparency;
            m_materials[m_yQuadMatIndex].color = yQuadColor;

            Color zQuadColor = m_zColor; zQuadColor.a = m_quadTransparency;
            m_materials[m_zQuadMatIndex].color = zQuadColor;

            for (int i = 0; i <  m_models.Length; ++i)
            {
                Renderer renderer = m_models[i].GetComponent<Renderer>();
                renderer.sharedMaterials = m_materials;
            }
        }

        private void Start()
        {
            UpdateTransforms();
        }

        public void Select(bool x, bool y, bool z)
        {

        }

        public void SetCameraPosition(Vector3 pos)
        {
            Vector3 toCam = (pos - transform.position).normalized;
            toCam = transform.InverseTransformDirection(toCam);
            float[] dots = new[]
            {
                Vector3.Dot(new Vector3( 1,  1,  1).normalized, toCam),
                Vector3.Dot(new Vector3(-1,  1,  1).normalized, toCam),
                Vector3.Dot(new Vector3(-1, -1,  1).normalized, toCam),
                Vector3.Dot(new Vector3( 1, -1,  1).normalized, toCam),
                Vector3.Dot(new Vector3( 1,  1, -1).normalized, toCam),
                Vector3.Dot(new Vector3(-1,  1, -1).normalized, toCam),
                Vector3.Dot(new Vector3(-1, -1, -1).normalized, toCam),
                Vector3.Dot(new Vector3( 1, -1, -1).normalized, toCam),
            };

            float maxDot = float.MinValue;
            int maxIndex = -1;
            for(int i = 0; i < dots.Length; ++i)
            {
                if(dots[i] > maxDot)
                {
                    maxDot = dots[i];
                    maxIndex = i;
                }
            }

            for(int i = 0; i < m_models.Length; ++i)
            {
                if(i != maxIndex)
                {
                    m_models[i].SetActive(false);
                }
            }

            if(maxIndex >= 0)
            {
                m_models[maxIndex].SetActive(true);
            }
        }

#if DEBUG
        private float m_prevRadius;
        private float m_prevLength;
        private float m_prevArrowRadius;
        private float m_prevArrowLength;
        private float m_prevQuadLength;

        [SerializeField]
        private Transform m_camera;
        private Vector3 m_prevCameraPosition = new Vector3(float.MinValue, float.MinValue, float.MinValue);
        private Vector3 m_prevPosition;
        private Quaternion m_prevRotation;

        private void Update()
        {

            if(m_prevCameraPosition != m_camera.transform.position || m_prevPosition != transform.position || m_prevRotation != transform.rotation)
            {
                m_prevRotation = transform.rotation;
                m_prevPosition = transform.position;
                m_prevCameraPosition = m_camera.transform.position;
                SetCameraPosition(m_prevCameraPosition);
            }

            if (m_prevRadius != m_radius || m_prevLength != m_length || m_prevArrowRadius != m_arrowRadius || m_prevArrowLength != m_arrowLength || m_prevQuadLength != m_quadLength)
            {
                m_prevRadius = m_radius;
                m_prevLength = m_length;
                m_prevArrowRadius = m_arrowRadius;
                m_prevArrowLength = m_arrowLength;
                m_prevQuadLength = m_quadLength;

                UpdateTransforms();
            }
        }
#endif

    }
}


