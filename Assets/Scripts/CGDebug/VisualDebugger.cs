using UnityEngine;
using CardGame.Hexgrid;

namespace CardGame.CGDebug
{
    [ExecuteInEditMode]
    public class VisualDebugger : MonoBehaviour
    {
        public bool map_plane = false;

        [System.NonSerialized] public HexMap map = null;

        void OnEnable()
        {}

        #if UNITY_EDITOR
        void OnDrawGizmos()
        {
            if (map_plane && map != null)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawRay(map.origin, map.plane.normal);
            }
        }
        #endif
    }
}
