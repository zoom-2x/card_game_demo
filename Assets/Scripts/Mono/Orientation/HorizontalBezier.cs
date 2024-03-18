using System.Collections;
using System.Collections.Generic;

using CardGame.Data;

using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace CardGame.Mono.Orientation
{
    public class HorizontalBezier : CardOrientation
    {
        [HideInInspector] public OrientationAlignment alignment = OrientationAlignment.CENTER;

        public Vector3 start_point = Vector3.zero;
        public Vector3 end_point = Vector3.zero;

        public float start_angle = -20.0f;
        public float end_angle = 20.0f;
        public float curvature = 0.5f;

        public float tightness_start = 0.8f;
        public float tightness_end = 0.5f;
        
        float length = 0;
        Bezier curve = new Bezier();

        void Start() 
        {
            absolute = false;
            recommended_count = 10;

            Vector3 start_rotation_vector = Quaternion.Euler(0, start_angle, 0) * Vector3.right;
            Vector3 end_rotation_vector = Quaternion.Euler(0, end_angle, 0) * (-Vector3.right);

            Vector3 c0 = start_point + start_rotation_vector.normalized * curvature;
            Vector3 c1 = end_point + end_rotation_vector.normalized * curvature;

            curve.setup(start_point, end_point, c0, c1);
            length = curve.length();
        }

        protected float multiplier(float x) {
            return (tightness_end - tightness_start) * x + tightness_start;
        }

        public override float spacing() {
            return spacing_t() * length;
        }

        public override float spacing_t() {
            return multiplier((float) virtual_count / recommended_count) * (Constants.CARD_WIDTH / length);
        }

        public override Vector3 position(int index) {
            return position_t(relative_position(index));
        }

        public override Vector3 position_t(float t) {
            return curve.compute_point_synchronized(t);
        }

        public override float rotation(int index) {
            return rotation_t(relative_position(index));
        }

        public override float rotation_t(float t) {
            return Mathf.LerpUnclamped(start_angle, end_angle, t);
        }

        public override float relative_position(int index)
        {
            float t = 0;
            float spacing = spacing_t();

            if (alignment == OrientationAlignment.CENTER)
            {
                int middle = (int) (virtual_count * 0.5f);

                if (virtual_count % 2 == 0)
                {
                    if (index < middle)
                        t = 0.5f - spacing * 0.5f - spacing * (middle - index - 1);
                    else
                        t = 0.5f + spacing * 0.5f + spacing * (index - middle);
                }
                else
                {
                    if (index == middle) {
                        t = 0.5f;
                    }
                    else
                    {
                        if (index < middle)
                            t = 0.5f - spacing * (middle - index);
                        else
                            t = 0.5f + spacing * (index - middle);
                    }
                }
            }
            else if (alignment == OrientationAlignment.LEFT) {
                t = Mathf.Clamp01(0.0f + index * spacing);
            }
            else if (alignment == OrientationAlignment.RIGHT) {
                t = Mathf.Clamp01(1.0f - index * spacing);
            }

            return t;
        }

        #if UNITY_EDITOR
        void OnDrawGizmosSelected()
        {
            Vector3 start_rotation_vector = Quaternion.Euler(0, start_angle, 0) * Vector3.right;
            Vector3 end_rotation_vector = Quaternion.Euler(0, end_angle, 0) * (-Vector3.right);

            Vector3 c0 = start_point + start_rotation_vector.normalized * curvature;
            Vector3 c1 = end_point + end_rotation_vector.normalized * curvature;

            Vector3 tp0 = transform.TransformPoint(start_point);
            Vector3 tp1 = transform.TransformPoint(end_point);
            Vector3 tcp0 = transform.TransformPoint(c0);
            Vector3 tcp1 = transform.TransformPoint(c1);

            Gizmos.DrawLine(tp0, tcp0);
            Gizmos.DrawLine(tp1, tcp1);

            Gizmos.DrawSphere(tp0, 0.02f);
            Gizmos.DrawSphere(tp1, 0.02f);

            Gizmos.DrawSphere(tcp0, 0.02f);
            Gizmos.DrawSphere(tcp1, 0.02f);

            Handles.DrawBezier(tp0, tp1, tcp0, tcp1, Color.white, null, 2.0f);
        }
        #endif
    }
}
