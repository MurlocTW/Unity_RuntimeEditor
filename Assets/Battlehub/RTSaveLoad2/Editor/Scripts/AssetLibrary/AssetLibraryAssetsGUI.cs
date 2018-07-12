﻿using Battlehub.RTCommon.EditorTreeView;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using UnityObject = UnityEngine.Object;
namespace Battlehub.RTSaveLoad2
{
    public class AssetLibraryAssetsGUI 
    {
        [NonSerialized]
        private bool m_Initialized;
        [SerializeField]
        private TreeViewState m_TreeViewState; // Serialized in the window layout file so it survives assembly reloading
        [SerializeField]
        private MultiColumnHeaderState m_MultiColumnHeaderState;
        private SearchField m_SearchField;
        private AssetFolderInfo[] m_folders;
        private AssetLibraryAsset m_asset;
        private const string kSessionStateKeyPrefix = "AssetLibraryAssetsTVS";

        public AssetLibraryAssetsGUI()
        {
        }

        internal AssetTreeView TreeView { get; private set; }

        public void SetTreeAsset(AssetLibraryAsset asset)
        {
            m_asset = asset;
            m_Initialized = false;
        }

        public bool IsFolderSelected(AssetFolderInfo folder)
        {
            return m_folders != null && m_folders.Contains(folder);
        }

        public void SetSelectedFolders(AssetFolderInfo[] folders)
        {
            m_folders = folders;
            m_Initialized = false;
        }

        public void InitIfNeeded()
        {
            if (!m_Initialized)
            {
                // Check if it already exists (deserialized from window layout file or scriptable object)
                if (m_TreeViewState == null)
                    m_TreeViewState = new TreeViewState();

                //var jsonState = SessionState.GetString(kSessionStateKeyPrefix + m_asset.GetInstanceID().ToString() + m_folder.id, "");
                //if (!string.IsNullOrEmpty(jsonState))
                //    JsonUtility.FromJsonOverwrite(jsonState, m_TreeViewState);

                bool firstInit = m_MultiColumnHeaderState == null;
                var headerState = AssetTreeView.CreateDefaultMultiColumnHeaderState(0);
                if (MultiColumnHeaderState.CanOverwriteSerializedFields(m_MultiColumnHeaderState, headerState))
                    MultiColumnHeaderState.OverwriteSerializedFields(m_MultiColumnHeaderState, headerState);
                m_MultiColumnHeaderState = headerState;

                var multiColumnHeader = new MultiColumnHeader(headerState);
                if (firstInit)
                    multiColumnHeader.ResizeToFit();

                var treeModel = new TreeModel<AssetInfo>(GetData());

                TreeView = new AssetTreeView(
                    m_TreeViewState,
                    multiColumnHeader,
                    treeModel,
                    ExternalDropInside,
                    ExternalDropOutside);
                // m_TreeView.Reload();

                m_SearchField = new SearchField();
                m_SearchField.downOrUpArrowKeyPressed += TreeView.SetFocusAndEnsureSelectedItem;

                m_Initialized = true;
            }
        }

        public void OnEnable()
        {
            Undo.undoRedoPerformed += OnUndoRedoPerformed;
        }

        public void OnDisable()
        {
            Undo.undoRedoPerformed -= OnUndoRedoPerformed;

            //SessionState.SetString(kSessionStateKeyPrefix + m_asset.GetInstanceID().ToString() + m_folder.id, JsonUtility.ToJson(m_TreeView.state));
        }

        private void OnUndoRedoPerformed()
        {
            if (TreeView != null)
            {
                TreeView.treeModel.SetData(GetData());
                TreeView.Reload();
            }
        }

