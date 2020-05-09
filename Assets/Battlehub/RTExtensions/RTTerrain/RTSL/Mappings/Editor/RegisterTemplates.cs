#if UNITY_EDITOR
using Battlehub.RTSL;
using UnityEditor;

namespace Battlehub.RTTerrain
{
    public static class RegisterTemplates 
    {
        [InitializeOnLoadMethod]
        public static void Register()
        {
            RTSLPath.ClassMappingsTemplatePath.Add("Assets/" + BHRoot.Path +"/RTExtensions/RTTerrain/RTSL/Mappings/Editor/RTTerrain.ClassMappingsTemplate.prefab");
        }
    }
}
#endif
