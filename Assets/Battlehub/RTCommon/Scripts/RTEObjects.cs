﻿using Battlehub.Utils;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

using UnityObject = UnityEngine.Object;
namespace Battlehub.RTCommon
{
    public delegate void ObjectEvent(ExposeToEditor obj);
    public delegate void ObjectParentChangedEvent(ExposeToEditor obj, ExposeToEditor oldValue, ExposeToEditor newValue);
   
    public interface IRTEObjects
    {
        event ObjectEvent Awaked;
        event ObjectEvent Started;
        event ObjectEvent Enabled;
        event ObjectEvent Disabled;
        event ObjectEvent Destroying;
        event ObjectEvent Destroyed;
        event ObjectEvent MarkAsDestroyedChanged;
        event ObjectEvent TransformChanged;
        event ObjectEvent NameChanged;
        event ObjectParentChangedEvent ParentChanged;

        IEnumerable<ExposeToEditor> Get(bool rootsOnly);
    }

    public class RTEObjects : MonoBehaviour, IRTEObjects
    {
        public event ObjectEvent Awaked;
        public event ObjectEvent Started;
        public event ObjectEvent Enabled;
        public event ObjectEvent Disabled;
        public event ObjectEvent Destroying;
        public event ObjectEvent Destroyed;
        public event ObjectEvent MarkAsDestroyedChanged;
        public event ObjectEvent TransformChanged;
        public event ObjectEvent NameChanged;
        public event ObjectParentChangedEvent ParentChanged;

        private IRTE m_editor;

        private ExposeToEditor[] m_enabledObjects;
        private UnityObject[] m_selectedObjects;
        private HashSet<ExposeToEditor> m_editModeCache;
        private HashSet<ExposeToEditor> m_playModeCache;

        public IEnumerable<ExposeToEditor> Get(bool rootsOnly)
        {
            if(rootsOnly)
            {
                if (m_editor.IsPlaying)
                {
                    return m_playModeCache.Where(o => o.Parent == null);
                }
                else
                {
                    return m_editModeCache.Where(o => o.Parent == null);
                }
            }

            if (m_editor.IsPlaying)
            {
                return m_playModeCache;
            }

            return m_editModeCache;
        }


        private void Awake()
        {
            m_editor = IOC.Resolve<IRTE>();
            if(m_editor.IsPlaying || m_editor.IsPlaymodeStateChanging)
            {
                Debug.LogError("Editor should be switched to edit mode");
                return;
            }

     
            List<ExposeToEditor> objects = FindAll();
            for(int i = 0; i < objects.Count; ++i)
            {
                objects[i].Init();
            }
            m_editModeCache = new HashSet<ExposeToEditor>(objects);
            m_playModeCache = null;

            OnIsOpenedChanged();
            m_editor.PlaymodeStateChanging += OnPlaymodeStateChanging;
            m_editor.IsOpenedChanged += OnIsOpenedChanged;

            ExposeToEditor._Awaked += OnAwaked;
            ExposeToEditor._Enabled += OnEnabled;
            ExposeToEditor._Started += OnStarted;
            ExposeToEditor._Disabled += OnDisabled;
            ExposeToEditor._Destroying += OnDestroying;
            ExposeToEditor._Destroyed += OnDestroyed;
            ExposeToEditor._MarkAsDestroyedChanged += OnMarkAsDestroyedChanged;

            ExposeToEditor._TransformChanged += OnTransformChanged;
            ExposeToEditor._NameChanged += OnNameChanged;
            ExposeToEditor._ParentChanged += OnParentChanged;
        }

        private void OnDestroy()
        {
            if(m_editor != null)
            {
                m_editor.PlaymodeStateChanging -= OnPlaymodeStateChanging;
                m_editor.IsOpenedChanged -= OnIsOpenedChanged;
            }

            ExposeToEditor._Awaked -= OnAwaked;
            ExposeToEditor._Enabled -= OnEnabled;
            ExposeToEditor._Started -= OnStarted;
            ExposeToEditor._Disabled -= OnDisabled;
            ExposeToEditor._Destroying -= OnDestroying;
            ExposeToEditor._Destroyed -= OnDestroyed;
            ExposeToEditor._MarkAsDestroyedChanged -= OnMarkAsDestroyedChanged;

            ExposeToEditor._TransformChanged -= OnTransformChanged;
            ExposeToEditor._NameChanged -= OnNameChanged;
            ExposeToEditor._ParentChanged -= OnParentChanged;
        }

