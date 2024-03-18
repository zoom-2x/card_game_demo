using System.IO;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace CardGame
{
    public class Utils
    {
        public static float lerp(float a, float b, float t) {
            return Mathf.Lerp(a, b, t);
        }

        public static float ease_in_quad(float a, float b, float t) {
            return Mathf.Lerp(a, b, Easing.ease_in_quad(t));
        }

        public static float ease_out_quad(float a, float b, float t) {
            return Mathf.Lerp(a, b, Easing.ease_out_quad(t));
        }

        public static float ease_in_out_quad(float a, float b, float t) {
            return Mathf.Lerp(a, b, Easing.ease_in_out_quad(t));
        }

        public static float ease_in_cubic(float a, float b, float t) {
            return Mathf.Lerp(a, b, Easing.ease_in_cubic(t));
        }

        public static float ease_out_cubic(float a, float b, float t) {
            return Mathf.Lerp(a, b, Easing.ease_out_cubic(t));
        }

        public static float ease_in_out_cubic(float a, float b, float t) {
            return Mathf.Lerp(a, b, Easing.ease_in_out_cubic(t));
        }

        public static float ease_in_quart(float a, float b, float t) {
            return Mathf.Lerp(a, b, Easing.ease_in_quart(t));
        }

        public static float ease_out_quart(float a, float b, float t) {
            return Mathf.Lerp(a, b, Easing.ease_out_quart(t));
        }

        public static Vector3 ease_in_quad(Vector3 a, Vector3 b, float t)
        {
            Vector3 res = Vector3.zero;
            float tt = Easing.ease_in_quad(t);

            res.x = Mathf.Lerp(a.x, b.x, tt);
            res.y = Mathf.Lerp(a.y, b.y, tt);
            res.z = Mathf.Lerp(a.z, b.z, tt);

            return res;
        }

        public static Vector3 ease_out_quad(Vector3 a, Vector3 b, float t)
        {
            Vector3 res = Vector3.zero;
            float tt = Easing.ease_out_quad(t);

            res.x = Mathf.Lerp(a.x, b.x, tt);
            res.y = Mathf.Lerp(a.y, b.y, tt);
            res.z = Mathf.Lerp(a.z, b.z, tt);

            return res;
        }

        public static Vector3 ease_in_cubic(Vector3 a, Vector3 b, float t)
        {
            Vector3 res = Vector3.zero;
            float tt = Easing.ease_in_cubic(t);

            res.x = Mathf.Lerp(a.x, b.x, tt);
            res.y = Mathf.Lerp(a.y, b.y, tt);
            res.z = Mathf.Lerp(a.z, b.z, tt);

            return res;
        }

        public static Vector3 ease_out_cubic(Vector3 a, Vector3 b, float t)
        {
            Vector3 res = Vector3.zero;
            float tt = Easing.ease_out_cubic(t);

            res.x = Mathf.Lerp(a.x, b.x, tt);
            res.y = Mathf.Lerp(a.y, b.y, tt);
            res.z = Mathf.Lerp(a.z, b.z, tt);

            return res;
        }

        public static Vector3 ease_in_quart(Vector3 a, Vector3 b, float t)
        {
            Vector3 res = Vector3.zero;
            float tt = Easing.ease_in_quart(t);

            res.x = Mathf.Lerp(a.x, b.x, tt);
            res.y = Mathf.Lerp(a.y, b.y, tt);
            res.z = Mathf.Lerp(a.z, b.z, tt);

            return res;
        }

        public static Vector3 ease_out_quart(Vector3 a, Vector3 b, float t)
        {
            Vector3 res = Vector3.zero;
            float tt = Easing.ease_out_quart(t);

            res.x = Mathf.Lerp(a.x, b.x, tt);
            res.y = Mathf.Lerp(a.y, b.y, tt);
            res.z = Mathf.Lerp(a.z, b.z, tt);

            return res;
        }

        public static Vector3 ease_out_back3(Vector3 a, Vector3 b, float t) {
            return Vector3.LerpUnclamped(a, b, Easing.ease_out_back3(t));
        }

        public static Vector3 ease_out_back5(Vector3 a, Vector3 b, float t) {
            return Vector3.LerpUnclamped(a, b, Easing.ease_out_back5(t));
        }

        public static Vector3 ease_in_back3(Vector3 a, Vector3 b, float t) {
            return Vector3.LerpUnclamped(a, b, Easing.ease_in_back3(t));
        }

        public static Vector3 ease_in_back5(Vector3 a, Vector3 b, float t) {
            return Vector3.LerpUnclamped(a, b, Easing.ease_in_back5(t));
        }

        public static float ease_in_out_quart(float a, float b, float t) {
            return Mathf.Lerp(a, b, Easing.ease_in_out_quart(t));
        }

        public static Vector3 extract_euler_direction(Vector3 rotation)
        {
            if (rotation.x > 180) rotation.x -= 360;
            if (rotation.y > 180) rotation.y -= 360;
            if (rotation.z > 180) rotation.z -= 360;

            return rotation;
        }

        public static bool has_flag(uint flags, uint flag) {
            return (flags & flag) > 0;
        }

        public static uint set_flag(uint flags, uint flag) 
        {
            flags |= flag;
            return flags;
        }

        public static uint unset_flag(uint flags, uint flag) 
        {
            flags &= ~flag;
            return flags;
        }

        public static uint toggle_flag(uint flags, uint flag) 
        {
            flags ^= flag;
            return flags;
        }

        public static int int_to_layer_mask(int layer) {
            return 1 << layer;
        }

        public static int layer_mask_to_int(int mask)
        {
            int res = 0;

            while (mask > 0)
            {
                mask = mask >> 1;
                res++;
            }

            return res;
        }

        public static bool go_equal(Transform t0, Transform t1) {
            return t0.gameObject.GetInstanceID() == t1.gameObject.GetInstanceID();
        }

        public static Vector3 mouse_screen_to_world(Camera camera, Vector3 rv)
        {
            Vector3 mouse_position = Input.mousePosition;
            Vector3 rv_view = camera.WorldToViewportPoint(rv);

            mouse_position.z = rv_view.z;
            return camera.ScreenToWorldPoint(mouse_position);
        }

        // World position.
        public static void attach_object_to_mouse(Camera camera, Transform t)
        {
            if (t == null)
                return;

            Vector3 mouse_position = Input.mousePosition;
            Vector3 card_view_position = camera.WorldToViewportPoint(t.position);

            mouse_position.z = card_view_position.z;
            Vector3 world_position = camera.ScreenToWorldPoint(mouse_position);
            t.position = world_position;
        }

        public static Vector3 screen_to_plane(Vector3 screen_point, Plane plane, Camera camera)
        {
            Ray ray = camera.ScreenPointToRay(screen_point);
            float dist = 0;
            plane.Raycast(ray, out dist);
            return ray.GetPoint(dist);
        }

        public static string basepath
        {
            get {
                #if UNITY_ENGINE
                    return "Assets";
                #else
                    return Application.dataPath;
                #endif
            }
        }

        public static string fullpath(string filepath)
        {
            #if UNITY_ENGINE
            string full_path = Path.Combine(Utils.basepath, $"{filepath}");
            #else
            string full_path = Path.Combine(Utils.basepath, $"{filepath}");
            #endif

            return full_path;
        }
    }
}
