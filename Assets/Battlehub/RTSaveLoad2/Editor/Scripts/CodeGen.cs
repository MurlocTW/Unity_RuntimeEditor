﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using UnityObject = UnityEngine.Object;

namespace Battlehub.RTSaveLoad2
{
    /// <summary>
    /// This class is responsible for code generation of persistent objects (surrogates) 
    /// </summary>
    public class CodeGen
    {
        /// <summary>
        /// Automatically generated fields have ProtoMember tag offset = 256. 1 - 256 is reserved for user defined fields.
        /// User defined fields should be located in auto-generated partial class.
        /// </summary>
        private const int AutoFieldTagOffset = 256;

        /// <summary>
        /// Subclass offset which is used in TypeModel creator code. 
        /// (1024 value means, that there is 1024 - 256 - 1 = 767 slots available for auto-generated fields
        /// </summary>
        private const int SubclassOffset = 1024;

        /// <summary>
        /// For text formatting
        /// </summary>
        private static readonly string BR = Environment.NewLine;
        private static readonly string END = BR + BR;
        private static readonly string TAB = "    ";
        private static readonly string TAB2 = "        ";
        private static readonly string TAB3 = "            ";
        private static readonly string SEMICOLON = ";";

        /// <summary>
        /// Default namespaces which will be included in all auto-generated classes
        /// </summary>
        private static string[] DefaultNamespaces =
        {
            "System.Collections.Generic",
            "ProtoBuf",
            "Battlehub.RTSaveLoad2"
        };

        //Templates
        private static readonly string PersistentClassTemplate =
            "{0}" + BR +
            "using UnityObject = UnityEngine.Object;" + BR +
            "namespace {1}" + BR +
            "{{" + BR +
            "    [ProtoContract(AsReferenceDefault = true)]" + BR +
            "    public class {2} : {3}" + BR +
            "    {{" + BR +
            "        {4}" +
            "    }}" + BR +
            "}}" + END;

        private static readonly string FieldTemplate =
            "[ProtoMember({0})]" + BR + TAB2 +
            "public {1} {2};" + END + TAB2;

        private static readonly string ReadFromMethodTemplate =
            "protected override void ReadFromImpl(object obj)" + BR + TAB2 +
            "{{" + BR + TAB2 +
            "    base.ReadFromImpl(obj);" + BR + TAB2 +
            "    {1} uo = ({1})obj;" + BR + TAB2 +
            "{0}" +
            "}}" + BR;

        private static readonly string WriteToMethodTemplate =
            "protected override object WriteToImpl(object obj)" + BR + TAB2 +
            "{{" + BR + TAB2 +
            "    obj = base.WriteToImpl(obj);" + BR + TAB2 +
            "    {1} uo = ({1})obj;" + BR + TAB2 +
            "{0}" +
            "    return obj;" + BR + TAB2 +
            "}}" + BR;

        private static readonly string GetDepsMethodTemplate =
            "protected override void GetDepsImpl(GetDepsContext context)" + BR + TAB2 +
            "{{" + BR + TAB2 +
            "{0}" +
            "}}" + BR;

        private static readonly string GetDepsFromMethodTemplate =
            "protected override void GetDepsFromImpl(object obj, GetDepsFromContext context)" + BR + TAB2 +
            "{{" + BR + TAB2 +
            "    {1} uo = ({1})obj;" + BR + TAB2 +
            "{0}" +
            "}}" + BR;


        private static readonly string ImplicitOperatorsTemplate =
            "public static implicit operator {0}({1} surrogate)" + BR + TAB2 +
            "{{" + BR + TAB2 +
            "    return ({0})surrogate.WriteTo(new {0}());" + BR + TAB2 +
            "}}" + BR + TAB2 +
                                                                                                  BR + TAB2 +
            "public static implicit operator {1}({0} obj)" + BR + TAB2 +
            "{{" + BR + TAB2 +
            "    {1} surrogate = new {1}();" + BR + TAB2 +
            "    surrogate.ReadFrom(obj);" + BR + TAB2 +
            "    return surrogate;" + BR + TAB2 +
            "}}" + BR;

