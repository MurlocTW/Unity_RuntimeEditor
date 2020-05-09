#if UNITY_EDITOR
using Battlehub.RTSL;
using UnityEditor;

namespace Battlehub.RTBuilder
{
    public static class RegisterTemplates 
    {
        [InitializeOnLoadMethod]
        public static void Register()
        {
            RTSLPath.ClassMappingsTemplatePath.Add("Assets/" + BHRoot.Path + "/RTExtensions/RTBuilder/RTSL/Mappings/Editor/RTBuilder.ClassMappingsTemplate.prefab");
            RTSLPath.SurrogatesMappingsTemplatePath.Add("Assets/" + BHRoot.Path + "/RTExtensions/RTBuilder/RTSL/Mappings/Editor/RTBuilder.SurrogatesMappingsTemplate.prefab");
        }
    }
}
#endif
