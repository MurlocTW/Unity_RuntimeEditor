using Battlehub.RTSL.Interface;

namespace UnityEngine.Battlehub.SL2
{    
    //This class is not serializable
    public class PersistentRuntimeTextAsset : PersistentObject
    {
        public string Text;
        public string Ext;

        protected override void ReadFromImpl(object obj)
        {
            base.ReadFromImpl(obj);

            RuntimeTextAsset text = (RuntimeTextAsset)obj;
            Text = text.Text;
            Ext = text.Ext;
        }

        protected override object WriteToImpl(object obj)
        {
            RuntimeTextAsset text = (RuntimeTextAsset)base.WriteToImpl(obj);
            text.Text = Text;
            text.Ext = Ext;
            return text;
        }
    }
}
