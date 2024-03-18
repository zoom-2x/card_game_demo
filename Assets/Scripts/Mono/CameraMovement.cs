using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEditor;

using CardGame;

public class CameraMovement : MonoBehaviour
{
    const int FORWARD = 0;
    const int RIGHT = 1;
    const int BACKWARD = 2;
    const int LEFT = 3;

    [System.NonSerialized] new public static bool enabled = true;

    Camera scene_camera = null;

    [SerializeField] bool show_box = false;
    [SerializeField] bool restrict_box = false;
    [SerializeField] Vector3 origin = Vector3.zero;
    // box lenghts
    [SerializeField] Vector3 box_size = new Vector3(2, 2, 2);

    [SerializeField] [Range(1, 10)] float drag_sensitivity = 1;
    [SerializeField] [Range(0, 1)] float drag_max_velocity = 0.5f;

    [SerializeField] [Range(0, 1)] float max_velocity = 0.3f;
    [SerializeField] [Range(0, 2)] float acceleration = 0.2f;
    [SerializeField] [Range(0, 2)] float friction = 0.01f;

    [SerializeField] float min_zoom = 1.0f;
    [SerializeField] float max_zoom = 20.0f;
    [SerializeField] float camera_zoom = 5;
    [SerializeField] [Range(0, 1)] float zoom_sensitivity = 0.5f;

    bool drag_mode = false;

    Vector3 start_screen_point = Vector3.zero;
    Vector3 end_screen_point = Vector3.zero;
    Vector3 start_point = Vector3.zero;
    Vector3 start_camera_position = Vector3.zero;

    Vector3 plane_point = Vector3.zero;
    Plane plane = new Plane(Vector3.up, Vector3.zero);

    float zoom_velocity = 0;
    float zoom_direction = -1;
    Vector3 zoom_vector = Vector3.zero;

    float oosw = 1;
    float drag_velocity = 0;
    float drag_acceleration = 0;
    Vector3 drag_direction = Vector3.zero;

    // forward / right / backward / left
    string[] key_bindings = new string[4] { "w", "d", "s", "a" };
    float[] dir_acceleration = new float[4];
    float[] dir_velocity = new float[4];
    float[] dir_dp = new float[4];
    Vector3[] dir_vector = new Vector3[4];

    bool[] _key_down = new bool[4];

    public float zoom {
        get { return camera_zoom; }
    }

    void Start()
    {
        scene_camera = gameObject.GetComponent<Camera>();

        if (scene_camera == null)
            Debug.LogWarning("CameraMovement: Missing camera !");

        // NOTE(gabic): oosw = one over screen width
        oosw = 1.0f / (Screen.width * 0.5f);

        camera_zoom_setup();
    }

