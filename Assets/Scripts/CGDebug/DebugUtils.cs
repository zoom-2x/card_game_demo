using System.Text;
using UnityEngine;

namespace CardGame.CGDebug
{
    public class DebugUtils
    {
        public static void log_array_vector2int(Vector2Int[] arr)
        {
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < arr.Length; ++i)
            {
                Vector2Int v = arr[i];
                sb.Append($"[{v.x}, {v.y}]");

                if (i < arr.Length - 1)
                    sb.Append(", ");
            }

            Debug.Log(sb.ToString());
        }
    }
}