        private void OnIsOpenedChanged()
        {
            if (m_editor.IsOpened)
            {
                foreach(ExposeToEditor obj in m_editModeCache)
                {
                    TryToAddColliders(obj);
                }
            }
            else
            {
                foreach (ExposeToEditor obj in m_editModeCache)
                {
                    TryToDestroyColliders(obj);
                }
            }
        }

        private void OnPlaymodeStateChanging()
        {
            if (m_editor.IsPlaying)
            {
                m_playModeCache = new HashSet<ExposeToEditor>();
                m_enabledObjects = m_editModeCache.Where(eo => eo.gameObject.activeSelf).ToArray();
                m_selectedObjects = m_editor.Selection.objects;

                HashSet<GameObject> selectionHS = new HashSet<GameObject>(m_editor.Selection.gameObjects != null ? m_editor.Selection.gameObjects : new GameObject[0]);
                List<GameObject> playmodeSelection = new List<GameObject>();
                foreach(ExposeToEditor editorObj in m_editModeCache.OrderBy(eo => eo.transform.GetSiblingIndex()))
                {
                    if (editorObj.Parent != null)
                    {
                        continue;
                    }

                    GameObject instance = Instantiate(editorObj.gameObject, editorObj.transform.position, editorObj.transform.rotation);
                    ExposeToEditor playModeObj = instance.GetComponent<ExposeToEditor>();
                    playModeObj.SetName(editorObj.name);
                    playModeObj.Init();
                    m_playModeCache.Add(playModeObj);

                    ExposeToEditor[] editorObjAndChildren = editorObj.GetComponentsInChildren<ExposeToEditor>(true);
                    ExposeToEditor[] playModeObjAndChildren = instance.GetComponentsInChildren<ExposeToEditor>(true);
                    for (int j = 0; j < editorObjAndChildren.Length; j++)
                    {
                        if (selectionHS.Contains(editorObjAndChildren[j].gameObject))
                        {
                            playmodeSelection.Add(playModeObjAndChildren[j].gameObject);
                        }
                    }

                    editorObj.gameObject.SetActive(false);
                }

                bool isEnabled = m_editor.Undo.Enabled;
                m_editor.Undo.Enabled = false;
                m_editor.Selection.objects = playmodeSelection.ToArray();
                m_editor.Undo.Enabled = isEnabled;
                m_editor.Undo.Store();
            }
            else
            {
                foreach (ExposeToEditor playObj in m_playModeCache)
                {
                    if(playObj != null)
                    {
                        DestroyImmediate(playObj.gameObject);
                    }
                }
                
                for (int i = 0; i < m_enabledObjects.Length; ++i)
                {
                    ExposeToEditor editorObj = m_enabledObjects[i];
                    if (editorObj != null)
                    {
                        editorObj.gameObject.SetActive(true);
                    }
                }

                bool isEnabled = m_editor.Undo.Enabled;
                m_editor.Undo.Enabled = false;
                m_editor.Selection.objects = m_selectedObjects;
                m_editor.Undo.Enabled = isEnabled;
                m_editor.Undo.Restore();

                m_playModeCache = null;
                m_enabledObjects = null;
                m_selectedObjects = null;
            }
        }

        private static bool IsExposedToEditor(ExposeToEditor exposeToEditor)
        {
            return exposeToEditor != null &&
                !exposeToEditor.MarkAsDestroyed &&
                exposeToEditor.hideFlags != HideFlags.HideAndDontSave;
        }