        private static readonly string TypeModelCreatorTemplate =
            "using ProtoBuf.Meta;" + BR +
            "{0}" + BR +
            "using UnityObject = UnityEngine.Object;" + BR +
            "namespace Battlehub.RTSaveLoad2" + BR +
            "{{" + BR +
            "   public static partial class TypeModelCreator" + BR +
            "   {{" + BR +
            "       static partial void RegisterAutoTypes(RuntimeTypeModel model)" + BR +
            "       {{" + BR +
            "            {1}" + BR +
            "       }}" + BR +
            "   }}" + BR +
            "}}" + END;

        private static readonly string AddTypeTemplate =
            "model.Add(typeof({0}), {1}){2}";

        private static readonly string AddSubtypeTemplate =
            ".AddSubType({1}, typeof({0}))";

        private static readonly string SetSerializationSurrogate =
            ".SetSurrogate(typeof({0}))";

        private static readonly string TypeMapTemplate =
            "{0}" + BR +
            "using UnityObject = UnityEngine.Object;" + BR +
            "namespace Battlehub.RTSaveLoad2" + BR +
            "{{" + BR +
            "    public partial class TypeMap" + BR +
            "    {{" + BR +
            "        public TypeMap()" + BR +
            "        {{" + BR +
            "            {1}" +
            "        }}" + BR +
            "    }}" + BR +
            "}}" + END;

        private static readonly string AddToPersistentTypeTemplate = 
             "m_toPeristentType.Add(typeof({0}), typeof({1}));" + BR;
        private static readonly string AddToUnityTypeTemplate =
             "m_toUnityType.Add(typeof({0}), typeof({1}));" + BR;


        /// <summary>
        /// Short names for primitive types
        /// </summary>
        private static Dictionary<Type, string> m_primitiveNames = new Dictionary<Type, string>
        {
            { typeof(string), "string" },
            { typeof(int), "int" },
            { typeof(long), "long" },
            { typeof(short), "short" },
            { typeof(byte), "byte" },
            { typeof(ulong), "ulong" },
            { typeof(uint), "uint" },
            { typeof(ushort), "ushort" },
            { typeof(char), "char" },
            { typeof(object), "object" },
            { typeof(float), "float" },
            { typeof(double), "double" },
            { typeof(bool), "bool" },
            { typeof(string[]), "string[]" },
            { typeof(long[]), "long[]" },
            { typeof(int[]), "int[]" },
            { typeof(short[]), "short[]" },
            { typeof(byte[]), "byte[]" },
            { typeof(ulong[]), "ulong[]" },
            { typeof(uint[]), "uint[]" },
            { typeof(ushort[]), "ushort[]" },
            { typeof(char[]), "char[]" },
            { typeof(object[]), "object[]" },
            { typeof(float[]), "float[]" },
            { typeof(double[]), "double[]" },
            { typeof(bool[]), "bool[]" },
        };

        /// <summary>
        /// Get all public properties with getter and setter
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public PropertyInfo[] GetProperties(Type type)
        {
            return GetAllProperties(type).Where(p => p.GetGetMethod() != null && p.GetSetMethod() != null).ToArray();
        }

        /// <summary>
        /// Get all public instance declared only properties (excluding indexers)
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public PropertyInfo[] GetAllProperties(Type type)
        {
            return type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).Where(p => p.GetIndexParameters().Length == 0).ToArray();
        }

        /// <summary>
        /// Get all public instance declared only fields
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public FieldInfo[] GetFields(Type type)
        {
            return type.GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
        }

        /// <summary>
        /// Get type which is not subclass of UnityObject and "suitable" to be persistent object
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public Type GetSurrogateType(Type type)
        {
            if (type.IsArray)
            {
                type = type.GetElementType();
            }

            if (!type.IsSubclassOf(typeof(UnityObject)) &&
                 type != typeof(UnityObject) &&
                !type.IsEnum &&
                !type.IsGenericType &&
                !type.IsArray &&
                !type.IsPrimitive &&
                (type.IsPublic || type.IsNestedPublic) &&
                (type.IsValueType || type.GetConstructor(Type.EmptyTypes) != null) &&
                type != typeof(string))
            {
                return type;
            }
            return null;
        }

        /// <summary>
        /// Returns true if type has fields or properties referencing UnityObjects. Search is done recursively.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public bool HasDependencies(Type type)
        {
            return HasDependenciesRecursive(type, new HashSet<Type>());
        }

