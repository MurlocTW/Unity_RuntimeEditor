using UnityEngine;
using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using Battlehub.RTCommon;
using Battlehub.RTSL.Interface;
using TMPro;

namespace Battlehub.RTEditor
{
    public struct PropertyDescriptor
    {
        public string Label;
        public string AnimationPropertyName;
        public MemberInfo MemberInfo;
        public MemberInfo ComponentMemberInfo;
        public PropertyEditorCallback ValueChangedCallback;
        public PropertyEditorCallback EndEditCallback;
        public Range Range;
        public PropertyDescriptor[] ChildDesciptors;
     
        public Type MemberType
        {
            get
            {
                if(Range != null)
                {
                    return Range.GetType();
                }

                if (MemberInfo is PropertyInfo)
                {
                    PropertyInfo prop = (PropertyInfo)MemberInfo;
                    return prop.PropertyType;
                }
                else if (MemberInfo is FieldInfo)
                {
                    FieldInfo field = (FieldInfo)MemberInfo;
                    return field.FieldType;
                }

                return null;
            }
        }

        public Type ComponentMemberType
        {
            get
            {
                if (ComponentMemberInfo is PropertyInfo)
                {
                    PropertyInfo prop = (PropertyInfo)ComponentMemberInfo;
                    return prop.PropertyType;
                }
                else if (ComponentMemberInfo is FieldInfo)
                {
                    FieldInfo field = (FieldInfo)ComponentMemberInfo;
                    return field.FieldType;
                }

                return null;
            }
        }

		public object Target;
        
        public PropertyDescriptor(string label, object target, MemberInfo memberInfo) : this(label, target, memberInfo, memberInfo.Name) {}

        public PropertyDescriptor(string label, object target, MemberInfo memberInfo, string animationPropertyName)
        {
            MemberInfo = memberInfo;
            ComponentMemberInfo = memberInfo;
            Label = label;
            Target = target;
            ValueChangedCallback = null;
            EndEditCallback = null;
            Range = TryGetRange(memberInfo);
            ChildDesciptors = null;
            AnimationPropertyName = animationPropertyName;
        }

        public PropertyDescriptor(string label, object target, MemberInfo memberInfo, MemberInfo componentMemberInfo)
        {
            MemberInfo = memberInfo;
            ComponentMemberInfo = componentMemberInfo;
            Label = label;
            Target = target;
            ValueChangedCallback = null;
            EndEditCallback = null;
            Range = TryGetRange(memberInfo);
            ChildDesciptors = null;
            AnimationPropertyName = null;
        }

        public PropertyDescriptor(string label, object target, MemberInfo memberInfo, MemberInfo componentMemberInfo, PropertyEditorCallback valueChangedCallback)
        {
            MemberInfo = memberInfo;
            ComponentMemberInfo = componentMemberInfo;
            Label = label;
            Target = target;
            ValueChangedCallback = valueChangedCallback;
            EndEditCallback = null;
            Range = TryGetRange(memberInfo);
            ChildDesciptors = null;
            AnimationPropertyName = null;
        }

        public PropertyDescriptor(string label, object target, MemberInfo memberInfo, MemberInfo componentMemberInfo, PropertyEditorCallback valueChangedCallback, PropertyEditorCallback endEditCallback)
        {
            MemberInfo = memberInfo;
            ComponentMemberInfo = componentMemberInfo;
            Label = label;
            Target = target;
            ValueChangedCallback = valueChangedCallback;
            EndEditCallback = endEditCallback;
            Range = TryGetRange(memberInfo);
            ChildDesciptors = null;
            AnimationPropertyName = null;
        }

        public PropertyDescriptor(string label, object target, MemberInfo memberInfo, MemberInfo componentMemberInfo, PropertyEditorCallback valueChangedCallback, Range range)
        {
            MemberInfo = memberInfo;
            ComponentMemberInfo = componentMemberInfo;
            Label = label;
            Target = target;
            ValueChangedCallback = valueChangedCallback;
            EndEditCallback = null;
            Range = range;
            ChildDesciptors = null;
            AnimationPropertyName = null;
        }

        private static Range TryGetRange(MemberInfo memberInfo)
        {
            RangeAttribute range = memberInfo.GetCustomAttribute<RangeAttribute>();
            if (range != null)
            {
                if (memberInfo.GetUnderlyingType() == typeof(int))
                {
                    return new RangeInt((int)range.min, (int)range.max);
                }
                else if (memberInfo.GetUnderlyingType() == typeof(float))
                {
                    return new Range(range.min, range.max);
                }
            }
            return null;
        }

    }

