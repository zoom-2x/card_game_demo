using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using CardGame.Data;

namespace CardGame.Mono.Orientation
{
    public class HorizontalLine : CardOrientation
    {
        public OrientationAlignment alignment = OrientationAlignment.CENTER;

        public Vector3 start_point = Vector3.zero;
        public Vector3 end_point = Vector3.zero;

        public float tightness_start = 0.8f;
        public float tightness_end = 0.5f;
        
        float length = 0;

        void OnEnable() 
        {
            absolute = false;
            recommended_count = 10;

            length = (end_point - start_point).magnitude;
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
            return Vector3.LerpUnclamped(start_point, end_point, t);
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
    }
}