        private DragAndDropVisualMode CanDrop(TreeViewItem parent, int insertIndex)
        {
            if(m_folders == null || m_folders.Length != 1)
            {
                return DragAndDropVisualMode.None;
            }

            AssetInfo parentAssetInfo = GetAssetInfo(parent);
            if (parentAssetInfo != TreeView.treeModel.root)
            {
                return DragAndDropVisualMode.None;
            }

            if(m_folders == null || m_folders.Length != 1)
            {
                return DragAndDropVisualMode.None;
            }

            bool allFolders = true;
            bool allUnityEditor = DragAndDrop.objectReferences.All(o => o != null && o.GetType().Assembly.FullName.Contains("UnityEditor"));
            foreach (UnityObject dragged_object in DragAndDrop.objectReferences)
            {
                string path = AssetDatabase.GetAssetPath(dragged_object);
                if (!string.IsNullOrEmpty(path) && File.Exists(path))
                {
                    allFolders = false;
                    break;
                }
            }

            if(allFolders || allUnityEditor)
            {
                return DragAndDropVisualMode.None;
            }

            AssetInfo parentAsset;
            if (parent == null)
            {
                parentAsset = TreeView.treeModel.root;
            }
            else
            {
                parentAsset = ((TreeViewItem<AssetInfo>)parent).data;
            }

            if (parentAsset.hasChildren)
            {
                var names = parentAsset.children.Select(c => c.name);
                if (DragAndDrop.objectReferences.Any(item => names.Contains(item.name)))
                {
                    return DragAndDropVisualMode.None;
                }
            }
            return DragAndDropVisualMode.Copy;
        }

        private DragAndDropVisualMode PerformDrop(TreeViewItem parent, int insertIndex)
        {
            DragAndDrop.AcceptDrag();

            List<UnityObject> assets = new List<UnityObject>();
            foreach (UnityObject dragged_object in DragAndDrop.objectReferences)
            {
                string path = AssetDatabase.GetAssetPath(dragged_object);
                if (!string.IsNullOrEmpty(path) && File.Exists(path))
                {
                    assets.Add(dragged_object);
                }
            }


            AssetFolderInfo folder = m_folders[0];
            UnityObject[] assetsArray = assets.ToArray();
            bool moveToNewLocation = MoveToNewLocationDialog(assetsArray, folder);

            AddAssetToFolder(parent, insertIndex, assetsArray, folder, moveToNewLocation);
            return DragAndDropVisualMode.Copy;
        }

        public void AddAssetToFolder(UnityObject[] objects, AssetFolderInfo folder, bool moveToNewLocation)
        {
            AddAssetToFolder(null, -1, objects, folder, moveToNewLocation);
        }

        private void AddAssetToFolder(TreeViewItem parent, int insertIndex, UnityObject[] objects, AssetFolderInfo folder, bool moveToNewLocation)
        {
            for (int i = 0; i < objects.Length; ++i)
            {
                UnityObject obj = objects[i];
                if (obj == null || obj.GetType().Assembly.FullName.Contains("UnityEditor"))
                {
                    continue;
                }

                AssetInfo parentAssetInfo = GetAssetInfo(parent);
                AssetInfo assetInfo;
                AssetFolderInfo existingFolder;
                AssetInfo existingAsset;
                if(m_asset.AssetLibrary.TryGetAssetInfo(obj, out existingFolder, out existingAsset))
                {
                    assetInfo = existingAsset;
                    if(!moveToNewLocation)
                    {
                        continue;
                    }
                }
                else
                {
                    if(m_asset.AssetLibrary.Identity >= AssetLibraryInfo.MAX_ASSETS)
                    {
                        EditorUtility.DisplayDialog("Unable to add asset", string.Format("Max 'Indentity' value reached. 'Identity' ==  {0}", AssetLibraryInfo.MAX_ASSETS), "OK");
                        return;
                    }

                    assetInfo = CreateAsset(obj.name, parentAssetInfo, insertIndex, folder);
                    assetInfo.PersistentID = m_asset.AssetLibrary.Identity;
                    m_asset.AssetLibrary.Identity++;
                    assetInfo.Object = obj; 
                }

                AddAssetToFolder(assetInfo, folder);
            }
        }

