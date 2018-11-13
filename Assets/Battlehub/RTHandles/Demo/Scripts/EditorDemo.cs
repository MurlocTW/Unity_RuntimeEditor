﻿
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

using Battlehub.Utils;
using Battlehub.RTSaveLoad;
using Battlehub.RTCommon;

namespace Battlehub.RTHandles
{
   // [RequireComponent(typeof(PersistentIgnore))]
    [DisallowMultipleComponent]
    public class EditorDemo : RTEBase
    {
        [SerializeField]
        private string SaveFileName = "RTHandlesEditorDemo";
        private bool m_saveFileExists;

        public GameObject[] Prefabs;
        public GameObject PrefabsPanel;
        public GameObject PrefabPresenter;
        public GameObject GamePrefab;
        public RuntimeSelectionComponent SelectionController;

        public KeyCode EditorModifierKey = KeyCode.LeftShift;
        public KeyCode RuntimeModifierKey = KeyCode.LeftControl;
        public KeyCode ModifierKey
        {
            get
            {
                #if UNITY_EDITOR
                return EditorModifierKey;
                #else
                return RuntimeModifierKey;
                #endif
            }
        }
        public KeyCode SnapToGridKey = KeyCode.S;
        public KeyCode DuplicateKey = KeyCode.D;
        public KeyCode DeleteKey = KeyCode.Delete;
        public KeyCode EnterPlayModeKey = KeyCode.P;
        public KeyCode FocusKey = KeyCode.F;
        public KeyCode LeftKey = KeyCode.LeftArrow;
        public KeyCode RightKey = KeyCode.RightArrow;
        public KeyCode FwdKey = KeyCode.UpArrow;
        public KeyCode BwdKey = KeyCode.DownArrow;
        public KeyCode UpKey = KeyCode.PageUp;
        public KeyCode DownKey = KeyCode.PageDown;
        public float PanSpeed = 10.0f;
        public Texture2D PanTexture;
        private Vector3 m_panOffset;
        private bool m_pan;
        private Plane m_dragPlane;
        private Vector3 m_prevMouse;
        
        private Vector3 m_playerCameraPostion;
        private Quaternion m_playerCameraRotation;
        public Camera PlayerCamera;
        public Camera EditorCamera;
        public RuntimeGrid Grid;
        public Button ProjectionButton;
        public Button PlayButton;
        public Button HintButton;
        public Button StopButton;
        public Button SaveButton;
        public Button LoadButton;
        public Button UndoButton;
        public Button RedoButton;
        public Button ResetButton;
        public GameObject UI;
        public GameObject TransformPanel;
        public GameObject BottomPanel;
        public Toggle TogAutoFocus;
        public Toggle TogUnitSnap;
        public Toggle TogBoundingBoxSnap;
        public Toggle TogEnableCharacters;
        public Toggle TogGrid;
        public Toggle TogShowGizmos;
        public Toggle TogPivotRotation;
        public Text TxtCurrentControl;
        public GameObject ConfirmationSave;
        public GameObject ConfirmationLoad;
        
        private GameObject m_game;

        private ISceneManager m_sceneManager;

        private void OnAwaked(ExposeToEditor obj)
        {
            if (IsInPlayMode)
            {
                if (obj.ObjectType == ExposeToEditorObjectType.Undefined)
                {
                    obj.ObjectType = ExposeToEditorObjectType.PlayMode;
                }
            }
            else
            {
                if (obj.ObjectType == ExposeToEditorObjectType.Undefined)
                {
                    obj.ObjectType = ExposeToEditorObjectType.EditorMode;
                }
            }
        }
        private void OnDestroyed(ExposeToEditor obj)
        {
        }

        private bool IsInPlayMode
        {
            get { return m_game != null; }
        }

        private Vector3 m_pivot;
        public Vector3 Pivot
        {
            get { return m_pivot; }
        }

        public float EditorCamDistance = 10;
        private IAnimationInfo[] m_focusAnimations = new IAnimationInfo[3];
        private Transform m_autoFocusTranform;
        public bool AutoFocus
        {
            get { return Tools.AutoFocus; }
            set { Tools.AutoFocus = value; }
        }

