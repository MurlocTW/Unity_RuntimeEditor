using UnityEngine;

namespace Battlehub.RTEditor
{
    public class AudioListenerComponentDescriptor : ComponentDescriptorBase<AudioListener>
    {
        public override PropertyDescriptor[] GetProperties(ComponentEditor editor, object converter)
        {
            return new PropertyDescriptor[0];
        }
    }
}
