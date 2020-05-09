#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;

using UnityObject = UnityEngine.Object;

namespace Battlehub.RTExtensions
{
    public static class RTExtensionsMenu
    {
        public static string Root
        {
            get { return BHRoot.Path + @"/RTExtensions/"; }
        }

        [MenuItem("Tools/Runtime Editor/Create Extensions")]
        public static void CreateRuntimeEditor()
        {
            GameObject editorExtensions = InstantiateEditorExtensions();
            Undo.RegisterCreatedObjectUndo(editorExtensions, "Battlehub.RTExtensions.Create");
        }

        public static GameObject InstantiateEditorExtensions()
        {
            return InstantiatePrefab("EditorExtensions.prefab");
        }

        public static GameObject InstantiatePrefab(string name)
        {
            UnityObject prefab = AssetDatabase.LoadAssetAtPath("Assets/" + Root + "/" + name, typeof(GameObject));
            return (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        } 
    }
}
#endif