        private static List<ExposeToEditor> FindAll()
        {
            if (SceneManager.GetActiveScene().isLoaded)
            {
                return FindAllUsingSceneManagement();
            }
            List<ExposeToEditor> result = new List<ExposeToEditor>();
            ExposeToEditor[] objects = Resources.FindObjectsOfTypeAll<ExposeToEditor>();
            for (int i = 0; i < objects.Length; ++i)
            {
                ExposeToEditor obj = objects[i];
                if (obj == null)
                {
                    continue;
                }

                if(!IsExposedToEditor(obj))
                {
                    continue;
                }

                if (!obj.gameObject.IsPrefab())
                {
                    result.Add(obj);
                }
            }

            return result;
        }

        private static List<ExposeToEditor> FindAllUsingSceneManagement()
        {
            List<ExposeToEditor> result = new List<ExposeToEditor>();
            GameObject[] rootGameObjects = SceneManager.GetActiveScene().GetRootGameObjects();
            for (int i = 0; i < rootGameObjects.Length; ++i)
            {
                ExposeToEditor[] exposedObjects = rootGameObjects[i].GetComponentsInChildren<ExposeToEditor>(true);
                for (int j = 0; j < exposedObjects.Length; ++j)
                {
                    ExposeToEditor obj = exposedObjects[j];
                    if (IsExposedToEditor(obj))
                    {
                        result.Add(obj);
                    }
                }
            }
            return result;
        }

        private void TryToAddColliders(ExposeToEditor obj)
        {
            if (obj == null)
            {
                return;
            }

            if (obj.Colliders == null || obj.Colliders.Length == 0)
            {
                List<Collider> colliders = new List<Collider>();
                Rigidbody rigidBody = obj.BoundsObject.GetComponent<Rigidbody>();

                bool isRigidBody = rigidBody != null;
                if (obj.EffectiveBoundsType == BoundsType.Any)
                {
                    if (obj.MeshFilter != null)
                    {
                        if (obj.AddColliders && !isRigidBody)
                        {
                            MeshCollider collider = obj.BoundsObject.AddComponent<MeshCollider>();
                            collider.convex = isRigidBody;
                            collider.sharedMesh = obj.MeshFilter.sharedMesh;
                            colliders.Add(collider);
                        }
                    }
                    else if (obj.SkinnedMeshRenderer != null)
                    {
                        if (obj.AddColliders && !isRigidBody)
                        {
                            MeshCollider collider = obj.BoundsObject.AddComponent<MeshCollider>();
                            collider.convex = isRigidBody;
                            collider.sharedMesh = obj.SkinnedMeshRenderer.sharedMesh;
                            colliders.Add(collider);
                        }
                    }
                    else if (obj.SpriteRenderer != null)
                    {
                        if (obj.AddColliders && !isRigidBody)
                        {
                            BoxCollider collider = obj.BoundsObject.AddComponent<BoxCollider>();
                            collider.size = obj.SpriteRenderer.sprite.bounds.size;
                            colliders.Add(collider);
                        }
                    }
                }
                else if (obj.EffectiveBoundsType == BoundsType.Mesh)
                {
                    if (obj.MeshFilter != null)
                    {
                        if (obj.AddColliders && !isRigidBody)
                        {
                            MeshCollider collider = obj.BoundsObject.AddComponent<MeshCollider>();
                            collider.convex = isRigidBody;
                            collider.sharedMesh = obj.MeshFilter.sharedMesh;
                            colliders.Add(collider);
                        }
                    }
                }
                else if (obj.EffectiveBoundsType == BoundsType.SkinnedMesh)
                {
                    if (obj.SkinnedMeshRenderer != null)
                    {
                        if (obj.AddColliders && !isRigidBody)
                        {
                            MeshCollider collider = obj.BoundsObject.AddComponent<MeshCollider>();
                            collider.convex = isRigidBody;
                            collider.sharedMesh = obj.SkinnedMeshRenderer.sharedMesh;
                            colliders.Add(collider);
                        }
                    }
                }
                else if (obj.EffectiveBoundsType == BoundsType.Sprite)
                {
                    if (obj.SpriteRenderer != null)
                    {
                        if (obj.AddColliders && !isRigidBody)
                        {
                            BoxCollider collider = obj.BoundsObject.AddComponent<BoxCollider>();
                            collider.size = obj.SpriteRenderer.sprite.bounds.size;
                            colliders.Add(collider);
                        }
                    }
                }
                else if (obj.EffectiveBoundsType == BoundsType.Custom)
                {
                    if (obj.AddColliders && !isRigidBody)
                    {
                        Mesh box = RuntimeGraphics.CreateCubeMesh(Color.black, obj.CustomBounds.center, obj.CustomBounds.extents.x * 2, obj.CustomBounds.extents.y * 2, obj.CustomBounds.extents.z * 2);

                        MeshCollider collider = obj.BoundsObject.AddComponent<MeshCollider>();
                        collider.convex = isRigidBody;

                        collider.sharedMesh = box;
                        colliders.Add(collider);
                    }
                }

                obj.Colliders = colliders.ToArray();
            }
        }