        private bool HasDependencies(Type type, HashSet<Type> inspectedTypes)
        {
            if (type.IsArray)
            {
                type = type.GetElementType();
            }

            if (inspectedTypes.Contains(type))
            {
                return false;
            }

            inspectedTypes.Add(type);

            PropertyInfo[] properties = GetProperties(type);
            for (int i = 0; i < properties.Length; ++i)
            {
                PropertyInfo property = properties[i];
                if (HasDependenciesRecursive(property.PropertyType, inspectedTypes))
                {
                    return true;
                }
            }

            FieldInfo[] fields = GetFields(type);
            for (int i = 0; i < fields.Length; ++i)
            {
                FieldInfo field = fields[i];
                if (HasDependenciesRecursive(field.FieldType, inspectedTypes))
                {
                    return true;
                }
            }

            return false;
        }

        private bool HasDependenciesRecursive(Type type, HashSet<Type> inspectedTypes)
        {
            if (type.IsArray)
            {
                type = type.GetElementType();
            }

            if (type.IsSubclassOf(typeof(UnityObject)))
            {
                return true;
            }

            Type surrogateType = GetSurrogateType(type);
            if (surrogateType != null)
            {
                return HasDependencies(surrogateType, inspectedTypes);
            }
            return false;
        }

        /// <summary>
        /// Returns type name (including names of nested types)
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public string TypeName(Type type)
        {
            if (type.DeclaringType == null)
                return type.Name;

            return TypeName(type.DeclaringType) + "+" + type.Name;
        }

        public string PreparePersistentTypeName(string typeName)
        {
            return typeName.Replace("+", "Nested");
        }

        public string PrepareMappedTypeName(string typeName)
        {
            return typeName.Replace("+", ".");
        }

        //Generate C# code of TypeMap for selected mappings
        public string CreateTypeMap(PersistentClassMapping[] mappings)
        {
            string usings = CreateUsings(mappings);
            string body = CreateTypeMapBody(mappings);

            return string.Format(TypeMapTemplate, usings, body);
        }

        private string CreateTypeMapBody(PersistentClassMapping[] mappings)
        {
            StringBuilder sb = new StringBuilder();
            for (int m = 0; m < mappings.Length; ++m)
            {
                PersistentClassMapping mapping = mappings[m];
                if (mapping == null)
                {
                    continue;
                }

                if (!mapping.IsEnabled)
                {
                    continue;
                }

                Type mappingType = Type.GetType(mapping.MappedAssemblyQualifiedName);
                if(mappingType == null)
                {
                    Debug.LogWarning("Type " + mapping.MappedAssemblyQualifiedName + " was not found");
                    continue;
                }

                string mappedTypeName = mappingType.Name;
                if (mappedTypeName == "Object")
                {
                    mappedTypeName = "UnityObject";
                }
                sb.AppendFormat(AddToPersistentTypeTemplate, mappedTypeName, mapping.PersistentTypeName);
                sb.Append(TAB3);
                sb.AppendFormat(AddToUnityTypeTemplate, mapping.PersistentTypeName, mappedTypeName);
                sb.Append(TAB3);
            }
            sb.Append(BR);
            return sb.ToString();
        }

        //Generate C# code of TypeModelCreator for selected mappings
        public string CreateTypeModelCreator(PersistentClassMapping[] mappings)
        {
            string usings = CreateUsings(mappings);
            string body = CreateTypeModelCreatorBody(mappings);

            return string.Format(TypeModelCreatorTemplate, usings, body);
        }

