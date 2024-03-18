using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace CardGame
{
    public struct EaseInterval
    {
        public float x0;
        public float x1;
        public float y0;
        public float y1;
        public float dy;
    }

    public class Easing
    {
        private static int samples = 5;

        private static EaseInterval[] quart_out_to_in_samples;
        private static EaseInterval[] quart_in_to_out_samples;

        public static void init()
        {
            quart_out_to_in_samples = new EaseInterval[samples];
            quart_in_to_out_samples = new EaseInterval[samples];

            float sample_step = 1.0f / samples;

            for (int i = 0; i < samples; ++i)
            {
                EaseInterval interval = quart_out_to_in_samples[i];

                interval.x0 = i * sample_step;
                interval.x1 = (i + 1) * sample_step;

                interval.y0 = Easing.ease_in_quart(interval.x0);
                interval.y1 = Easing.ease_in_quart(interval.x1);
                interval.dy = interval.y1 - interval.y0;

                quart_out_to_in_samples[i] = interval;

                // ----------------------------------------------------------------------------------

                interval = quart_in_to_out_samples[i];

                interval.x0 = i * sample_step;
                interval.x1 = (i + 1) * sample_step;

                interval.y0 = Easing.ease_out_quart(interval.x0);
                interval.y1 = Easing.ease_out_quart(interval.x1);
                interval.dy = interval.y1 - interval.y0;

                quart_in_to_out_samples[i] = interval;
            }
        }

        public static float quart_out_to_in_t(float t)
        {
            float res = 0;
            float tt = Easing.ease_out_quart(t);

            for (int i = 0; i < samples; ++i)
            {
                EaseInterval interval = quart_out_to_in_samples[i];

                if (interval.y0 <= tt && tt <= interval.y1)
                {
                    float interp = (tt - interval.y0) / interval.dy;
                    res = Mathf.Lerp(interval.x0, interval.x1, interp);
                    break;
                }
            }

            return res;
        }

        public static float quart_in_to_out_t(float t)
        {
            float res = 0;
            float tt = Easing.ease_in_quart(t);

            for (int i = 0; i < samples; ++i)
            {
                EaseInterval interval = quart_in_to_out_samples[i];

                if (interval.y0 <= tt && tt < interval.y1)
                {
                    float interp = (tt - interval.y0) / interval.dy;
                    res = Mathf.Lerp(interval.x0, interval.x1, interp);
                    break;
                }
            }

            return res;
        }

        public static float ease_in_quad(float t) {
            return t * t;
        }

        public static float ease_out_quad(float t) {
            return 1 - (1 - t) * (1 - t);
        }

        public static float ease_in_out_quad(float t)
        {
            float res = t;

            if (t < 0.5f)
                res = 2 * t * t;
            else
            {
                float tmp = -2 * t + 2;
                res = 1.0f - tmp * tmp * 0.5f;
            }

            return res;
        }

        public static float ease_in_cubic(float t) {
            return t * t * t;
        }

        public static float ease_out_cubic(float t)
        {
            float q = 1 - t;
            return 1 - q * q * q;
        }

        public static float ease_in_out_cubic(float t)
        {
            float res = t;

            if (t < 0.5f)
                res = 4 * t * t * t;
            else
            {
                float tmp = -2 * t + 2;
                res = 1.0f - tmp * tmp * tmp * 0.5f;
            }

            return res;
        }

        public static float ease_in_quart(float t) {
            return t * t * t * t;
        }

        public static float ease_out_quart(float t)
        {
            float q = 1 - t;
            return 1 - q * q * q * q;
        }

        public static float ease_in_out_quart(float t)
        {
            float res = t;

            if (t < 0.5f)
                res = 8 * t * t * t * t;
            else
            {
                float tmp = -2 * t + 2;
                res = 1.0f - tmp * tmp * tmp * tmp * 0.5f;
            }

            return res;
        }

        public static float ease_out_back3(float t)
        {
            float c0 = 1.25f;
            float c1 = c0 + 1;

            float tt = t - 1;
            float a = tt * tt * tt;
            float b = tt * tt;

            return 1 + c1 * a + c0 * b;
        }

        public static float ease_out_back5(float t)
        {
            float c0 = 0.35f;
            float c1 = c0 + 1;

            float tt = t - 1;
            float a = tt * tt * tt * tt * tt;
            float b = tt * tt;

            return 1 + c1 * a + c0 * b;
        }

        public static float ease_in_back3(float t)
        {
            float c0 = 0.8f;
            float c1 = c0 + 1;

            float a = t * t * t;
            float b = t * t;

            return c1 * a - c0 * b;
        }

        public static float ease_in_back5(float t)
        {
            float c0 = 0.3f;
            float c1 = c0 + 1;

            float a = t * t * t * t * t;
            float b = t * t;

            return c1 * a - c0 * b;
        }
    }
}
