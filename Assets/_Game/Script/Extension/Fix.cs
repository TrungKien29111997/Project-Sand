using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace TrungKien
{
    public static class Fix
    {
        public static Tween DelayedCall(float delay, TweenCallback callback)
        {
            return DOTween.Sequence().AppendInterval(delay).OnStepComplete(callback)
                .SetUpdate(UpdateType.Normal, false)
                .SetAutoKill(autoKillOnCompletion: true);
        }
    }
}

