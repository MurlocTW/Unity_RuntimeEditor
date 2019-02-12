using System.Collections.Generic;
using ProtoBuf;
using Battlehub.RTSaveLoad2;
using UnityEngine;
using UnityEngine.Battlehub.SL2;

using UnityObject = UnityEngine.Object;
namespace UnityEngine.Battlehub.SL2
{
    [ProtoContract]
    public partial class PersistentLayerMask : PersistentSurrogate
    {
        
        public static implicit operator LayerMask(PersistentLayerMask surrogate)
        {
            if(surrogate == null) return default(LayerMask);
            return (LayerMask)surrogate.WriteTo(new LayerMask());
        }
        
        public static implicit operator PersistentLayerMask(LayerMask obj)
        {
            PersistentLayerMask surrogate = new PersistentLayerMask();
            surrogate.ReadFrom(obj);
            return surrogate;
        }
    }
}

