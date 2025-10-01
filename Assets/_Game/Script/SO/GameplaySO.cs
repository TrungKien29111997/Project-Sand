using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
namespace TrungKien
{
    [CreateAssetMenu(fileName = "GameplaySO", menuName = "TrungKien/GameplaySO")]
    public class GameplaySO : SerializedScriptableObject
    {
        public Color sandEmissiveColor;
        public AudioClip sfxClick, sfxBling;
        public float delayFactor = 0.5f;
        public int spawmFactor = 5000;
    }
}