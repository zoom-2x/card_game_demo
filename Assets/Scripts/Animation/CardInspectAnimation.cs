using UnityEngine;

using CardGame.Mono;
using CardGame.Data;

namespace CardGame.Animation
{
    public class CardInspectAnimation : AnimationAbstract
    {
        public CardMono card;
        public Vector3 start;
        public Vector3 end;

        // 0 = idle, 1 = forward, 2 = backward
        public int state = 0;

        public CardInspectAnimation()
        {
            state = 0;
            duration_sec = 0.5f;
            set_duration(duration_sec);
        }

        public override bool frame_update()
        {
            if (card == null)
                return true;

            time += Time.deltaTime;
            t = Mathf.Clamp01(time * one_over_duration_sec);

            if (state == 1)
                card.transform.position = Utils.ease_out_quart(start, end, t);
            else if (state == 2)
                card.transform.position = Utils.ease_in_cubic(start, end, t);

            if (t == 1.0f)
            {
                on_finished_event.trigger();
                on_finished_event.clear();

                return true;
            }

            return false;
        }
    }
}
