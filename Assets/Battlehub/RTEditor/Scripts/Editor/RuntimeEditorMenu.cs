#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.EventSystems;

using UnityObject = UnityEngine.Object;
using Battlehub.RTSL;

namespace Battlehub.RTEditor
{
    public static class RTEditorMenu
    {
        public static string Root
        {
            get { return BHRoot.Path + @"/RTEditor/"; }
        }
            
        [MenuItem("Tools/Runtime Editor/Create")]
        public static void CreateRuntimeEditor()
        {
            Undo.RegisterCreatedObjectUndo(InstantiateRuntimeEditor(), "Battlehub.RTEditor.Create");
            EventSystem eventSystem = UnityObject.FindObjectOfType<EventSystem>();
            if (!eventSystem)
            {
                GameObject es = new GameObject();
                eventSystem = es.AddComponent<EventSystem>();
                es.AddComponent<StandaloneInputModule>();
                es.name = "EventSystem";
            }

            eventSystem.gameObject.AddComponent<RTSLIgnore>();

            GameObject camera = GameObject.Find("Main Camera");
            if(camera != null)
            {
                if(camera.GetComponent<GameViewCamera>() == null)
                {
                    if(EditorUtility.DisplayDialog("Main Camera setup.", "Do you want to add Game View Camera script to Main Camera and render it to Runtime Editors's Game view?", "Yes", "No"))
                    {
                        Undo.AddComponent<GameViewCamera>(camera.gameObject);
                    }
                }
            }
        }

        public static GameObject InstantiateRuntimeEditor()
        {
            return InstantiatePrefab("RuntimeEditor.prefab");
        }

        public static GameObject InstantiatePrefab(string name)
        {
            UnityObject prefab = AssetDatabase.LoadAssetAtPath("Assets/" + Root + "Prefabs/" + name, typeof(GameObject));
            return (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        } 
    }
}
#endif