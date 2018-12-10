﻿using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Battlehub.UIControls.MenuControl
{
    public delegate void MenuItemEventHandler(MenuItem menuItem);

    public class MenuItem : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        [SerializeField]
        private Menu m_menuPrefab;

        [SerializeField]
        private Image m_icon;

        [SerializeField]
        private Text m_text;

        [SerializeField]
        private GameObject m_expander;

        [SerializeField]
        private GameObject m_selection;

        private Transform m_root;
        public Transform Root
        {
            get { return m_root; }
            set { m_root = value; }
        }

        private int m_depth;
        public int Depth
        {
            get { return m_depth; }
            set { m_depth = value; }
        }

        private MenuItemInfo m_item;
        public MenuItemInfo Item
        {
            get { return m_item; }
            set
            {
                if(m_item != value)
                {
                    m_item = value;
                    DataBind();
                }
            }
        }

        private MenuItemInfo[] m_children;
        public MenuItemInfo[] Children
        {
            get { return m_children; }
            set { m_children = value; }
        }

        public bool HasChildren
        {
            get { return m_children != null && m_children.Length > 0; }
        }

        private bool m_isPointerOver;
        public bool IsPointerOver
        {
            get { return m_isPointerOver; }
        }

        private Menu m_submenu;
        public Menu Submenu
        {
            get { return m_submenu; }
        }

        private GraphicRaycaster m_raycaster;

        private void Awake()
        {
            m_raycaster = GetComponentInParent<GraphicRaycaster>();
            if(m_raycaster == null)
            {
                Canvas canvas = GetComponentInParent<Canvas>();
                if(canvas != null)
                {
                    m_raycaster = canvas.gameObject.AddComponent<GraphicRaycaster>();
                }
            }
        }

        private void OnDestroy()
        {
            if(m_submenu != null)
            {
                Destroy(m_submenu.gameObject);
            }
        }

        private void DataBind()
        {
            if(m_item != null)
            {
                m_icon.sprite = m_item.Icon;
                m_icon.gameObject.SetActive(m_icon.sprite != null);
                m_text.text = m_item.Text;
                m_expander.SetActive(HasChildren);
            }
            else
            {
                m_icon.sprite = null;
                m_icon.gameObject.SetActive(false);
                m_text.text = string.Empty;
                m_expander.SetActive(false);
            }
        }

        void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
        {
            if (HasChildren)
            {
                return;
            }
            if (m_item.Action != null)
            {
                if (m_item.Validate != null)
                {
                    MenuItemValidationArgs args = new MenuItemValidationArgs();
                    m_item.Validate.Invoke(args);
                    if (args.IsValid)
                    {
                        m_item.Action.Invoke();
                    }
                }
                else
                {
                    m_item.Action.Invoke();
                }
            }


            Menu menu = GetComponentInParent<Menu>();
            while(menu.Parent != null)
            {
                menu = menu.Parent.GetComponentInParent<Menu>();
            }
            menu.Close();
        }

        void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
        {
            Select();
        }

        void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
        {
            m_isPointerOver = false;
            if(m_submenu == null)
            {
                Menu menu = GetComponentInParent<Menu>();
                menu.Child = null;
                m_selection.SetActive(false);
            }
            else
            {
                if (!IsPointerOverSubmenu(eventData))
                {
                    Unselect();
                }
            }
        }

        public void Select()
        {
            m_isPointerOver = true;
            m_selection.SetActive(true);

            Menu menu = GetComponentInParent<Menu>();
            menu.Child = this;

            if (HasChildren)
            {
                if (m_submenu == null)
                {
                    m_submenu = Instantiate(m_menuPrefab, m_root, false);
                    m_submenu.Parent = this;
                    m_submenu.name = "Submenu";
                    m_submenu.Depth = m_depth;
                    m_submenu.Items = Children;
                    m_submenu.transform.position = FindPosition();
                }
            }
        }

        public void Unselect()
        {
            Menu menu = GetComponentInParent<Menu>();
            menu.Child = null;
            m_selection.SetActive(false);
            if (m_submenu != null)
            {
                Destroy(m_submenu.gameObject);
                m_submenu = null;
            }
        }

        private Vector3 FindPosition()
        {
            const float overlap = 5;

            RectTransform rootRT = (RectTransform)m_root;
            RectTransform rt = (RectTransform)transform;

            Vector2 size = new Vector2(rt.rect.width, rt.rect.height * Children.Length);

            Vector3 position = -Vector2.Scale(rt.rect.size, rt.pivot);
            position.y = -position.y;
            position = rt.TransformPoint(position);
            position = rootRT.InverseTransformPoint(position);

            Vector2 topLeft = -Vector2.Scale(rootRT.rect.size, rootRT.pivot);
            
            if (position.x + size.x + size.x > topLeft.x + rootRT.rect.width)
            {
                position.x = position.x - size.x + overlap;
            }
            else
            {
                position.x = position.x + size.x - overlap;
            }            
            
            if (position.y - size.y < topLeft.y)
            {
                position.y -= (position.y - size.y) - topLeft.y;
            }

            return rootRT.TransformPoint(position);
        }

        private bool IsPointerOverSubmenu(PointerEventData eventData)
        {
            List<RaycastResult> raycastResultList = new List<RaycastResult>();
            m_raycaster.Raycast(eventData, raycastResultList);
            for(int i = 0; i < raycastResultList.Count; ++i)
            {
                RaycastResult raycastResult = raycastResultList[i];
                if(raycastResult.gameObject == m_submenu.gameObject)
                {
                    return true;
                }
            }

            return false;
        }
    }
}

