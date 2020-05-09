using UnityEngine;
using Battlehub.Utils;
using System.Reflection;
using Battlehub.RTCommon;

namespace Battlehub.RTEditor
{
    public class AudioSourceComponentDescriptor : ComponentDescriptorBase<AudioSource>
    {
        public override PropertyDescriptor[] GetProperties(ComponentEditor editor, object converter)
        {
            ILocalization lc = IOC.Resolve<ILocalization>();

            MemberInfo clipInfo = Strong.PropertyInfo((AudioSource x) => x.clip, "clip");
            MemberInfo volumeInfo = Strong.PropertyInfo((AudioSource x) => x.volume, "volume");
            MemberInfo loopInfo = Strong.PropertyInfo((AudioSource x) => x.loop, "loop");
            MemberInfo playOnAwake = Strong.PropertyInfo((AudioSource x) => x.playOnAwake, "playOnAwake");

            

            return new[]
            {
                new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_AudioSource_Loop", "Loop"), editor.Component, loopInfo),
                new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_AudioSource_PlayOnAwake", "Play On Awake"), editor.Component, playOnAwake),
                new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_AudioSource_Clip", "Clip"), editor.Component, clipInfo),
                new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_AudioSource_Volume", "Volume"), editor.Component, volumeInfo, volumeInfo,
                    null, new Range(0.0f, 1.0f)) { AnimationPropertyName = "m_Volume" },
            };
        }
    }
}