        private void TryToDestroyColliders(ExposeToEditor obj)
        {
            if (obj != null && obj.Colliders != null)
            {
                for (int i = 0; i < obj.Colliders.Length; ++i)
                {
                    Collider collider = obj.Colliders[i];
                    if (collider != null)
                    {
                        Destroy(collider);
                    }
                }
                obj.Colliders = null;
            }
        }

        private void OnAwaked(ExposeToEditor obj)
        {
            if(m_editor.IsPlaying)
            {
                if(!m_playModeCache.Contains(obj))
                {
                    m_playModeCache.Add(obj);
                }
                
            }
            else
            {
                if(!m_editModeCache.Contains(obj))
                {
                    m_editModeCache.Add(obj);
                    if (m_editor.IsOpened)
                    {
                        TryToAddColliders(obj);
                    }
                    else
                    {
                        TryToDestroyColliders(obj);
                    }
                }                
            }

            if (Awaked != null)
            {
                Awaked(obj);
            }
        }

        private void OnDestroying(ExposeToEditor obj)
        {
            if (Destroying != null)
            {
                Destroying(obj);
            }
        }

        private void OnDestroyed(ExposeToEditor obj)
        {
            if (m_editor.IsPlaying)
            {
                m_playModeCache.Remove(obj);
            }
            else
            {
                m_editModeCache.Remove(obj);
                TryToDestroyColliders(obj);
            }
            if (Destroyed != null)
            {
                Destroyed(obj);
            }
        }

        private void OnMarkAsDestroyedChanged(ExposeToEditor obj)
        {
            if (m_editor.IsPlaying)
            {
                if (obj.MarkAsDestroyed)
                {
                    m_playModeCache.Remove(obj);
                }
                else
                {
                    m_playModeCache.Add(obj);
                }

            }
            else
            {
                if (obj.MarkAsDestroyed)
                {
                    m_editModeCache.Remove(obj);
                }
                else
                {
                    m_editModeCache.Add(obj);
                }
            }

            if (MarkAsDestroyedChanged != null)
            {
                MarkAsDestroyedChanged(obj);
            }
        }

        private void OnEnabled(ExposeToEditor obj)
        {
            if (Enabled != null)
            {
                Enabled(obj);
            }
        }

        private void OnStarted(ExposeToEditor obj)
        {
            if (Started != null)
            {
                Started(obj);
            }
        }

        private void OnDisabled(ExposeToEditor obj)
        {
            if (Disabled != null)
            {
                Disabled(obj);
            }
        }

        private void OnTransformChanged(ExposeToEditor obj)
        {
            if (TransformChanged != null)
            {
                TransformChanged(obj);
            }
        }

        private void OnNameChanged(ExposeToEditor obj)
        {
            if (NameChanged != null)
            {
                NameChanged(obj);
            }
        }

        private void OnParentChanged(ExposeToEditor obj, ExposeToEditor oldValue, ExposeToEditor newValue)
        {
            if (ParentChanged != null)
            {
                ParentChanged(obj, oldValue, newValue);
            }
        }
    }
}
