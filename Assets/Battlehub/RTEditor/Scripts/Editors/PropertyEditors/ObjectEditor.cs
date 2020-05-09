using Battlehub.RTCommon;
using Battlehub.RTSL.Interface;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

using UnityObject = UnityEngine.Object;
namespace Battlehub.RTEditor
{
    public interface IObjectEditorLoader
    {
        void Load(object obj, Type memberInfoType, Action<UnityObject> callback);
        Type GetObjectType(object obj, Type memberInfoType);
    }

    public class ObjectEditor : PropertyEditor<UnityObject>, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField]
        private GameObject DragHighlight = null;
        [SerializeField]
        private TMP_InputField Input = null;
        [SerializeField]
        private Button BtnSelect = null;

        private IObjectEditorLoader m_loader;
        private ILocalization m_localization;

        protected override void SetInputField(UnityObject value)
        {
            string memberInfoTypeName = m_localization.GetString("ID_RTEditor_PE_TypeName_" + MemberInfoType.Name, MemberInfoType.Name);

            bool isDestroyed = value == null;

            if (!isDestroyed)
            {
                GameObject go = null;
                if (value is GameObject)
                {
                    go = (GameObject)value;
                }
                else if (value is Component)
                {
                    go = ((Component)value).gameObject;
                }

                if (go != null)
                {
                    ExposeToEditor exposeToEditor = go.GetComponent<ExposeToEditor>();
                    if (exposeToEditor != null && exposeToEditor.MarkAsDestroyed)
                    {
                        isDestroyed = true;
                    }
                }
            }

            if (isDestroyed)
            {
                Input.text = string.Format(m_localization.GetString("ID_RTEditor_PE_ObjectEditor_None", "None") + " ({0})", memberInfoTypeName);
            }
            else
            {
                Input.text = string.Format("{1} ({0})", memberInfoTypeName, value.name);
            }
        }

        protected override void AwakeOverride()
        {
            base.AwakeOverride();
            BtnSelect.onClick.AddListener(OnSelect);

            m_localization = IOC.Resolve<ILocalization>();
            m_loader = IOC.Resolve<IObjectEditorLoader>();
            if(m_loader == null)
            {
                m_loader = Editor.Root.gameObject.AddComponent<ObjectEditorLoader>();
            }
        }

        protected override void OnDestroyOverride()
        {
            base.OnDestroyOverride();
            if(BtnSelect != null)
            {
                BtnSelect.onClick.RemoveListener(OnSelect);
            }

            if(Editor != null)
            {
                Editor.DragDrop.Drop -= OnDrop;
            }
        }
        
        private void OnSelect()
        {
            ISelectObjectDialog objectSelector = null;

            IWindowManager wm = IOC.Resolve<IWindowManager>();

            string memberInfoTypeName = m_localization.GetString("ID_RTEditor_PE_TypeName_" + MemberInfoType.Name, MemberInfoType.Name);
            string select = m_localization.GetString("ID_RTEditor_PE_ObjectEditor_Select", "Select") + " ";
            if (wm.IsWindowRegistered("Select" + MemberInfoType.Name))
            {
                Transform dialogTransform = IOC.Resolve<IWindowManager>().CreateDialogWindow("Select" + MemberInfoType.Name, select + memberInfoTypeName,
                      (sender, args) =>
                      {
                          if (objectSelector.IsNoneSelected)
                          {
                              SetObject(null);
                          }
                          else
                          {
                              SetObject(objectSelector.SelectedObject);
                          }
                      });
            }
            else
            {
                Transform dialogTransform = IOC.Resolve<IWindowManager>().CreateDialogWindow(RuntimeWindowType.SelectObject.ToString(), select + memberInfoTypeName,
                    (sender, args) =>
                    {
                        if (objectSelector.IsNoneSelected)
                        {
                            SetObject(null);
                        }
                        else
                        {
                            SetObject(objectSelector.SelectedObject);
                        }
                    });
            }
            
            objectSelector = IOC.Resolve<ISelectObjectDialog>();
            objectSelector.ObjectType = MemberInfoType;
        }

        public void SetObject(UnityObject obj)
        {
            SetValue(obj);
            EndEdit();
            SetInputField(obj);
        }

        private void OnDrop(PointerEventData pointerEventData)
        {
            object dragObject = Editor.DragDrop.DragObjects[0];
            
            Editor.IsBusy = true;
            m_loader.Load(dragObject, MemberInfoType, loadedObject =>
            {
                Editor.IsBusy = false;
                SetObject(loadedObject);
                HideDragHighlight();
            });
        }

        void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
        {
            if(!Editor.DragDrop.InProgress)
            {
                return;
            }
            object dragObject = Editor.DragDrop.DragObjects[0];            
            Type type = m_loader.GetObjectType(dragObject, MemberInfoType);
           
            if (type != null && MemberInfoType.IsAssignableFrom(type))
            {
                Editor.DragDrop.Drop -= OnDrop;
                Editor.DragDrop.Drop += OnDrop;
                ShowDragHighlight();
                Editor.DragDrop.SetCursor(Utils.KnownCursor.DropAllowed);
            }
        }

        void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
        {
            Editor.DragDrop.Drop -= OnDrop;
            if(Editor.DragDrop.InProgress)
            {
                Editor.DragDrop.SetCursor(Utils.KnownCursor.DropNotAllowed);
                HideDragHighlight();
            }
        }

        private void ShowDragHighlight()
        {
            if(DragHighlight != null)
            {
                DragHighlight.SetActive(true);
            }
        }

        private void HideDragHighlight()
        {
            if(DragHighlight != null)
            {
                DragHighlight.SetActive(false);
            }
        }
    }
}
