using System;
using System.Security.Cryptography;
using UnityEngine;

namespace Battlehub.Wireframe
{
    public class InstanceIDToColor : MonoBehaviour
    {
        [SerializeField]
        private Color[] m_colors =
        {
            new Color32(0x99, 0x00, 0x00, 0xFF),
            new Color32(0x93, 0x33, 0x00, 0xFF),
            new Color32(0x99, 0x66, 0x00, 0xFF),
            new Color32(0x66, 0x99, 0x00, 0xFF),
            new Color32(0x19, 0x99, 0x00, 0xFF),
            new Color32(0x00, 0x99, 0x1A, 0xFF),
            new Color32(0x00, 0x99, 0x4D, 0xFF),
            new Color32(0x00, 0x99, 0x7f, 0xFF),
            new Color32(0x00, 0x7f, 0x99, 0xFF),
            new Color32(0x00, 0x66, 0x99, 0xFF),
            new Color32(0x00, 0x33, 0x99, 0xFF),
            new Color32(0x66, 0x00, 0x99, 0xFF),
            new Color32(0x99, 0x00, 0x99, 0xFF),
            new Color32(0x99, 0x00, 0x66, 0xFF),
            new Color32(0x99, 0x00, 0x33, 0xFF),
            new Color32(0x99, 0x00, 0x19, 0xFF)
        };

        private static readonly MD5 s_hashAlgo = MD5.Create();

        public Color Convert(int instanceId)
        {
            instanceId = Mathf.Abs(BitConverter.ToInt32(s_hashAlgo.ComputeHash(BitConverter.GetBytes(instanceId)), 0));
            return m_colors[instanceId % m_colors.Length];
        }

    }

}
