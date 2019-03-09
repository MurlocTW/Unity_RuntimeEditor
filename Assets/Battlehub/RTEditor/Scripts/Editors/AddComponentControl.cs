﻿using Battlehub.RTCommon;
using Battlehub.UIControls;
using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Battlehub.RTEditor
{
    public class AddComponentControl : MonoBehaviour
    {
        public event Action<Type> ComponentSelected;

        [SerializeField]
        private Dropdown m_dropDown = null;

        private VirtualizingTreeView m_treeView;

        private InputField m_filter = null;

        private Type[] m_cache;
        private string m_filterText;

        private bool m_isOpened;

        private void Update()
        {
            bool isOpened = m_dropDown.transform.childCount == 3;

            if(m_isOpened != isOpened)
            {
                m_isOpened = isOpened;
                if(m_isOpened)
                {
                    OnOpened();
                }
                else
                {
                    OnClosed();
                }
            }
        }

        private void OnOpened()
        {
            Type[] editableTypes = IOC.Resolve<IEditorsMap>().GetEditableTypes();

            m_filter = GetComponentInChildren<InputField>();
            if(m_filter != null)
            {
                m_filter.onValueChanged.AddListener(OnFilterValueChanged);
                m_filter.text = m_filterText;
                m_filter.Select();
            }

            m_treeView = GetComponentInChildren<VirtualizingTreeView>();
            m_treeView.ItemDataBinding += OnItemDataBinding;
            m_treeView.SelectionChanged += OnSelectionChanged;
            m_cache = editableTypes.Where(t => t.IsSubclassOf(typeof(Component))).OrderBy(t => t.Name).ToArray();
            InstantApply(m_filterText);
        }

        private void OnClosed()
        {
            if (m_filter != null)
            {
                m_filter.onValueChanged.RemoveListener(OnFilterValueChanged);
            }

            if (m_treeView != null)
            {
                m_treeView.ItemDataBinding -= OnItemDataBinding;
                m_treeView.SelectionChanged -= OnSelectionChanged;
            }
        }

        private void OnItemDataBinding(object sender, VirtualizingTreeViewItemDataBindingArgs e)
        {
            Type type = (Type)e.Item;
            Text text = e.ItemPresenter.GetComponentInChildren<Text>(true);
            text.text = type.Name;
        }

        private void OnSelectionChanged(object sender, SelectionChangedArgs e)
        {
            StartCoroutine(CoHide());
        }

        private IEnumerator CoHide()
        {
            yield return new WaitForEndOfFrame();
            if(m_treeView.SelectedItem != null)
            {
                m_dropDown.Hide();
                if (ComponentSelected != null)
                {
                    ComponentSelected((Type)m_treeView.SelectedItem);
                }
            }
        }


        private void OnFilterValueChanged(string text)
        {
            m_filterText = text;
            ApplyFilter(text);
        }

        private void ApplyFilter(string text)
        {
            if (m_coApplyFilter != null)
            {
                StopCoroutine(m_coApplyFilter);
            }
            StartCoroutine(m_coApplyFilter = CoApplyFilter(text));
        }

        private IEnumerator m_coApplyFilter;
        private IEnumerator CoApplyFilter(string filter)
        {
            yield return new WaitForSeconds(0.3f);

            InstantApply(filter);

        }

        private void InstantApply(string filter)
        {
            if (m_treeView != null)
            {
                if (string.IsNullOrEmpty(filter))
                {
                    m_treeView.Items = m_cache;
                }
                else
                {
                    m_treeView.Items = m_cache.Where(item => item.Name.ToLower().Contains(filter.ToLower()));
                }
            }
        }
    }
}

