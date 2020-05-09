#if UNITY_EDITOR
using System.IO;
using UnityEditor;
#endif
using UnityEngine;

namespace Battlehub
{
    public class BHRoot : ScriptableObject
    {
#if UNITY_EDITOR
        private static string m_path;
        public static string Path
        {
            get
            {
                if(string.IsNullOrEmpty(m_path))
                {
                    BHRoot script = CreateInstance<BHRoot>();
                    
                    MonoScript monoScript = MonoScript.FromScriptableObject(script);
                    m_path = AssetDatabase.GetAssetPath(monoScript);
                    m_path = System.IO.Path.GetDirectoryName(m_path);
                    m_path = System.IO.Path.GetDirectoryName(m_path);

                    const string assetsFolder = "Assets";
                    int index = m_path.IndexOf(assetsFolder);
                    if(index >= 0)
                    {
                        m_path = m_path.Remove(index, assetsFolder.Length + 1);
                    }

                    DestroyImmediate(script);
                }

                return m_path;
            }
        }
#endif

        public static readonly string[] Assemblies =
        {
            "Assembly-CSharp",
            "Battlehub.RTEditor"
        };
    }

}