        public void AddAssetToFolder(AssetInfo assetInfo, AssetFolderInfo folder)
        {
            if (folder.Assets == null)
            {
                folder.Assets = new List<AssetInfo>();
            }

            if(assetInfo.Folder != null)
            {
                if(!m_folders.Contains(folder))
                {
                    TreeView.treeModel.RemoveElements(new[] { assetInfo.id });
                }
                else if(TreeView.treeModel.Find(assetInfo.id) == null)
                {
                    TreeView.treeModel.AddElement(assetInfo, TreeView.treeModel.root, TreeView.treeModel.root.hasChildren ? TreeView.treeModel.root.children.Count : 0);
                }
                assetInfo.Folder.Assets.Remove(assetInfo);
            }

            assetInfo.Folder = folder;
            folder.Assets.Add(assetInfo);

        }

        private  AssetInfo CreateAsset(string name, TreeElement parent, int insertIndex, AssetFolderInfo folder)
        {
            int depth = parent != null ? parent.depth + 1 : 0;
            int id = TreeView.treeModel.GenerateUniqueID();
            var assetInfo = new AssetInfo(name, depth, id);

            if(IsFolderSelected(folder))
            {
                TreeView.treeModel.AddElement(assetInfo, parent, insertIndex == -1 ?
                    parent.hasChildren ?
                    parent.children.Count
                    : 0
                    : insertIndex);

                // Select newly created element
                TreeView.SetSelection(new[] { id }, TreeViewSelectionOptions.RevealAndFrame);
            }
      
            return assetInfo;
        }

        private AssetInfo GetAssetInfo(TreeViewItem treeViewItem)
        {
            return treeViewItem != null ? ((TreeViewItem<AssetInfo>)treeViewItem).data : TreeView.treeModel.root;
        }

        private DragAndDropVisualMode ExternalDropInside(TreeViewItem parent, int insertIndex, bool performDrop)
        {
            if (performDrop)
            {
                return PerformDrop(parent, insertIndex);
            }
            return CanDrop(parent, insertIndex);
        }

        private DragAndDropVisualMode ExternalDropOutside(TreeViewItem parent, int insertIndex, bool performDrop)
        {
            if (performDrop)
            {
                return PerformDrop(parent, insertIndex);
            }
            return CanDrop(parent, insertIndex);
        }

        private IList<AssetInfo> GetData()
        {
            if (m_folders != null)
            {
                List<AssetInfo> result = new List<AssetInfo>();

                AssetInfo root = new AssetInfo
                {
                    id = 0,
                    name = "Root",
                    IsEnabled = false,
                    depth = -1,
                };

                result.Add(root);
                foreach (AssetInfo assetInfo in m_folders.Where(folder => folder.Assets != null).SelectMany(folder => folder.Assets))
                {
                    assetInfo.parent = root;
                    result.Add(assetInfo);
                }
                return result;
            }

            return new List<AssetInfo>
            {
                new AssetInfo
                {
                    id = 0,
                    name = "Root",
                    IsEnabled = false,
                    depth = -1
                }
            };
        }

        private void SearchBar()
        {
            Rect rect = EditorGUILayout.GetControlRect();
            TreeView.searchString = m_SearchField.OnGUI(rect, TreeView.searchString);
        }

        private void DoTreeView()
        {
            Rect rect = GUILayoutUtility.GetRect(0, 10000, 0, Mathf.Max(10000, TreeView.totalHeight));
            TreeView.OnGUI(rect);
        }