        private string CreateTypeModelCreatorBody(PersistentClassMapping[] mappings)
        {
            StringBuilder sb = new StringBuilder();
            for (int m = 0; m < mappings.Length; ++m)
            {
                PersistentClassMapping mapping = mappings[m];
                if (mapping == null)
                {
                    continue;
                }

                if (!mapping.IsEnabled)
                {
                    continue;
                }
                string endOfLine = string.Empty;
                if (mapping.Subclasses != null && mapping.Subclasses.Where(s => s.IsEnabled).Count() > 0)
                {
                    endOfLine = CreateAddSubtypesBody(mapping);
                }

                bool hasSurrogate = false;
                Type mappingType = Type.GetType(mapping.MappedAssemblyQualifiedName);
                if (mappingType == null)
                {
                    Debug.LogWarning("Type " + mapping.MappedAssemblyQualifiedName + " was not found");
                }
                else
                {
                    if (GetSurrogateType(mappingType) != null)
                    {
                        endOfLine += string.Format(SetSerializationSurrogate, PreparePersistentTypeName(mapping.PersistentTypeName));
                        hasSurrogate = true;
                    }
                }

                endOfLine += SEMICOLON + BR + TAB3;

                if (hasSurrogate)
                {
                    sb.AppendFormat(AddTypeTemplate, PrepareMappedTypeName(mapping.MappedTypeName), "false", endOfLine);
                }
                else if (mappingType.IsSubclassOf(typeof(UnityObject)) || mappingType == typeof(UnityObject))
                {
                    sb.AppendFormat(AddTypeTemplate, PreparePersistentTypeName(mapping.PersistentTypeName), "true", endOfLine);
                }

            }

            return sb.ToString();
        }

        private string CreateAddSubtypesBody(PersistentClassMapping mapping)
        {
            StringBuilder sb = new StringBuilder();
            PersistentSubclass[] subclasses = mapping.Subclasses.Where(sc => sc.IsEnabled).ToArray();
            for (int i = 0; i < subclasses.Length - 1; ++i)
            {
                PersistentSubclass subclass = mapping.Subclasses[i];
                if(subclass.IsEnabled)
                {
                    sb.Append(BR + TAB3 + TAB);
                    sb.AppendFormat(AddSubtypeTemplate, subclass.TypeName, subclass.PersistentTag + SubclassOffset);
                }
            }

            if (subclasses.Length > 0)
            {
                if(mapping.Subclasses[subclasses.Length - 1].IsEnabled)
                {
                    PersistentSubclass subclass = mapping.Subclasses[subclasses.Length - 1];
                    sb.Append(BR + TAB3 + TAB);
                    sb.AppendFormat(AddSubtypeTemplate, subclass.TypeName, subclass.PersistentTag + SubclassOffset);
                }
            }

            return sb.ToString();
        }


        /// <summary>
        /// Generate C# code of persistent class using persistent class mapping
        /// </summary>
        /// <param name="mapping"></param>
        /// <returns></returns>
        public string CreatePersistentClass(PersistentClassMapping mapping)
        {
            if (mapping == null)
            {
                throw new ArgumentNullException("mapping");
            }
            string usings = CreateUsings(mapping);
            string ns = mapping.PersistentNamespace;
            string className = PreparePersistentTypeName(mapping.PersistentTypeName);
            string baseClassName = mapping.PersistentBaseTypeName != null ?
                 PreparePersistentTypeName(mapping.PersistentBaseTypeName) : null;
            string body = mapping.IsEnabled ? CreatePersistentClassBody(mapping) : string.Empty;
            return string.Format(PersistentClassTemplate, usings, ns, className, baseClassName, body);
        }

        private string CreatePersistentClassBody(PersistentClassMapping mapping)
        {
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < mapping.PropertyMappings.Length; ++i)
            {
                PersistentPropertyMapping prop = mapping.PropertyMappings[i];
                if (!prop.IsEnabled)
                {
                    continue;
                }

                string typeName;
                Type repacementType = GetReplacementType(prop.MappedType);
                if (repacementType != null)
                {
                    string primitiveTypeName;
                    if (m_primitiveNames.TryGetValue(repacementType, out primitiveTypeName))
                    {
                        typeName = primitiveTypeName;
                    }
                    else
                    {
                        typeName = PrepareMappedTypeName(repacementType.Name);
                    }
                }
                else
                {
                    string primitiveTypeName;
                    if (m_primitiveNames.TryGetValue(prop.MappedType, out primitiveTypeName))
                    {
                        typeName = primitiveTypeName;
                    }
                    else
                    {
                        typeName = PreparePersistentTypeName(prop.PersistentTypeName);
                    }
                }

                sb.AppendFormat(
                    FieldTemplate, i + AutoFieldTagOffset,
                    typeName,
                    prop.PersistentName);
            }

            string readMethodBody = CreateReadMethodBody(mapping);
            string writeMethodBody = CreateWriteMethodBody(mapping);
            string getDepsMethodBody = CreateDepsMethodBody(mapping);
            string getDepsFromMethodBody = CreateDepsFromMethodBody(mapping);

