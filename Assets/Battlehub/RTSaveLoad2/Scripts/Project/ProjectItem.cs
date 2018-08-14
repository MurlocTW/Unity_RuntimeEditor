﻿using ProtoBuf;
using System.Collections.Generic;
using UnityEngine;

namespace Battlehub.RTSaveLoad2
{
    [ProtoContract]
    public class AssetLibraryReference
    {
        [ProtoMember(1)]
        public string Path;

        [ProtoMember(2)]
        public int Ordinal;

    }

    [ProtoContract]
    public class ProjectInfo
    {
        [ProtoMember(1)]
        public string Description;

        [ProtoMember(2)]
        public AssetLibraryReference[] References;

        [ProtoMember(3)]
        public int IdentityCounter;
    }

    [ProtoContract]
    public class ProjectItem
    {
        [ProtoMember(1)]
        public long ItemID;

        [ProtoMember(2)]
        public long ParentItemID;

        [ProtoMember(3)]
        public string Name;

        public ProjectItem Parent;
        public List<ProjectItem> Children; 
    }

    [ProtoContract]
    public class AssetItem : ProjectItem
    {
        [ProtoMember(1)]
        public byte[] PreviewData;

        [ProtoMember(2)]
        public long PersistentID;
    }

}
