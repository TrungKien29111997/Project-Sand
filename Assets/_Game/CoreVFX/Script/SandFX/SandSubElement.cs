using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace TrungKien.Core.VFX.Sand
{
    public class SandSubElement : PoolingElement
    {
        [SerializeField] ParticleSystem ps, subsp;
        [SerializeField] AudioSource auSource;
        public void SetColor(Color color)
        {
            var main = ps.main;
            main.startColor = color;
            var main2 = subsp.main;
            main2.startColor = color;
        }
        public void Play()
        {
            ps.Play();
            subsp.Play();
            auSource.Play();
        }
        public void Stop()
        {
            ps.Stop();
            subsp.Stop();
            auSource.Stop();
        }
        public void SetSize(float size)
        {
            var main = ps.main;
            main.startSize = size;
        }
        public void SetSpeed(float speed)
        {
            // var main = ps.main;
            // main.startSpeed = speed;
        }
    }
}