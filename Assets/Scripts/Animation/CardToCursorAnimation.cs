using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using CardGame.Mono;

namespace CardGame.Animation
{
    public class CardToCursorAnimation : AnimationAbstract
    {
        CardMono card = null;
        Vector3 start = Vector3.zero;

        public void setup(CardMono card, float duration)
        {
            this.card = card;

            set_duration(duration);
            start = card.transform.position;

            t = 0;
            time = 0;
        }

        public override bool frame_update()
        {
            time += Time.deltaTime;
            t = time * one_over_duration_sec;

            if (t > 1.0f)
                t = 1.0f;

            if (t >= 0.0f && t <= 1.0f)
            {
                Vector3 end = Utils.mouse_screen_to_world(GameSystems.game.scene_camera, card.transform.position);
                // card.transform.position = Vector3.Lerp(start, end, t);
                card.transform.position = Utils.ease_out_quart(start, end, t); 
            }

            if (t == 1.0f)
            {
                Debug.Log("finished cursor animation");
                return true;
            }

            return false;
        }
    }
}