        private void DoCommands()
        {
            Event e = Event.current;
            switch (e.type)
            {
                case EventType.KeyDown:
                    {
                        if (Event.current.keyCode == KeyCode.Delete)
                        {
                            if (TreeView.HasFocus())
                            {
                                RemoveAsset();
                            }
                        }
                        break;
                    }
            }
            EditorGUILayout.BeginHorizontal();

            if(m_folders != null && m_folders.Length == 1)
            {
                if (GUILayout.Button("Pick Asset"))
                {
                    PickObject();
                }
            }

            if (GUILayout.Button("Rename Asset"))
            {
                RenameAsset();
            }
            if (GUILayout.Button("Remove Asset"))
            {
                RemoveAsset();
            }
            EditorGUILayout.EndHorizontal();

            if (Event.current.commandName == "ObjectSelectorUpdated" && EditorGUIUtility.GetObjectPickerControlID() == m_currentPickerWindow)
            {
                m_pickedObject = EditorGUIUtility.GetObjectPickerObject();
            }
            else
            {
                if (Event.current.commandName == "ObjectSelectorClosed" && EditorGUIUtility.GetObjectPickerControlID() == m_currentPickerWindow)
                {
                    m_currentPickerWindow = -1;
                    if (m_pickedObject != null)
                    {
                        if (m_folders[0].Assets == null || !m_folders[0].Assets.Any(a => a.Object == m_pickedObject))
                        {
                            if (m_pickedObject == null || m_pickedObject.GetType().Assembly.FullName.Contains("UnityEditor"))
                            {
                                EditorUtility.DisplayDialog("Unable to add asset",
                                   string.Format("Unable to add asset {0} from assembly {1}", m_pickedObject.GetType().Name, m_pickedObject.GetType().Assembly.GetName()), "OK");
                            }
                            else
                            {
                                bool moveToNewLocation = MoveToNewLocationDialog(new[] { m_pickedObject }, m_folders[0]);
                                AddAssetToFolder(new[] { m_pickedObject }, m_folders[0], moveToNewLocation);
                            }
                        }
                        m_pickedObject = null;
                    }
                }
            }
        }


        private bool MoveToNewLocationDialog(UnityObject[] assets, AssetFolderInfo folder)
        {
            bool moveToNewLocation = true;
            bool moveDialogDisplayed = false;
            foreach (UnityObject asset in assets)
            {
                if (!moveDialogDisplayed)
                {
                    AssetFolderInfo existingFolder;
                    AssetInfo existingAsset;
                    if (m_asset.AssetLibrary.TryGetAssetInfo(asset, out existingFolder, out existingAsset))
                    {
                        if(existingFolder != folder)
                        {
                            moveToNewLocation = EditorUtility.DisplayDialog(
                                                       "Same asset already added",
                                                       "Same asset already added to asset library. Do you want to move it to new location?", "Yes", "No");
                            moveDialogDisplayed = true;
                        }  
                    }
                }
            }

            return moveToNewLocation;
        }

        private UnityObject m_pickedObject;
        private int m_currentPickerWindow;
        private void PickObject()
        {
            m_currentPickerWindow = GUIUtility.GetControlID(FocusType.Passive) + 100;
            EditorGUIUtility.ShowObjectPicker<UnityObject>(null, false, string.Empty, m_currentPickerWindow);
        }

        private void RenameAsset()
        {
            var selection = TreeView.GetSelection();
            if (selection != null && selection.Count > 0)
            {
                TreeView.BeginRename(selection[0]);
            }
        }

        private void RemoveAsset()
        {
            Undo.RecordObject(m_asset, "Remove Asset");
            IList<int> selection = TreeView.GetSelection();
          
            foreach(int selectedId in selection)
            {
                AssetInfo assetInfo = TreeView.treeModel.Find(selectedId);
                if(assetInfo != null)
                {
                    TreeElement parent = assetInfo.parent;

                    int index = parent.children.IndexOf(assetInfo);
                    assetInfo.Folder.Assets.Remove(assetInfo);
                    assetInfo.Folder = null;
                    
                    TreeView.treeModel.RemoveElements(new[] { assetInfo });
                    
                    if(index >= parent.children.Count)
                    {
                        index--;
                    }

                    if(index >= 0)
                    {
                        TreeView.SetSelection(new int[] { parent.children[index].id }, TreeViewSelectionOptions.FireSelectionChanged);
                    }
                    else
                    {
                        TreeView.SetSelection(new int[0], TreeViewSelectionOptions.FireSelectionChanged);
                    }
                    
                }   
            }

            
            TreeView.Reload();
        }

        public void OnGUI()
        {
            EditorGUILayout.BeginVertical();
            InitIfNeeded();
            EditorGUILayout.Space();
            SearchBar();
            EditorGUILayout.Space();
            DoTreeView();
            EditorGUILayout.Space();
            DoCommands();
            EditorGUILayout.EndVertical();
        }
    }
}