using ProtoBuf.Meta;
using System;
using System.IO;
using Battlehub.RTSL.Interface;
namespace Battlehub.RTSL
{  
    [ProtoBuf.ProtoContract]
    public class NilContainer { }

    public class ProtobufSerializer : ISerializer
    {
        private static TypeModel model;

        static ProtobufSerializer()
        {
#if !UNITY_EDITOR
            Type type = Type.GetType("RTSLTypeModel, RTSLTypeModel");

            if(type != null)
            {
                model = Activator.CreateInstance(type) as TypeModel;
            }  

            if(model == null)
            {
                UnityEngine.Debug.LogError("RTSLTypeModel.dll was not found. Please build type model using Tools->Runtime SaveLoad->Build All menu item from Unity Editor");
            }
#endif
            if (model == null)
            {
                model = TypeModelCreator.Create();
            }
            
            model.DynamicTypeFormatting += (sender, args) =>
            {
                if (args.FormattedName == null)
                {
                    return;
                }

                if (Type.GetType(args.FormattedName) == null)
                {
                    args.Type = typeof(NilContainer);
                }
            };

            #if UNITY_EDITOR
            RuntimeTypeModel runtimeTypeModel = model as RuntimeTypeModel;
            if(runtimeTypeModel != null)
            {
                runtimeTypeModel.CompileInPlace();
            }      
            #endif  
        }


        public TData DeepClone<TData>(TData data)
        {
            return (TData)model.DeepClone(data);
        }

        public TData Deserialize<TData>(Stream stream)
        {
            TData deserialized = (TData)model.Deserialize(stream, null, typeof(TData));
            return deserialized;
        }

        public object Deserialize(byte[] b, Type type)
        {
            using (var stream = new MemoryStream(b))
            {
                return model.Deserialize(stream, null, type);
            }
        }

        public object Deserialize(Stream stream, Type type)
        {
            return model.Deserialize(stream, null, type);
        }

        public TData Deserialize<TData>(byte[] b)
        {
            using (var stream = new MemoryStream(b))
            {
                TData deserialized = (TData)model.Deserialize(stream, null, typeof(TData));
                return deserialized;
            }
        }

        public TData Deserialize<TData>(byte[] b, TData obj)
        {
            using (var stream = new MemoryStream(b))
            {
                TData deserialized = (TData)model.Deserialize(stream, obj, typeof(TData));
                return deserialized;
            }
        }

        public void Serialize<TData>(TData data, Stream stream)
        {
            model.Serialize(stream, data);
        }

        public byte[] Serialize<TData>(TData data)
        {
            using (var stream = new MemoryStream())
            {
                model.Serialize(stream, data);
                stream.Flush();
                stream.Position = 0;
                return stream.ToArray();
            }
        }
    }
 }
