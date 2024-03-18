using UnityEngine;

using CardGame.Mono;
using CardGame.Data;

namespace CardGame.Animation
{
    public class TransferAnimation : AnimationAbstract
    {
        // Animation duration for a single unit (seconds)
        public float threshold = 0;
        // Flag used to signal when the animation has reached a certain point in time.
        public bool threshold_reached = false;

        public Vector3 start_position = Vector3.zero;
        public Vector3 start_scale = Vector3.one;
        public Vector3 start_control_vector = Vector3.zero;
        public Vector3 end_control_vector = Vector3.zero;
        public Vector3 start_rotation = Vector3.zero;
        public Vector3 end_rotation = Vector3.zero;

        public CardMono card = null;
        public Bezier curve = new Bezier();
        public CardContainerMono src = null;
        public CardContainerMono dest = null;

        public TransferAnimation() {
            set_duration(duration_sec);
        }

        public void reset()
        {
            src = null;
            dest = null;

            t = 0;
            time = 0;
            threshold_reached = false;
            // use_cache = false;
        }

        public override bool frame_update()
        {
            float dt = Time.deltaTime;

            time += dt;
            t = Mathf.Clamp01(time * one_over_duration_sec);

            float local_t = Easing.ease_out_cubic(t);

            if (t >= threshold && !threshold_reached)
            {
                threshold_reached = true;
                dest.reposition_reset(card.index + 1);
            }

            card.transform.localPosition = curve.compute_point(local_t);
            
            Vector3 ir = Utils.ease_out_quart(start_rotation, end_rotation, t);
            card.transform.localRotation = Quaternion.Euler(ir.x, ir.y, ir.z);
            card.transform.localScale = Utils.ease_out_quart(start_scale, Vector3.one, t);

            if (t == 1.0f)
            {
                card.flags &= ~CardMono.IN_TRANSIT;
                on_finished_event.trigger();
                on_finished_event.clear();

                return true;
            }

            return false;
        }
    }
}
