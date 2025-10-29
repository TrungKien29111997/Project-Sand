using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TrungKien
{
    public class GizmoSquareSize : MonoBehaviour
    {
        [SerializeField] Color gizColor = Color.green;
        public Vector2 size;
        private void OnDrawGizmos()
        {
            Gizmos.color = gizColor;
            Gizmos.DrawWireCube(transform.position, size);
        }
    }
}