            string mappedTypeName = PrepareMappedTypeName(mapping.MappedTypeName);
            if (mappedTypeName == "Object")
            {
                mappedTypeName = "UnityObject";
            }

            if (!string.IsNullOrEmpty(readMethodBody))
            {
                sb.AppendFormat(ReadFromMethodTemplate, readMethodBody, mappedTypeName);
            }
        
            if (!string.IsNullOrEmpty(writeMethodBody))
            {
                sb.Append(BR + TAB2);
                sb.AppendFormat(WriteToMethodTemplate, writeMethodBody, mappedTypeName);
            }

            if (!string.IsNullOrEmpty(getDepsMethodBody))
            {
                sb.Append(BR + TAB2);
                sb.AppendFormat(GetDepsMethodTemplate, getDepsMethodBody);
            }
            if (!string.IsNullOrEmpty(getDepsFromMethodBody))
            {
                sb.Append(BR + TAB2);
                sb.AppendFormat(GetDepsFromMethodTemplate, getDepsFromMethodBody, mappedTypeName);
            }


            Type mappingType = Type.GetType(mapping.MappedAssemblyQualifiedName);
            if (mappingType.GetConstructor(Type.EmptyTypes) != null || mappingType.IsValueType)
            {
                sb.Append(BR + TAB2);
                sb.AppendFormat(ImplicitOperatorsTemplate, mappedTypeName, PreparePersistentTypeName(mapping.PersistentTypeName));
            }
           
