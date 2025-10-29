using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TrungKien
{
    public class GizmoPos : MonoBehaviour
    {
        [SerializeField] Color gizColor = Color.green;
        public float radius;
        private void OnDrawGizmos()
        {
            Gizmos.color = gizColor;
            Gizmos.DrawWireSphere(transform.position, radius);
        }
    }
}