        public bool AutoUnitSnapping
        {
            get { return Tools.UnitSnapping; }
            set { Tools.UnitSnapping = value; }
        }

        public bool BoundingBoxSnapping
        {
            get { return Tools.IsSnapping; }
            set { Tools.IsSnapping = value;  }
        }

        private bool m_enableCharacters;
        public bool EnableCharacters
        {
            get { return m_enableCharacters; }
            set
            {
                if (m_enableCharacters == value)
                {
                    return;
                }

                m_enableCharacters = value;
                ForEachSelectedObject(go =>
                {
                    ExposeToEditor exposeObj = go.GetComponent<ExposeToEditor>();
                    if (exposeObj)
                    {
                        exposeObj.Unselected.Invoke(exposeObj);
                        exposeObj.Selected.Invoke(exposeObj);
                    }
                });
            }
        }

        public bool ShowSelectionGizmos
        {
            get { return Tools.ShowSelectionGizmos; }
            set
            {
                Tools.ShowSelectionGizmos = value;
            }
        }

        public bool IsGlobalPivotRotation
        {
            get { return Tools.PivotRotation == RuntimePivotRotation.Global; }
            set
            {
                if (value)
                {
                    Tools.PivotRotation = RuntimePivotRotation.Global;
                }
                else
                {
                    Tools.PivotRotation = RuntimePivotRotation.Local;
                }
            }
        }
        protected override void Awake()
        {
            base.Awake();

            IOC.Register<IRTE>(this);
            IOC.RegisterFallback(this);
            
            ExposeToEditor[] editorObjects = ExposeToEditor.FindAll(this, ExposeToEditorObjectType.Undefined, false).Select(go => go.GetComponent<ExposeToEditor>()).ToArray();
            for (int i = 0; i < editorObjects.Length; ++i)
            {
                editorObjects[i].ObjectType = ExposeToEditorObjectType.EditorMode;
            }

            Tools.SnappingMode = SnappingMode.BoundingBox;
            IsOpened = !IsInPlayMode;
            PlaymodeStateChanged += OnPlaymodeStateChanged;
            IsOpenedChanged += OnIsOpenedChanged;
            Selection.SelectionChanged += OnRuntimeSelectionChanged;
            Tools.ToolChanged += OnRuntimeToolChanged;
            Tools.PivotRotationChanged += OnPivotRotationChanged;
            Undo.UndoCompleted += OnUndoCompleted;
            Undo.RedoCompleted += OnRedoCompleted;
            Undo.StateChanged += OnUndoRedoStateChanged;

            TransformPanel.SetActive(Selection.activeTransform != null);
            if (Prefabs != null && PrefabsPanel != null && PrefabPresenter != null)
            {
                Prefabs = Prefabs.Where(p => p != null).ToArray();
                for (int i = 0; i < Prefabs.Length; ++i)
                {
                    GameObject presenter = Instantiate(PrefabPresenter);
                    presenter.transform.SetParent(PrefabsPanel.transform);
                    presenter.transform.position = Vector3.zero;
                    presenter.transform.localScale = Vector3.one;

                    InstantiatePrefab instantiatePrefab = presenter.GetComponentInChildren<InstantiatePrefab>();
                    if (instantiatePrefab != null)
                    {
                        instantiatePrefab.Prefab = Prefabs[i];
                    }
                    TakeSnapshot takeSnapshot = presenter.GetComponentInChildren<TakeSnapshot>();
                    if (takeSnapshot != null)
                    {
                        takeSnapshot.TargetPrefab = Prefabs[i];
                    }
                }
            }
        }