            return sb.ToString();
        }

        private string CreateReadMethodBody(PersistentClassMapping mapping)
        {
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < mapping.PropertyMappings.Length; ++i)
            {
                PersistentPropertyMapping prop = mapping.PropertyMappings[i];
                if (!prop.IsEnabled)
                {
                    continue;
                }

                sb.Append(TAB);

                
                if(prop.MappedType.IsSubclassOf(typeof(UnityObject)) || prop.MappedType.IsArray && prop.MappedType.GetElementType().IsSubclassOf(typeof(UnityObject)))
                {
                    //generate code which will convert unity object to identifier
                    sb.AppendFormat("{0} = ToID(uo.{1});", prop.PersistentName, prop.MappedName);
                }
                else
                {
                    sb.AppendFormat("{0} = uo.{1};", prop.PersistentName, prop.MappedName);
                }

                sb.Append(BR + TAB2);
            }

            return sb.ToString();
        }

        private string CreateWriteMethodBody(PersistentClassMapping mapping)
        {
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < mapping.PropertyMappings.Length; ++i)
            {
                PersistentPropertyMapping prop = mapping.PropertyMappings[i];
                if (!prop.IsEnabled)
                {
                    continue;
                }

                sb.Append(TAB);
 
                if (prop.MappedType.IsSubclassOf(typeof(UnityObject)) || prop.MappedType.IsArray && prop.MappedType.GetElementType().IsSubclassOf(typeof(UnityObject)))
                {
                    //generate code which will convert identifier to unity object

                    Type mappedType = prop.MappedType.IsArray ? prop.MappedType.GetElementType() : prop.MappedType;
                    sb.AppendFormat("uo.{0} = FromID<{2}>({1});", prop.MappedName, prop.PersistentName, PrepareMappedTypeName(mappedType.Name));
                }
                else
                {
                    sb.AppendFormat("uo.{0} = {1};", prop.MappedName, prop.PersistentName);
                }

                sb.Append(BR + TAB2);
            }

            return sb.ToString();
        }

        /// <summary>
        /// Generate method which will populate context with dependencies (referenced unity object identifiers)
        /// </summary>
        /// <param name="mapping"></param>
        /// <returns></returns>
        private string CreateDepsMethodBody(PersistentClassMapping mapping)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < mapping.PropertyMappings.Length; ++i)
            {
                PersistentPropertyMapping prop = mapping.PropertyMappings[i];
                if (!prop.IsEnabled)
                {
                    continue;
                }

                if (prop.HasDependenciesOrIsDependencyItself)
                {
                    if (prop.UseSurrogate)
                    {
                        sb.Append(TAB);
                        sb.AppendFormat("AddSurrogateDeps({0}, context);", prop.PersistentName);
                        sb.Append(BR + TAB2);
                    }
                    else if (prop.MappedType.IsSubclassOf(typeof(UnityObject)) || prop.MappedType.IsArray && prop.MappedType.GetElementType().IsSubclassOf(typeof(UnityObject)))
                    {
                        sb.Append(TAB);
                        sb.AppendFormat("AddDep({0}, context);", prop.PersistentName);
                        sb.Append(BR + TAB2);
                    }
                }    
            }
            return sb.ToString();
        }

        /// <summary>
        /// Generate method which will extract and populate context with dependencies (referenced unity objects)
        /// </summary>
        /// <param name="mapping"></param>
        /// <returns></returns>
        private string CreateDepsFromMethodBody(PersistentClassMapping mapping)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < mapping.PropertyMappings.Length; ++i)
            {
                PersistentPropertyMapping prop = mapping.PropertyMappings[i];
                if (!prop.IsEnabled)
                {
                    continue;
                }
                if (prop.HasDependenciesOrIsDependencyItself)
                {
                    if (prop.UseSurrogate)
                    {
                        sb.Append(TAB);
                        sb.AppendFormat("AddSurrogateDeps(uo.{0}, context);", prop.MappedName);
                        sb.Append(BR + TAB2);
                    }
                    if (prop.MappedType.IsSubclassOf(typeof(UnityObject)) || prop.MappedType.IsArray && prop.MappedType.GetElementType().IsSubclassOf(typeof(UnityObject)))
                    {
                        sb.Append(TAB);
                        sb.AppendFormat("AddDep(uo.{0}, context);", prop.MappedName);
                        sb.Append(BR + TAB2);
                    }
                }
            }
            return sb.ToString();
        }

        private string CreateUsings(params PersistentClassMapping[] mappings)
        {
            StringBuilder sb = new StringBuilder();
            HashSet<string> namespaces = new HashSet<string>();

            for (int m = 0; m < mappings.Length; ++m)
            {
                PersistentClassMapping mapping = mappings[m];
                if (mapping == null)
                {
                    continue;
                }

                for (int i = 0; i < DefaultNamespaces.Length; ++i)
                {
                    namespaces.Add(DefaultNamespaces[i]);
                }

                if (!namespaces.Contains(mapping.MappedNamespace))
                {
                    namespaces.Add(mapping.MappedNamespace);
                }

                if (!namespaces.Contains(mapping.PersistentNamespace))
                {
                    namespaces.Add(mapping.PersistentNamespace);
                }

                if (!namespaces.Contains(mapping.PersistentBaseNamespace))
                {
                    namespaces.Add(mapping.PersistentBaseNamespace);
                }

                if(mapping.IsEnabled)
                {
                    for (int i = 0; i < mapping.PropertyMappings.Length; ++i)
                    {
                        PersistentPropertyMapping propertyMapping = mapping.PropertyMappings[i];
                        if (!propertyMapping.IsEnabled)
                        {
                            continue;
                        }
                        if (!namespaces.Contains(propertyMapping.MappedNamespace))
                        {
                            namespaces.Add(propertyMapping.MappedNamespace);
                        }

                        Type type = propertyMapping.MappedType;
                        Type replacementType = GetReplacementType(type);
                        if (replacementType != null)
                        {
                            if (!namespaces.Contains(replacementType.Namespace))
                            {
                                namespaces.Add(replacementType.Namespace);
                            }
                        }
                        else
                        {
                            if(!type.FullName.Contains("System"))
                            {
                                if (!namespaces.Contains(propertyMapping.PersistentNamespace))
                                {
                                    namespaces.Add(propertyMapping.PersistentNamespace);
                                }
                            }
                        }
                    }
                }
            }
            foreach (string ns in namespaces)
            {
                sb.Append("using " + ns + ";" + BR);
            }

            return sb.ToString();
        }

        private Type GetReplacementType(Type type)
        {
            if(type.IsArray)
            {
                Type elementType = type.GetElementType();
                if(elementType.IsSubclassOf(typeof(UnityObject)))
                {
                    return typeof(long[]);
                }
            }

            if(type.IsSubclassOf(typeof(UnityObject)))
            {
                return typeof(long);
            }
            return null;
        }
    }
}
