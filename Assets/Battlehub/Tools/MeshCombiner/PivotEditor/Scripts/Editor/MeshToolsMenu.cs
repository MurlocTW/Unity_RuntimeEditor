#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Linq;

namespace Battlehub.MeshTools
{
    public partial class MeshToolsMenu
    {
        private static string root = "Battlehub/Tools/MeshCombiner/PivotEditor/";
        public static GameObject InstantiatePrefab(string name)
        {
            Object prefab = AssetDatabase.LoadAssetAtPath("Assets/" + root + "Prefabs/" + name, typeof(GameObject));
            return (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        }

        [MenuItem("Tools/Mesh/Edit Pivot", validate = true)]
        public static bool CanEditPivot()
        {
            if (Selection.activeTransform == null)
            {
                return false;
            }

            return Selection.activeTransform.GetComponent<MeshFilter>() != null;
        }


        [MenuItem("Tools/Mesh/Edit Pivot")]
        public static void EditPivot()
        {
            if (Selection.activeTransform == null)
            {
                Debug.LogError("Select object");
                return;
            }

            GameObject selectedObj = Selection.activeTransform.gameObject;
            GameObject pivotObj = InstantiatePrefab("Pivot.prefab");
            Pivot pivot = pivotObj.GetComponent<Pivot>();
            pivot.Target = selectedObj.transform;
            pivot.transform.position = selectedObj.transform.position;
            pivotObj.transform.SetParent(selectedObj.transform.parent, true);
            pivotObj.transform.SetSiblingIndex(selectedObj.transform.GetSiblingIndex());

            Undo.RegisterCreatedObjectUndo(pivotObj, "Battlehub.MeshTools.EditPivot");
            Selection.activeObject = pivotObj;

        }

        [MenuItem("Tools/Mesh/Save Mesh", validate = true)]
        public static bool CanSaveMesh()
        {
            GameObject[] selectedObjects = Selection.gameObjects;
            if (selectedObjects.Length == 0)
            {
                return false;
            }

            return selectedObjects.Any(so => so.GetComponent<MeshFilter>() != null);
        }

        [MenuItem("Tools/Mesh/Save Mesh")]
        public static void SaveMesh()
        {
            GameObject[] selectedObjects = Selection.gameObjects;
            MeshUtils.SaveMesh(selectedObjects, "Battlehub/");
        }
    }
}
#endif

