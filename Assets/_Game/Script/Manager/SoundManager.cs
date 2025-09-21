using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TrungKien
{
    public class SoundManager : Singleton<SoundManager>
    {
        // public void PlaySound(ESound esound, float delayTime = 0f, float volume = 1f)
        // {
        //     if (esound == ESound.None) return;
        //     AudioSoundObject result = PoolingSystem.Spawn(DataSystem.Instance.prefabSO.dicObjPooling[EPooling.AudioObject], Vector3.zero, Quaternion.identity) as AudioSoundObject;
        //     Fix.DelayedCall(delayTime, () => result.SetClipPlaying(DataSystem.Instance.audioConfig.dicAudioClip[esound], volume));
        // }
        public void PlaySound(AudioClip clip, float delayTime = 0f, float volume = 1f)
        {
            if (clip == null) return;
            AudioSoundObject result = PoolingSystem.Spawn(DataSystem.Instance.prefabSO.dicObjPooling[EPooling.AudioObject], Vector3.zero, Quaternion.identity) as AudioSoundObject;
            Fix.DelayedCall(delayTime, () => result.SetClipPlaying(clip, volume));
        }
    }
}