        protected override void Start()
        {
            base.Start();
        
            Vector3 toCam = new Vector3(1, 1, 1);

            bool useSceneViewInput = SelectionController is RuntimeSceneComponent;
            if (!useSceneViewInput)
            {
                EditorCamera.transform.position = m_pivot + toCam * EditorCamDistance;
                EditorCamera.transform.LookAt(m_pivot);
            }

            UpdateUIState(IsInPlayMode);
            AutoFocus = TogAutoFocus.isOn;
            AutoUnitSnapping = TogUnitSnap.isOn;
            BoundingBoxSnapping = TogBoundingBoxSnap.isOn;
            ShowSelectionGizmos = TogShowGizmos.isOn;
            EnableCharacters = TogEnableCharacters.isOn;

            Object.Awaked += OnAwaked;
            Object.Destroyed += OnDestroyed;

            m_sceneManager = Dependencies.SceneManager;
            if (m_sceneManager != null)
            {
                m_sceneManager.ActiveScene.Name = SaveFileName;
                m_sceneManager.Exists(m_sceneManager.ActiveScene, exists =>
                {
                    m_saveFileExists = exists;
                    LoadButton.interactable = exists;
                });
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            IOC.Unregister<IRTE>(this);
            IOC.UnregisterFallback(this);

            PlaymodeStateChanged -= OnPlaymodeStateChanged;
            IsOpenedChanged -= OnIsOpenedChanged;
            Selection.SelectionChanged -= OnRuntimeSelectionChanged;
            Tools.ToolChanged -= OnRuntimeToolChanged;
            Tools.PivotRotationChanged -= OnPivotRotationChanged;
            Undo.RedoCompleted -= OnUndoCompleted;
            Undo.RedoCompleted -= OnRedoCompleted;
            Undo.StateChanged -= OnUndoRedoStateChanged;
            Object.Awaked -= OnAwaked;
            Object.Destroyed -= OnDestroyed;
        }

        private void Update()
        {
            if (Input.GetKeyDown(EnterPlayModeKey))
            {
                if (Input.GetKey(ModifierKey))
                {
                    TogglePlayMode();
                }
            }

            if (IsInPlayMode)
            {
                return;
            }

            if(BoundingBoxSnapping != TogBoundingBoxSnap.isOn)
            {
                TogBoundingBoxSnap.isOn = BoundingBoxSnapping;
            }

            if (Input.GetKeyDown(DuplicateKey))
            {
                if (Input.GetKey(ModifierKey))
                {
                    Duplicate();
                }
            }
            else if (Input.GetKeyDown(SnapToGridKey))
            {
                if (Input.GetKey(ModifierKey))
                {
                    SnapToGrid();
                }
            }
            else if (Input.GetKeyDown(DeleteKey))
            {
                Delete();
            }
          
            bool useSceneViewInput = SelectionController is RuntimeSceneComponent;
            if (!useSceneViewInput)
            {
                float wheel = Input.GetAxis(InputAxis.Z); 
                if (wheel != 0.0f)
                {
#if UNITY_WEBGL
                    wheel *= 0.1f;    
#endif
                    EditorCamera.orthographicSize -= wheel * EditorCamera.orthographicSize;
                    if (EditorCamera.orthographicSize < 0.2f)
                    {
                        EditorCamera.orthographicSize = 0.2f;
                    }
                    else if (EditorCamera.orthographicSize > 15)
                    {
                        EditorCamera.orthographicSize = 15;
                    }
                }

#if UNITY_WEBGL
            if (m_editor.Input.GetMouseButtonDown(1))
#else
                if (Input.GetPointerDown(1) || Input.GetPointerDown(2))
#endif
                {
                    m_dragPlane = new Plane(Vector3.up, m_pivot);
                    m_pan = GetPointOnDragPlane(Input.GetPointerXY(0), out m_prevMouse);
                    m_prevMouse = Input.GetPointerXY(0);
                    CursorHelper.SetCursor(this, PanTexture, Vector2.zero, CursorMode.Auto);

                }
#if UNITY_WEBGL
            else if (InputController._GetMouseButton(1))
#else
                else if (Input.GetPointer(1) || Input.GetPointer(2))
#endif
                {
                    if (m_pan)
                    {
                        Vector3 pointOnDragPlane;
                        Vector3 prevPointOnDragPlane;
                        if (GetPointOnDragPlane(Input.GetPointerXY(0), out pointOnDragPlane) &&
                            GetPointOnDragPlane(m_prevMouse, out prevPointOnDragPlane))
                        {
                            Vector3 dragOffset = (pointOnDragPlane - prevPointOnDragPlane);
                            m_prevMouse = Input.GetPointerXY(0);
                            m_panOffset -= dragOffset;
                            EditorCamera.transform.position -= dragOffset;
                        }
                    }

                }
#if UNITY_WEBGL
                else if (Input.GetMouseButtonUp(1))
#else
                else if (Input.GetPointerUp(1) || Input.GetPointerUp(2))
#endif
                {
                    m_pan = false;
                    CursorHelper.ResetCursor(this);
                }

                if (Input.GetKey(UpKey))
                {
                    Vector3 position = EditorCamera.transform.position;
                    position.y += PanSpeed * Time.deltaTime;
                    m_panOffset.y += PanSpeed * Time.deltaTime;
                    EditorCamera.transform.position = position;
                }
                if (Input.GetKey(DownKey))
                {
                    Vector3 position = EditorCamera.transform.position;
                    position.y -= PanSpeed * Time.deltaTime;
                    m_panOffset.y -= PanSpeed * Time.deltaTime;
                    EditorCamera.transform.position = position;
                }
                if (Input.GetKey(LeftKey))
                {
                    MoveMinZ();
                    MovePlusX();
                }
                if (Input.GetKey(RightKey))
                {
                    MovePlusZ();
                    MoveMinX();
                }
                if (Input.GetKey(FwdKey))
                {
                    MoveMinX();
                    MoveMinZ();
                }
                if (Input.GetKey(BwdKey))
                {
                    MovePlusX();
                    MovePlusZ();
                }

                if (Input.GetKeyDown(FocusKey))
                {
                    Focus();
                }
                else if (AutoFocus)
                {
                    do
                    {
                        if (Tools.ActiveTool != null)
                        {
                            break;
                        }

                        if (m_autoFocusTranform == null)
                        {
                            break;
                        }

                        if (m_autoFocusTranform.position == m_pivot)
                        {
                            break;
                        }

                        if (m_focusAnimations[0] == null || m_focusAnimations[0].InProgress)
                        {
                            break;
                        }

                        Vector3 offset = (m_autoFocusTranform.position - m_pivot) - m_panOffset;
                        EditorCamera.transform.position += offset;
                        m_pivot += offset;
                        m_panOffset = Vector3.zero;
                    }
                    while (false);
                }
            }

            if (Selection.activeTransform != null)
            {
                Vector3 offset = Grid.GridOffset;
                offset.y = Selection.activeTransform.position.y;
                Grid.GridOffset = offset;
            }
        }

        private bool GetPointOnDragPlane(Vector3 mouse, out Vector3 point)
        {
            
            Ray ray = EditorCamera.ScreenPointToRay(mouse);
            float distance;
            if(m_dragPlane.Raycast(ray, out distance))
            {
                point = ray.GetPoint(distance);
                return true;
            }

            point = Vector3.zero;
            return false;
        }

        public void MovePlusX()
        {
            Vector3 position = EditorCamera.transform.position;
            position.x += PanSpeed * Time.deltaTime;
            m_panOffset.x += PanSpeed * Time.deltaTime;
            EditorCamera.transform.position = position;
        }

        public void MoveMinX()
        {
            Vector3 position = EditorCamera.transform.position;
            position.x -= PanSpeed * Time.deltaTime;
            m_panOffset.x -= PanSpeed * Time.deltaTime;
            EditorCamera.transform.position = position;
        }

        public void MovePlusZ()
        {
            Vector3 position = EditorCamera.transform.position;
            position.z += PanSpeed * Time.deltaTime;
            m_panOffset.z += PanSpeed * Time.deltaTime;
            EditorCamera.transform.position = position;
        }

        public void MoveMinZ()
        {
            Vector3 position = EditorCamera.transform.position;
            position.z -= PanSpeed * Time.deltaTime;
            m_panOffset.z -= PanSpeed * Time.deltaTime;
            EditorCamera.transform.position = position;
        }

        public void Duplicate()
        {
            GameObject[] selection = Selection.gameObjects;
            if(selection == null)
            {
                return;
            }

            Undo.BeginRecord();
            for (int i = 0; i < selection.Length; ++i)
            {
                GameObject selectedObj = selection[i];
                if (selectedObj != null)
                {
                    Transform p = selectedObj.transform.parent;
                    GameObject copy = Instantiate(selectedObj, selectedObj.transform.position, selectedObj.transform.rotation);
                    copy.transform.SetParent(p, true);

                    selection[i] = copy;
                    Undo.BeginRegisterCreateObject(copy);
                }
            }
            Undo.RecordSelection();
            Undo.EndRecord();

            bool isEnabled = Undo.Enabled;
            Undo.Enabled = false;
            Selection.objects = selection;
            Undo.Enabled = isEnabled;

            Undo.BeginRecord();
           
            for (int i = 0; i < selection.Length; ++i)
            {
                GameObject selectedObj = selection[i];
                if (selectedObj != null)
                {
                    Undo.RegisterCreatedObject(selectedObj);
                }
            }
            Undo.RecordSelection();
            Undo.EndRecord();
        }

        public void Delete()
        {
            GameObject[] selection = Selection.gameObjects;
            if (selection == null)
            {
                return;
            }

            Undo.BeginRecord();
            for (int i = 0; i < selection.Length; ++i)
            {
                GameObject selectedObj = selection[i];
                if (selectedObj != null)
                {
                    Undo.BeginDestroyObject(selectedObj);
                }
            }
            Undo.RecordSelection();
            Undo.EndRecord();

            bool isEnabled = Undo.Enabled;
            Undo.Enabled = false;
            Selection.objects = null;
            Undo.Enabled = isEnabled;

            Undo.BeginRecord();

            for (int i = 0; i < selection.Length; ++i)
            {
                GameObject selectedObj = selection[i];
                if (selectedObj != null)
                {
                    Undo.DestroyObject(selectedObj);
                }
            }
            Undo.RecordSelection();
            Undo.EndRecord();
        }

        public void TogglePlayMode()
        {
            IsPlaying = !IsPlaying;
        }


        private void OnIsOpenedChanged()
        {
            IsPlaying = !IsOpened;
        }

        private void OnPlaymodeStateChanged()
        {
            UpdateUIState(m_game == null);

            IsOpened = !IsPlaying;
            if (m_game == null)
            {
                m_game = Instantiate(GamePrefab);
               // UnityEditor.EditorApplication.isPaused = true;
            }
            else
            {
                DestroyImmediate(m_game);
                m_game = null;
            }

            IsOpened = !IsInPlayMode;

            if(IsInPlayMode)
            {
                Selection.objects = null;
                Undo.Purge();

                m_playerCameraPostion = PlayerCamera.transform.position;
                m_playerCameraRotation = PlayerCamera.transform.rotation;
            }
            else
            {
                PlayerCamera.transform.position = m_playerCameraPostion;
                PlayerCamera.transform.rotation = m_playerCameraRotation;
            }

            SaveButton.interactable = false;
        }
 
        public void Focus()
        {
            RuntimeSceneComponent sceneView = SelectionController as RuntimeSceneComponent;
            if(sceneView != null)
            {
                sceneView.Focus();
                return;
            }

            if (Selection.activeTransform == null)
            {
                return;
            }
            
            m_autoFocusTranform = Selection.activeTransform;

            Vector3 offset = Selection.activeTransform.position - m_pivot - m_panOffset;
            const float duration = 0.1f;
            Run.Instance.Remove(m_focusAnimations[0]);
            Run.Instance.Remove(m_focusAnimations[1]);
            Run.Instance.Remove(m_focusAnimations[2]);

            m_focusAnimations[0] = new Vector3AnimationInfo(
                EditorCamera.transform.position,
                EditorCamera.transform.position + offset, duration, Vector3AnimationInfo.EaseOutCubic,
                (target, value, t, completed) =>
                {
                    EditorCamera.transform.position = value;
                });
            m_focusAnimations[1] = new Vector3AnimationInfo(
                m_pivot,
                Selection.activeTransform.position, duration, Vector3AnimationInfo.EaseOutCubic,
                (target, value, t, completed) =>
                {
                    m_pivot = value;
                });
            m_focusAnimations[2] = new Vector3AnimationInfo(
                m_panOffset,
                Vector3.zero, duration, Vector3AnimationInfo.EaseOutCubic,
                (target, value, t, completed) =>
                {
                    m_panOffset = value;
                });

            Run.Instance.Animation(m_focusAnimations[0]);
            Run.Instance.Animation(m_focusAnimations[1]);
            Run.Instance.Animation(m_focusAnimations[2]);
        }

        private void OnRuntimeSelectionChanged(Object[] unselectedObjects)
        {
            TransformPanel.SetActive(Selection.activeTransform != null);
            TogPivotRotation.isOn = IsGlobalPivotRotation;
        }

        private void OnPivotRotationChanged()
        {
            TogPivotRotation.isOn = IsGlobalPivotRotation;
        }

        private void OnRuntimeToolChanged()
        {
            if (Tools.Current == RuntimeTool.None || Tools.Current == RuntimeTool.View)
            {
                TxtCurrentControl.text = "none";
                ResetButton.gameObject.SetActive(false);
            }

            else if (Tools.Current == RuntimeTool.Move)
            {
                TxtCurrentControl.text = "move";
                ResetButton.gameObject.SetActive(true);
            }

            else if (Tools.Current == RuntimeTool.Rotate)
            {
                TxtCurrentControl.text = "rotate";
                ResetButton.gameObject.SetActive(true);
            }

            else if (Tools.Current == RuntimeTool.Scale)
            {
                TxtCurrentControl.text = "scale";
                ResetButton.gameObject.SetActive(true);
            }
        }

        public void SwitchControl()
        {
            if(Tools.Current == RuntimeTool.None || Tools.Current == RuntimeTool.View)
            {
                Tools.Current = RuntimeTool.Move;
                TxtCurrentControl.text = "move";
            }

            else if (Tools.Current == RuntimeTool.Move)
            {
                Tools.Current = RuntimeTool.Rotate;
                TxtCurrentControl.text = "rotate";
            }

            else if (Tools.Current == RuntimeTool.Rotate)
            {
                Tools.Current = RuntimeTool.Scale;
                TxtCurrentControl.text = "scale";
            }

            else if (Tools.Current == RuntimeTool.Scale)
            {
                Tools.Current = RuntimeTool.View;
                TxtCurrentControl.text = "none";
            }
        }

        public void ResetPosition()
        {
            if(Tools.Current == RuntimeTool.Move)
            {
                ForEachSelectedObject(go => go.transform.position = Vector3.zero);
            }
            else if(Tools.Current == RuntimeTool.Rotate)
            {
                ForEachSelectedObject(go => go.transform.rotation = Quaternion.identity);
            }
            else if(Tools.Current == RuntimeTool.Scale)
            {
                ForEachSelectedObject(go => go.transform.localScale = Vector3.one);
            }
        }

        public void SnapToGrid()
        {
            GameObject[] selection = Selection.gameObjects;
            if (selection == null || selection.Length == 0)
            {
                return;
            }

            Transform activeTransform = selection[0].transform;
            Vector3 position = activeTransform.position;
            position.x = Mathf.Round(position.x);
            position.y = Mathf.Round(position.y);
            position.z = Mathf.Round(position.z);
            Vector3 offset = position - activeTransform.position;
            
            for(int i = 0; i < selection.Length; ++i)
            {
                selection[i].transform.position += offset;
            }
        }

        private void ForEachSelectedObject(System.Action<GameObject> execute)
        {
            GameObject[] selection = Selection.gameObjects;
            if (selection == null)
            {
                return;
            }

            for (int i = 0; i < selection.Length; ++i)
            {
                GameObject selectedObj = selection[i];
                if (selectedObj != null)
                {
                    execute(selectedObj);
                }
            }
        }

        public void Save()
        {
            if(!ConfirmationSave.activeSelf)
            {
                ConfirmationSave.SetActive(true);
                return;
            }

            Undo.Purge();

            ConfirmationSave.SetActive(false);
            if (m_sceneManager != null)
            {
                m_sceneManager.ActiveScene.Name = SaveFileName;
                m_sceneManager.SaveScene(m_sceneManager.ActiveScene, () =>
                {
                    SaveButton.interactable = false;
                    m_saveFileExists = true;
                    LoadButton.interactable = true;
                });
            }
        }

        public void Load()
        {
            if (!ConfirmationLoad.activeSelf)
            {
                ConfirmationLoad.SetActive(true);
                return;
            }

            Undo.Purge();
            ExposeToEditor[] editorObjects = ExposeToEditor.FindAll(this, ExposeToEditorObjectType.EditorMode).Select(go => go.GetComponent<ExposeToEditor>()).ToArray();
            for (int i = 0; i < editorObjects.Length; ++i)
            {
                ExposeToEditor exposeToEditor = editorObjects[i];
                if (exposeToEditor != null)
                {
                    DestroyImmediate(exposeToEditor.gameObject);
                }
            }

            ConfirmationLoad.SetActive(false);
            if (m_sceneManager != null)
            {
                m_sceneManager.ActiveScene.Name = SaveFileName;
                m_sceneManager.LoadScene(m_sceneManager.ActiveScene, () =>
                {
                    SaveButton.interactable = false;
                });
            }
        }

        public void DoUndo()
        {
            Undo.Undo();
        }

        public void DoRedo()
        {
            Undo.Redo();
        }

        private void OnUndoCompleted()
        {
            UndoButton.interactable = Undo.CanUndo;
            RedoButton.interactable = Undo.CanRedo;

            SaveButton.interactable = m_sceneManager != null;
            LoadButton.interactable = m_sceneManager != null && m_saveFileExists;
        }

        private void OnRedoCompleted()
        {
            UndoButton.interactable = Undo.CanUndo;
            RedoButton.interactable = Undo.CanRedo;

            SaveButton.interactable = m_sceneManager != null;
            LoadButton.interactable = m_sceneManager != null && m_saveFileExists;
        }

        private void OnUndoRedoStateChanged()
        {
            UndoButton.interactable = Undo.CanUndo;
            RedoButton.interactable = Undo.CanRedo;

            SaveButton.interactable = m_sceneManager != null; 
            LoadButton.interactable = m_sceneManager != null && m_saveFileExists;
        }

        private void UpdateUIState(bool isInPlayMode)
        {
            if(ProjectionButton != null)
            {
                ProjectionButton.gameObject.SetActive(!isInPlayMode);
            }
            
            EditorCamera.gameObject.SetActive(!isInPlayMode);
            PlayerCamera.gameObject.SetActive(isInPlayMode);
            SelectionController.gameObject.SetActive(!isInPlayMode);
            PlayButton.gameObject.SetActive(!isInPlayMode);
            
            HintButton.gameObject.SetActive(!isInPlayMode);
            SaveButton.gameObject.SetActive(!isInPlayMode);
            LoadButton.gameObject.SetActive(!isInPlayMode);
            StopButton.gameObject.SetActive(isInPlayMode);
            UndoButton.gameObject.SetActive(!isInPlayMode);
            RedoButton.gameObject.SetActive(!isInPlayMode);
            UI.gameObject.SetActive(!isInPlayMode);
            Grid.gameObject.SetActive(TogGrid.isOn && !isInPlayMode);
            LoadButton.interactable = m_sceneManager != null && m_saveFileExists;

            if (isInPlayMode)
            {
                ActivateWindow(RuntimeWindowType.GameView);
            }
            else
            {
                ActivateWindow(RuntimeWindowType.SceneView);
            }
            
        }
    }
}

