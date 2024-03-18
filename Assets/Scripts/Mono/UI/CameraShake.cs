using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraShake: MonoBehaviour
{
    Vector3 camera_base_position;

    float TWO_PI = 2 * Mathf.PI;
    float time = 0;

    // right, up, forward (seconds)
    new public bool enabled = false;
    public Vector3 magnitude = new Vector3(1.0f, 1.0f, 1.0f);
    public Vector3 speed_sec = new Vector3(5, 5, 5);
    Vector3 oo_speed = Vector3.one;

    void Start()
    {
        oo_speed.x = 1.0f / speed_sec.x;
        oo_speed.y = 1.0f / speed_sec.y;
        oo_speed.z = 1.0f / speed_sec.z;

        camera_base_position = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        if (enabled)
        {
            time += Time.deltaTime;
            Vector3 scaled_time = oo_speed * time;

            Vector3 offset_vector = transform.right * Mathf.Sin(TWO_PI * scaled_time.x) * magnitude.x + 
                                    transform.up * Mathf.Cos(TWO_PI * scaled_time.y) * magnitude.y;

            transform.position = camera_base_position + offset_vector;
        }
    }
}
