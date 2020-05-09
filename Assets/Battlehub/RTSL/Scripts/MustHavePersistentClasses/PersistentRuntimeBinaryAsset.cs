using Battlehub.RTSL.Interface;

namespace UnityEngine.Battlehub.SL2
{
    //This class is not serializable
    public class PersistentRuntimeBinaryAsset : PersistentObject
    {
        public byte[] Data;
        public string Ext;

        protected override void ReadFromImpl(object obj)
        {
            base.ReadFromImpl(obj);

            RuntimeBinaryAsset bin = (RuntimeBinaryAsset)obj;
            Data = bin.Data;
            Ext = bin.Ext;
        }

        protected override object WriteToImpl(object obj)
        {
            RuntimeBinaryAsset bin = (RuntimeBinaryAsset)base.WriteToImpl(obj);
            bin.Data = Data;
            bin.Ext = Ext;
            return bin;
        }
    }
}
