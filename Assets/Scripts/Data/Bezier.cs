using UnityEngine;

namespace CardGame.Data
{
    public class Bezier
    {
        private struct BezierLUTSample
        {
            public float distance;
            public float t;
        }

        ushort lut_samples = 10;

        public Vector3[] points = new Vector3[2];
        public Vector3[] controls = new Vector3[2];

        BezierLUTSample[] lut = null;

        public Bezier() {}

        public void setup(Vector3 p0, Vector3 p1, Vector3 c0, Vector3 c1)
        {
            points[0] = p0;
            points[1] = p1;

            controls[0] = c0;
            controls[1] = c1;

            generate_lut();
        }

        public Vector3 compute_point(float t)
        {
            Vector3 point = Vector3.zero;

            float a = 1 - t;
            float t1 = a * a * a;
            float t2 = 3 * t * a * a;
            float t3 = 3 * t * t * a;
            float t4 = t * t * t;

            point = t1 * points[0] + t2 * controls[0] + t3 * controls[1] + t4 * points[1];

            return point;
        }

        public Vector3 compute_point_synchronized(float t)
        {
            t = sync_with_lut(t);
            return compute_point(t);
        }

        public float length() {
            return lut[lut.Length - 1].distance;
        }

        protected float sync_with_lut(float t)
        {
            if (lut_samples == 0)
                return 0;

            t = Mathf.Clamp(t, 0.0f, 1.0f);

            float arclen = length();
            float search_distance = t * arclen;

            uint li = 0;
            uint ri = (uint) (lut_samples - 1);

            while (true)
            {
                uint i = li + ((ri - li) >> 1);

                if (li == ri - 1)
                    break;

                BezierLUTSample sample = lut[i];

                if (sample.distance == search_distance)
                {
                    li = ri - 1;
                    break;
                }
                else
                {
                    if (search_distance > sample.distance)
                        li = i;
                    else
                        ri = i;
                }
            }

            BezierLUTSample left_sample = lut[li];
            BezierLUTSample right_sample = lut[ri];

            float t_local = (search_distance - left_sample.distance) / (right_sample.distance - left_sample.distance);
            t = (1.0f - t_local) * left_sample.t + t_local * right_sample.t;

            return t;
        }

        protected void generate_lut()
        {
            float lut_step = 1.0f / (lut_samples - 1);

            if (lut == null)
                lut = new BezierLUTSample[lut_samples];

            Vector3 prev_point = Vector3.zero;
            BezierLUTSample sample = new BezierLUTSample();

            for (ushort i = 0; i < lut_samples; ++i)
            {
                float t = i * lut_step;
                Vector3 point = compute_point(t);

                if (i == 0) {
                    sample.distance = 0;
                }
                else
                {
                    BezierLUTSample prev_sample = lut[i - 1];
                    float distance = Vector3.Distance(prev_point, point);

                    sample.distance = prev_sample.distance + distance;
                }

                sample.t = t;
                lut[i] = sample;

                prev_point = point;
            }
        }

        public void debug_draw_curve(bool tangents = false)
        {}
    }
}