    public class VoidComponentEditor : ComponentEditor
    {
        public override Component Component
        {
            get { return m_component; }
            set { m_component = value; }
        }

        protected override void UpdateOverride()
        {

        }
    }

    public class ComponentEditor : MonoBehaviour
    {
        public PropertyEditorCallback EndEditCallback;

        [SerializeField]
        protected Transform EditorsPanel = null;
        [SerializeField]
        private BoolEditor EnabledEditor = null;
        [SerializeField]
        private TextMeshProUGUI Header = null;
        [SerializeField]
        private Toggle Expander = null;
        [SerializeField]
        private GameObject ExpanderGraphics = null;
        [SerializeField]
        private Button ResetButton = null;
        [SerializeField]
        private Button RemoveButton = null;

        private object m_converter;

        private Component m_gizmo;

        private bool IsComponentExpanded
        {
            get
            {
                string componentName = "BH_CE_EX_" + Component.GetType().AssemblyQualifiedName;
                return PlayerPrefs.GetInt(componentName, 1) == 1;
            }
            set
            {
                string componentName = "BH_CE_EX_" + Component.GetType().AssemblyQualifiedName;
                PlayerPrefs.SetInt(componentName, value ? 1 : 0);
            }
        }

        protected Component m_component;
        public virtual Component Component
        {
            get { return m_component; }
            set
            {
                m_component = value;
                if(m_component == null)
                {
                    throw new ArgumentNullException("value");
                }

                IComponentDescriptor componentDescriptor = GetComponentDescriptor();
                if(EnabledEditor != null)
                {
                    PropertyInfo enabledProperty = EnabledProperty;
                    if (enabledProperty != null && (componentDescriptor == null || componentDescriptor.GetHeaderDescriptor(m_editor).ShowEnableButton))
                    {
                        EnabledEditor.gameObject.SetActive(true);
                        EnabledEditor.Init(Component, Component, enabledProperty, null, string.Empty, () => { },
                            () =>
                            {
                                if (IsComponentEnabled)
                                {
                                    TryCreateGizmo(componentDescriptor);
                                }
                                else
                                {
                                    DestroyGizmo();
                                }
                            },
                            () =>
                            {
                                if (EndEditCallback != null)
                                {
                                    EndEditCallback();
                                }
                            });
                    }
                    else
                    {
                        EnabledEditor.gameObject.SetActive(false);
                    }
                }

              
                if(Header != null)
                {
                    if (componentDescriptor != null)
                    {
                        Header.text = componentDescriptor.GetHeaderDescriptor(m_editor).DisplayName;
                    }
                    else
                    {
                        string typeName = Component.GetType().Name;
                        ILocalization localization = IOC.Resolve<ILocalization>();
                        Header.text = localization.GetString("ID_RTEditor_CD_" + typeName, typeName);
                    }
                }
                
                if(Expander != null)
                {
                    Expander.isOn = IsComponentExpanded;
                }
                
                BuildEditor();
            }
        }

        private bool IsComponentEnabled
        {
            get
            {
                if (EnabledProperty == null)
                {
                    return true;
                }

                object v = EnabledProperty.GetValue(Component, null);
                if (v is bool)
                {
                    bool isEnabled = (bool)v;
                    return isEnabled;
                }
                return true;
            }   
        }

        private PropertyInfo EnabledProperty
        {
            get
            {
                Type type = Component.GetType();

                while(type != typeof(UnityEngine.Object))
                {
                    PropertyInfo prop = type.GetProperty("enabled", BindingFlags.Public | BindingFlags.DeclaredOnly | BindingFlags.Instance);
                    if (prop != null && prop.PropertyType == typeof(bool) && prop.CanRead && prop.CanWrite)
                    {
                        return prop;
                    }
                    type = type.BaseType();
                }

                return null;
            }
        }

        private IRTE m_editor;
        public IRTE Editor
        {
            get { return m_editor; }
        }

        private IProject m_project;

        private IEditorsMap m_editorsMap;

        private void Awake()
        {
            m_editor = IOC.Resolve<IRTE>();
            if(m_editor.Object != null)
            {
                m_editor.Object.ReloadComponentEditor += OnReloadComponentEditor;
            }
            m_project = IOC.Resolve<IProject>();
            m_editorsMap = IOC.Resolve<IEditorsMap>();

            AwakeOverride();
        }

