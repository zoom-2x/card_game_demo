using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using CardGame.Data;
using CardGame.Mono;
using CardGame.Managers;

namespace CardGame.Mono.Orientation
{
    public class HorizontalFixed : CardOrientation
    {
        public OrientationAlignment alignment = OrientationAlignment.CENTER;

        // NOTE(gabic): Sa adaug si limitele pentru cazul cand orientarea e
        // la stanga sau la dreapta.

        // public int capacity = 1;
        
        public float placeholder_width = 0.74f;
        public float padding = 0.1f;
        public float left_margin = -1;
        public float right_margin = 1;

        void Start() {}

        public override float spacing() {
            return placeholder_width + padding;
        }

        public override Vector3 position(int i)
        {
            Vector3 pos = Vector3.zero;

            int half = (int) (recommended_count * 0.5f);
            // float half_width = placeholder_width * 0.5f;
            float unit_offset = spacing();

            if (recommended_count % 2 == 0)
            {
                // float t = unit_offset * 0.5f + unit_offset * (half - 1);
                float t = unit_offset * (half - 0.5f);
                // pos.x = -t + i * unit_offset;
                pos.x = unit_offset * (i - half + 0.5f);
            }
            else
            {
                float t = half * unit_offset;
                // pos.x = -t + i * unit_offset;
                pos.x = unit_offset * (i - half);
            }

            return pos;
        }
    }
}
