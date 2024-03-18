using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProceduralLineNode : MonoBehaviour
{
    public bool show_handles = true;

    void OnDrawGizmos()
    {
        if (show_handles)
        {
            Gizmos.color = Color.white;
            Gizmos.DrawSphere(transform.position, 0.05f);
        }
    }
}