﻿using System;
using UnityObject = UnityEngine.Object;

namespace Battlehub.RTSaveLoad2.Interface
{
    public interface IUnityObjectFactory
    {
        bool CanCreateInstance(Type type);
        bool CanCreateInstance(Type type, PersistentSurrogate surrogate);

        UnityObject CreateInstance(Type type);
        UnityObject CreateInstance(Type type, PersistentSurrogate surrogate);
    }
}
