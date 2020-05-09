using UnityEngine;
using Battlehub.RTCommon;
using System.Linq;
using Battlehub.Utils;

namespace Battlehub.RTHandles.URP
{
    public class OutlineManagerURP : MonoBehaviour, IOutlineManager
    {
        private IRenderersCache m_cache;
        private IRTE m_editor;

        private IRuntimeSelection m_selectionOverride;
        public IRuntimeSelection Selection
        {
            get
            {
                if (m_selectionOverride != null)
                {
                    return m_selectionOverride;
                }

                return m_editor.Selection;
            }
            set
            {
                if (m_selectionOverride != value)
                {
                    if (m_selectionOverride != null)
                    {
                        m_selectionOverride.SelectionChanged -= OnSelectionChanged;
                    }

                    m_selectionOverride = value;
                    if (m_selectionOverride == m_editor.Selection)
                    {
                        m_selectionOverride = null;
                    }

                    if (m_selectionOverride != null)
                    {
                        m_selectionOverride.SelectionChanged += OnSelectionChanged;
                    }
                }
            }
        }

        private void Start()
        {
            m_cache = GetComponentInChildren<IRenderersCache>();
            IOC.Register("SelectedRenderers", m_cache);

            m_editor = IOC.Resolve<IRTE>();

            TryToAddRenderers(m_editor.Selection);
            m_editor.Selection.SelectionChanged += OnRuntimeEditorSelectionChanged;

            IOC.RegisterFallback<IOutlineManager>(this);
        }

        private void OnDestroy()
        {
            if (m_editor != null)
            {
                m_editor.Selection.SelectionChanged -= OnRuntimeEditorSelectionChanged;
            }

            if (m_selectionOverride != null)
            {
                m_selectionOverride.SelectionChanged -= OnSelectionChanged;
            }

            IOC.Unregister("SelectedRenderers", m_cache);
            IOC.UnregisterFallback<IOutlineManager>(this);
        }

        private void OnRuntimeEditorSelectionChanged(Object[] unselectedObject)
        {
            OnSelectionChanged(m_editor.Selection, unselectedObject);
        }

        private void OnSelectionChanged(Object[] unselectedObjects)
        {
            OnSelectionChanged(m_selectionOverride, unselectedObjects);
        }

        private void OnSelectionChanged(IRuntimeSelection selection, Object[] unselectedObjects)
        {
            if (unselectedObjects != null)
            {
                Renderer[] renderers = unselectedObjects.Select(go => go as GameObject).Where(go => go != null).SelectMany(go => go.GetComponentsInChildren<Renderer>(true)).ToArray();
                for(int i = 0; i < renderers.Length; ++i)
                {
                    Renderer renderer = renderers[i];
                    m_cache.Remove(renderer);
                }
            }
            TryToAddRenderers(selection);
        }

        private void TryToAddRenderers(IRuntimeSelection selection)
        {
            if (selection.gameObjects != null)
            {
                Renderer[] renderers = selection.gameObjects.Where(go => go != null).Select(go => go.GetComponent<ExposeToEditor>()).Where(e => e != null && e.ShowSelectionGizmo && !e.gameObject.IsPrefab() && (e.gameObject.hideFlags & HideFlags.HideInHierarchy) == 0).SelectMany(e => e.GetComponentsInChildren<Renderer>().Where(r => (r.gameObject.hideFlags & HideFlags.HideInHierarchy) == 0)).ToArray();
                for (int i = 0; i < renderers.Length; ++i)
                {
                    Renderer renderer = renderers[i];
                    if((renderer.hideFlags & HideFlags.HideInHierarchy) == 0)
                    {
                        m_cache.Add(renderer);
                    }    
                }
            }
        }

        public void AddRenderers(Renderer[] renderers)
        {
            for (int i = 0; i < renderers.Length; ++i)
            {
                Renderer renderer = renderers[i];
                m_cache.Add(renderer);
            }
        }

        public void RemoveRenderers(Renderer[] renderers)
        {
            for (int i = 0; i < renderers.Length; ++i)
            {
                Renderer renderer = renderers[i];
                m_cache.Remove(renderer);
            }
        }

        public void RecreateCommandBuffer()
        {
            
        }

        public bool ContainsRenderer(Renderer renderer)
        {
            return m_cache.Renderers.Contains(renderer);
        }
    }

}

