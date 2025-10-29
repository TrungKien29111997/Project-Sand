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
        public float delayFactor = 1.2f;
        public int spawmFactor = 150;
        public Vector2 gravity = new Vector2(-5f, -4f);
        public int maxSandPerBowl = 3;
        public int amountCacheBowl = 5;
        public float timeSandFlyOut = 1f;
        public float timeSandStayInGroud = 0.7f;
        public float timeDelayStartDropSand = 1f;
        public Material sandMaterial;
    }
}