        protected virtual void AwakeOverride()
        {

        }

        private void Start()
        {
            if (Component == null)
            {
                return;
            }

            
            if(Expander != null)
            {
                Expander.onValueChanged.AddListener(OnExpanded);
            }
            
            if(ResetButton != null)
            {
                ResetButton.onClick.AddListener(OnResetClick);
            }

            if(RemoveButton != null)
            {
                RemoveButton.onClick.AddListener(OnRemove);
            }

            m_editor.Object.ReloadComponentEditor -= OnReloadComponentEditor;
            m_editor.Object.ReloadComponentEditor += OnReloadComponentEditor;
            m_editor.Undo.UndoCompleted += OnUndoCompleted;
            m_editor.Undo.RedoCompleted += OnRedoCompleted;
            
            StartOverride();
        }

        protected virtual void StartOverride()
        {

        }

        private void OnDestroy()
        {
            if(m_editor != null)
            {
                m_editor.Undo.UndoCompleted -= OnUndoCompleted;
                m_editor.Undo.RedoCompleted -= OnRedoCompleted;
                if(m_editor.Object != null)
                {
                    m_editor.Object.ReloadComponentEditor -= OnReloadComponentEditor;
                }
            }

            if (Expander != null)
            {
                Expander.onValueChanged.RemoveListener(OnExpanded);
            }

            if(ResetButton != null)
            {
                ResetButton.onClick.RemoveListener(OnResetClick);
            }

            if (RemoveButton != null)
            {
                RemoveButton.onClick.RemoveListener(OnRemove);
            }

            if (m_gizmo != null)
            {
                Destroy(m_gizmo);
                m_gizmo = null;
            }            

            OnDestroyOverride();
        }

        protected virtual void OnDestroyOverride()
        {

        }

        private void Update()
        {
            UpdateOverride();
        }

        protected virtual void UpdateOverride()
        {
            if(Component == null)
            {
                Destroy(gameObject);
            }
        }

        protected IComponentDescriptor GetComponentDescriptor()
        {
            IComponentDescriptor componentDescriptor;
            if (m_editorsMap.ComponentDescriptors.TryGetValue(Component.GetType(), out componentDescriptor))
            {
                return componentDescriptor;
            }
            return null;
        }

        public void BuildEditor()
        {
            IComponentDescriptor componentDescriptor = GetComponentDescriptor();
            if (componentDescriptor != null)
            {
                m_converter = componentDescriptor.CreateConverter(this);
            }

            PropertyDescriptor[] descriptors = m_editorsMap.GetPropertyDescriptors(Component.GetType(), this, m_converter);
            if (descriptors == null || descriptors.Length == 0)
            {
                if(ExpanderGraphics != null)
                {
                    ExpanderGraphics.SetActive(false);
                }
                
                return;
            }

            ISettingsComponent settingsComponent = IOC.Resolve<ISettingsComponent>();
            BuiltInWindowsSettings settings;
            if (settingsComponent == null)
            {
                settings = BuiltInWindowsSettings.Default;
            }
            else
            {
                settings = settingsComponent.BuiltInWindowsSettings;
            }

            if (ResetButton != null)
            {
                ResetButton.gameObject.SetActive(componentDescriptor != null ?
                    componentDescriptor.GetHeaderDescriptor(m_editor).ShowResetButton :
                    settings.Inspector.ComponentEditor.ShowResetButton);
            }

            if (RemoveButton != null)
            {
                bool showRemoveButton = componentDescriptor != null ?
                    componentDescriptor.GetHeaderDescriptor(m_editor).ShowRemoveButton :
                    settings.Inspector.ComponentEditor.ShowRemoveButton;
                if (showRemoveButton)
                {
                    bool canRemove = m_project == null || m_project.ToAssetItem(Component.gameObject) == null;
                    if (!canRemove)
                    {
                        showRemoveButton = false;
                    }
                }

                RemoveButton.gameObject.SetActive(showRemoveButton);
            }

            if (EnabledEditor != null && EnabledProperty != null)
            {
                EnabledEditor.gameObject.SetActive(componentDescriptor != null ?
                    componentDescriptor.GetHeaderDescriptor(m_editor).ShowEnableButton :
                    settings.Inspector.ComponentEditor.ShowEnableButton);
            }

            if (Expander == null)
            {
                BuildEditor(componentDescriptor, descriptors);
            }
            else
            {
                if (componentDescriptor != null ? !componentDescriptor.GetHeaderDescriptor(m_editor).ShowExpander : !settings.Inspector.ComponentEditor.ShowExpander)
                {
                    Expander.isOn = true;
                    Expander.enabled = false;
                }
                
                if (Expander.isOn)
                {
                    if (ExpanderGraphics != null)
                    {
                        ExpanderGraphics.SetActive(componentDescriptor != null ? componentDescriptor.GetHeaderDescriptor(m_editor).ShowExpander : settings.Inspector.ComponentEditor.ShowExpander);
                    }
                    BuildEditor(componentDescriptor, descriptors);
                }
            }
        }