    void Update()
    {
        if (scene_camera == null || !enabled)
            return;

        // -----------------------------------------------------------
        // -- Camera zoom.
        // -----------------------------------------------------------

        // float mouse_scroll = Input.mouseScrollDelta.y;
        float mouse_scroll = Input.GetAxis("Mouse ScrollWheel");
        float zoom_dp = 0;

        // Inlocuieste cu mouse wheel.
        if (mouse_scroll > 0)
        {
            // Debug.Log($"front: {mouse_scroll}");

            zoom_velocity += mouse_scroll * zoom_sensitivity;
            zoom_direction = -1;
            zoom_vector = scene_camera.transform.forward.normalized;
        }

        else if (mouse_scroll < 0)
        {
            // Debug.Log($"back: {mouse_scroll}");

            zoom_velocity += mouse_scroll * zoom_sensitivity;
            zoom_direction = 1;
            zoom_vector = -scene_camera.transform.forward.normalized;
        }

        if (zoom_velocity > 0)
        {
            zoom_velocity -= friction * Time.deltaTime;
            zoom_velocity = Mathf.Clamp(zoom_velocity, 0, max_velocity);

            zoom_dp = -Time.deltaTime * Time.deltaTime * friction + zoom_velocity;
        }

        else if (zoom_velocity < 0)
        {
            zoom_velocity += friction * Time.deltaTime;
            zoom_velocity = Mathf.Clamp(zoom_velocity, -max_velocity, 0);

            zoom_dp = Time.deltaTime * Time.deltaTime * friction + zoom_velocity;
            zoom_dp *= -1;
        }

        // Debug.Log($"zoom velocity: {zoom_velocity}");

        if (zoom_dp < 0) zoom_dp = 0;

        if (zoom_dp != 0)
        {
            camera_zoom += zoom_dp * zoom_direction;

            if (camera_zoom < min_zoom)
                camera_zoom = min_zoom;
            else if (camera_zoom > max_zoom)
                camera_zoom = max_zoom;
            else
                set_position(scene_camera.transform.position + zoom_vector * zoom_dp);
        }

        // -----------------------------------------------------------
        // -- Mouse drag.
        // -----------------------------------------------------------

        if (Input.GetMouseButton(2))
        {
            drag_acceleration = 0;

            if (!drag_mode)
            {
                drag_mode = true;
                start_point = Input.mousePosition;
            }

            Vector3 end_point = Input.mousePosition;
            drag_velocity = Mathf.Clamp01((end_point - start_point).magnitude * oosw) * drag_sensitivity;

            Vector3 world_start_point = Utils.screen_to_plane(start_point, plane, scene_camera);
            Vector3 world_end_point = Utils.screen_to_plane(end_point, plane, scene_camera);
            drag_direction = world_start_point - world_end_point;

            start_point = end_point;
        }
        else
        {
            drag_mode = false;
            drag_acceleration = -friction;
        }

        // -----------------------------------------------------------
        // -- Keyboard scroll (WASD).
        // -----------------------------------------------------------

        register_input();

        dir_vector[FORWARD] = scene_camera.transform.forward.normalized;
        dir_vector[RIGHT] = scene_camera.transform.right.normalized;
        dir_vector[BACKWARD] = -scene_camera.transform.forward.normalized;
        dir_vector[LEFT] = -scene_camera.transform.right.normalized;

        for (int i = 0; i < 4; ++i) {
            dir_vector[i].y = 0;
        }

        if (!drag_mode)
        {
            for (int i = 0; i < 4; ++i)
            {
                dir_acceleration[i] = -friction;

                if (_key_down[i])
                    dir_acceleration[i] = acceleration;
            }

            // Debug.Log($"{dir_acceleration[0]} / {dir_acceleration[1]} / {dir_acceleration[2]} / {dir_acceleration[3]}");
        }

        // -----------------------------------------------------------
        // -- Keyboard pan position calculation.
        // -----------------------------------------------------------

        for (int i = 0; i < 4; ++i)
        {
            // Reset the existing wasd movement.
            if (drag_mode) {
                dir_velocity[i] = 0;
            }

            else
            {
                dir_velocity[i] += dir_acceleration[i] * Time.deltaTime;
                dir_velocity[i] = Mathf.Clamp(dir_velocity[i], 0, max_velocity);
                dir_dp[i] = Time.deltaTime * Time.deltaTime * dir_acceleration[i] + dir_velocity[i];

                if (dir_dp[i] < 0) dir_dp[i] = 0;

                set_position(scene_camera.transform.position + dir_vector[i] * dir_dp[i]);
            }
        }

        // -----------------------------------------------------------
        // -- Mouse drag position calculation.
        // -----------------------------------------------------------

        if (drag_velocity > 0)
        {
            drag_velocity += drag_acceleration * Time.deltaTime;
            drag_velocity = Mathf.Clamp(drag_velocity, 0, drag_max_velocity);
            float dp = Time.deltaTime * Time.deltaTime * drag_acceleration + drag_velocity;

            if (dp < 0) dp = 0;

            set_position(scene_camera.transform.position + drag_direction.normalized * dp);
        }
    }

    void register_input()
    {
        for (int i = 0; i < 4; ++i)
        {
            if (Input.GetKeyDown(key_bindings[i]))
                _key_down[i] = true;

            if (Input.GetKeyUp(key_bindings[i]))
                _key_down[i] = false;
        }
    }

    public void set_plane(Vector3 normal, Vector3 point)
    {
        plane_point = point;
        plane.SetNormalAndPosition(normal, point);
        camera_zoom_setup();
    }

    public float camera_to_plane_distance()
    {
        float distance = 0;

        Ray ray = new Ray(scene_camera.transform.position, scene_camera.transform.forward);
        plane.Raycast(ray, out distance);

        return distance;
    }

    // Adjust the camera position to correspond to the "camera_zoom" field.
    void camera_zoom_setup()
    {
        float current_zoom = camera_to_plane_distance();
        float zoom_delta = current_zoom - camera_zoom;

        // set_position(scene_camera.transform.position + scene_camera.transform.forward.normalized * zoom_delta);
        scene_camera.transform.position += scene_camera.transform.forward.normalized * zoom_delta;
    }

    void set_position(Vector3 position)
    {
        if (restrict_box)
        {
            Vector3 relative = position - origin;

            if (relative.x > box_size.x)
            {
                position.x = origin.x + box_size.x;
                dir_velocity[RIGHT] = 0;
            }
            else if (relative.x < -box_size.x)
            {
                position.x = origin.x - box_size.x;
                dir_velocity[LEFT] = 0;
            }

            // if (relative.y > box_size.y)
            //     position.y = origin.y + box_size.y;
            // else if (relative.y < -box_size.y)
            //     position.y = origin.y - box_size.y;

            if (relative.z > box_size.z)
            {
                position.z = origin.z + box_size.z;
                dir_velocity[FORWARD] = 0;
            }
            else if (relative.z < -box_size.z)
            {
                position.z = origin.z - box_size.z;
                dir_velocity[BACKWARD] = 0;
            }
        }

        scene_camera.transform.position = position;
    }

    void OnDrawGizmosSelected()
    {
        if (show_box)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(origin, box_size * 2);
        }
    }
}
