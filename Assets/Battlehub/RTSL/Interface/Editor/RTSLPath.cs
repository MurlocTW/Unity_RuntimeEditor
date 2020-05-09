#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEditor;

namespace Battlehub.RTSL
{
    public static class RTSLPath 
    {
        public static string SaveLoadRoot
        {
            get { return @"/" + BHRoot.Path + @"/RTSL"; }
        }
        
        public static string UserRoot
        {
            get
            {
                string userRoot = EditorPrefs.GetString("RTSLDataRoot");
                if (string.IsNullOrEmpty(userRoot))
                {
                    string dll = AssetDatabase.FindAssets(TypeModelDll.Replace(".dll", string.Empty)).FirstOrDefault();
                    if(string.IsNullOrEmpty(dll))
                    {
                        return "/" + BHRoot.Path + "/RTSL_Data";
                    }
                    string path = AssetDatabase.GUIDToAssetPath(dll).Replace(TypeModelDll, "");
                    if(string.IsNullOrEmpty(path))
                    {
                        return "/" + BHRoot.Path + "/RTSL_Data";
                    }
                    int firstIndex = path.IndexOf("/");
                    if(firstIndex < 0)
                    {
                        return "/" + BHRoot.Path + "/RTSL_Data";
                    }

                    return path.Remove(0, firstIndex + 1);
                }
                if(!userRoot.StartsWith("/"))
                {
                    userRoot = "/" + userRoot;
                }
                userRoot = userRoot.TrimEnd(new[] { '/', '\\' });
                return userRoot;
            }
            set
            {
                EditorPrefs.SetString("RTSLDataRoot", value);
            }
        }

        public static string EditorPrefabsPath { get { return SaveLoadRoot + "/Editor/Prefabs"; } }
        public static string UserPrefabsPath { get { return UserRoot + "/Mappings/Editor"; } }
        public static string FilePathStoragePath { get { return "Assets" + UserPrefabsPath + @"/FilePathStorage.prefab"; } }
        public static string ClassMappingsStoragePath { get { return "Assets" + UserPrefabsPath + @"/ClassMappingsStorage.prefab"; } }
        public static string SurrogatesMappingsStoragePath { get { return "Assets" + UserPrefabsPath + @"/SurrogatesMappingsStorage.prefab"; } }
        public static IList<string> ClassMappingsTemplatePath = new List<string>
        {
            "Assets" + EditorPrefabsPath + @"/ClassMappingsTemplate.prefab"
        };
        public static readonly IList<string> SurrogatesMappingsTemplatePath = new List<string>
        {
            "Assets" + EditorPrefabsPath + @"/SurrogatesMappingsTemplate.prefab"
        };

        public const string ScriptsAutoFolder = "Scripts";
        public const string PersistentClassesFolder = "PersistentClasses";
        public const string PersistentCustomImplementationClasessFolder = "CustomImplementation";
        public const string LibrariesFolder = "Libraries";
        public const string TypeModelDll = "RTSLTypeModel.dll";
    }
}
#endif