        protected virtual void BuildEditor(IComponentDescriptor componentDescriptor, PropertyDescriptor[] descriptors)
        {
            DestroyEditor();
            TryCreateGizmo(componentDescriptor);

            for (int i = 0; i < descriptors.Length; ++i)
            {
                PropertyDescriptor descriptor = descriptors[i];
                if (descriptor.MemberInfo == EnabledProperty)
                {
                    continue;
                }
                BuildPropertyEditor(descriptor);
            }
        }


        protected virtual void BuildPropertyEditor(PropertyDescriptor descriptor)
        {
            PropertyEditor editor = InstantiatePropertyEditor(descriptor);
            if (editor == null)
            {
                return;
            }
            if (descriptor.Range != null)
            {
                if(descriptor.Range is RangeInt)
                {
                    RangeIntEditor rangeEditor = editor as RangeIntEditor;
                    rangeEditor.Min = (int)descriptor.Range.Min;
                    rangeEditor.Max = (int)descriptor.Range.Max;
                }
                else if(descriptor.Range is RangeOptions)
                {
                    RangeOptions range = (RangeOptions)descriptor.Range;
                    OptionsEditor optionsEditor = editor as OptionsEditor;
                    optionsEditor.Options = range.Options;
                }
                else
                {
                    RangeEditor rangeEditor = editor as RangeEditor;
                    rangeEditor.Min = descriptor.Range.Min;
                    rangeEditor.Max = descriptor.Range.Max;
                }   
            }

            InitEditor(editor, descriptor);
        }

        protected virtual void InitEditor(PropertyEditor editor, PropertyDescriptor descriptor)
        {
            editor.Init(descriptor.Target, descriptor.Target, descriptor.MemberInfo, null, descriptor.Label, null, descriptor.ValueChangedCallback, () => { descriptor.EndEditCallback?.Invoke(); EndEditCallback?.Invoke(); }, true, descriptor.ChildDesciptors);
        }

        protected virtual void DestroyEditor()
        {
            DestroyGizmo();
            foreach (Transform t in EditorsPanel)
            {
                Destroy(t.gameObject);
            }
        }


        protected virtual void TryCreateGizmo(IComponentDescriptor componentDescriptor)
        {
            if (componentDescriptor != null && componentDescriptor.GizmoType != null && IsComponentEnabled)
            {
                m_gizmo = Component.gameObject.GetComponent(componentDescriptor.GizmoType);
                if (m_gizmo == null)
                {
                    m_gizmo = Component.gameObject.AddComponent(componentDescriptor.GizmoType);
                }
                m_gizmo.SendMessageUpwards("Reset", SendMessageOptions.DontRequireReceiver);
            }
        }


        protected virtual void DestroyGizmo()
        {
            if (m_gizmo != null)
            {
                DestroyImmediate(m_gizmo);
                m_gizmo = null;
            }
        }


        private PropertyEditor InstantiatePropertyEditor(PropertyDescriptor descriptor)
        {
            if (descriptor.MemberInfo == null)
            {
                Debug.LogError("desciptor.MemberInfo is null");
                return null;
            }

            Type memberType;
            if (descriptor.MemberInfo is MethodInfo)
            {
                memberType = typeof(MethodInfo);
            }
            else
            {
                memberType = descriptor.MemberType;
            }

            if (memberType == null)
            {
                Debug.LogError("descriptor.MemberType is null");
                return null;
            }

            GameObject editorGo = m_editorsMap.GetPropertyEditor(memberType);
            if (editorGo == null)
            {
                return null;
            }

            if (!m_editorsMap.IsPropertyEditorEnabled(memberType))
            {
                return null;
            }
            PropertyEditor editor = editorGo.GetComponent<PropertyEditor>();
            if (editor == null)
            {
                Debug.LogErrorFormat("editor {0} is not PropertyEditor", editorGo);
                return null;
            }
            PropertyEditor instance = Instantiate(editor);
            instance.transform.SetParent(EditorsPanel, false);
            return instance;
        }

