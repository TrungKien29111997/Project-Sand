using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace TrungKien.Core.Gameplay
{
    public class TargetObject : BaseTargetObject
    {
        public override void AnimStartLevel(Action callback)
        {
            callback?.Invoke();
        }
    }
}