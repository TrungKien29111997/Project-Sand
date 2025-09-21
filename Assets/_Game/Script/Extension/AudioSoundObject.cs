using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TrungKien
{
    public class AudioSoundObject : PoolingElement
    {
        public AudioSource audioSource;
        public void SetClipPlaying(AudioClip audioClip, float volume = 1f)
        {
            audioSource.clip = audioClip;
            audioSource.volume = volume;
            audioSource.Play();
            Invoke(nameof(Despawn), audioSource.clip.length);
        }
        void Despawn()
        {
            audioSource.clip = null;
            PoolingSystem.Despawn(this);
        }
    }
}