        private void OnExpanded(bool expanded)
        {
            IsComponentExpanded = expanded;
            if (expanded)
            {
                IComponentDescriptor componentDescriptor = GetComponentDescriptor();
                PropertyDescriptor[] descriptors = m_editorsMap.GetPropertyDescriptors(Component.GetType(), this, m_converter);
                if(ExpanderGraphics != null)
                {
                    ExpanderGraphics.SetActive(true);
                }
                
                BuildEditor(componentDescriptor, descriptors);
            }
            else
            {
                DestroyEditor();
            }
        }

        private PropertyEditor GetPropertyEditor(MemberInfo memberInfo)
        {
            foreach(Transform t in EditorsPanel)
            {
                PropertyEditor propertyEditor = t.GetComponent<PropertyEditor>();
                if(propertyEditor != null && propertyEditor.MemberInfo == memberInfo)
                {
                    return propertyEditor;
                }
            }
            return null;
        }

        private void OnRedoCompleted()
        {
            ReloadEditors(false);
        }

        private void OnUndoCompleted()
        {
            ReloadEditors(false);
        }

        private void OnReloadComponentEditor(ExposeToEditor obj, Component component, bool force)
        {
            if(Component == component)
            {
                ReloadEditors(force);
            }
        }

        private void ReloadEditors(bool force)
        {
            foreach (Transform t in EditorsPanel)
            {
                PropertyEditor propertyEditor = t.GetComponent<PropertyEditor>();
                if (propertyEditor != null)
                {
                    propertyEditor.Reload(force);
                }
            }
        }

        protected virtual void OnResetClick()
        {
            GameObject go = new GameObject();
            go.SetActive(false);

            Component component = go.GetComponent(Component.GetType());
            if (component == null)
            {
                component = go.AddComponent(Component.GetType());
            }

            bool isMonoBehavior = component is MonoBehaviour;

            PropertyDescriptor[] descriptors = m_editorsMap.GetPropertyDescriptors(Component.GetType(), this, m_converter);
            for (int i = 0; i < descriptors.Length; ++i)
            {
                PropertyDescriptor descriptor = descriptors[i];
                MemberInfo memberInfo = descriptor.ComponentMemberInfo;
                if(memberInfo is PropertyInfo)
                {
                    PropertyInfo p = (PropertyInfo)memberInfo;
                    object defaultValue = p.GetValue(component, null);
                    m_editor.Undo.BeginRecordValue(Component, memberInfo);
                    p.SetValue(Component, defaultValue, null);
                }
                else
                {
                    if (isMonoBehavior)
                    {
                        if(memberInfo is FieldInfo)
                        {
                            FieldInfo f = (FieldInfo)memberInfo;
                            object defaultValue = f.GetValue(component);
                            m_editor.Undo.BeginRecordValue(Component, memberInfo);
                            f.SetValue(Component, defaultValue);
                        }
                    }
                }
                
            }

            for (int i = 0; i < descriptors.Length; ++i)
            {
                PropertyDescriptor descriptor = descriptors[i];
                MemberInfo memberInfo = descriptor.MemberInfo;
                PropertyEditor propertyEditor = GetPropertyEditor(memberInfo);
                if (propertyEditor != null)
                {
                    propertyEditor.Reload();
                }
            }

            Destroy(go);

            m_editor.Undo.BeginRecord();

            for (int i = 0; i < descriptors.Length; ++i)
            {
                PropertyDescriptor descriptor = descriptors[i];
                MemberInfo memberInfo = descriptor.ComponentMemberInfo;
                if (memberInfo is PropertyInfo)
                {
                    m_editor.Undo.EndRecordValue(Component, memberInfo);
                }
                else
                {
                    if(isMonoBehavior)
                    {
                        m_editor.Undo.EndRecordValue(Component, memberInfo);
                    }
                }
            }

            m_editor.Undo.EndRecord();
        }

        protected virtual void OnRemove()
        {
            PropertyDescriptor[] descriptors = m_editorsMap.GetPropertyDescriptors(Component.GetType(), this, m_converter);
            Editor.Undo.DestroyComponent(Component, descriptors.Where(d => d.Target == (object)Component).Select(d => d.ComponentMemberInfo).ToArray());
        }
    }

}
