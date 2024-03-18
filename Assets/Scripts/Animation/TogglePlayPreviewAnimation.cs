using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using CardGame.Mono;
using CardGame.Data;

namespace CardGame.Animation
{
    public class TogglePlayPreviewAnimation : AnimationAbstract
    {
        public const int CLOSED = 0;
        public const int OPEN = 1;

        public uint state = CLOSED;
        // All computations are done in viewport-space.
        Vector3 offset = new Vector3(0, 2, 0);

        Vector3 start = Vector3.zero;
        Vector3 end = Vector3.zero;
        Transform transform = null;
        Camera camera = null;

        public TogglePlayPreviewAnimation()
        {
            Player p = GameSystems.game.current_player;

            duration_sec = GameConfig.container.play_preview_duration_sec;
            offset = GameConfig.container.play_preview_offset;

            set_duration(duration_sec);

            CardContainerMono play_preview = p.get_container(CardLocation.LOCATION_PLAY_PREVIEW);

            if (play_preview != null)
            {
                transform = play_preview.transform;
                camera = GameObject.Find("MAIN_CAMERA").GetComponent<Camera>();

                Vector3 view_position = camera.WorldToViewportPoint(transform.position);

                start = view_position + offset;
                end = view_position;

                transform.position = camera.ViewportToWorldPoint(start);
                transform.gameObject.SetActive(true);
            }
        }

        public override bool frame_update()
        {
            time += Time.deltaTime;
            t = time * one_over_duration_sec;

            if (t > 1.0f)
                t = 1.0f;

            Vector3 vp = Vector3.zero;

            if (state == CLOSED)
                vp = Utils.ease_out_back5(start, end, t);
            else if (state == OPEN)
                vp = Utils.ease_in_back5(end, start, t);

            transform.position = camera.ViewportToWorldPoint(vp);

            if (t >= 1.0f)
            {
                t = 0;
                time = 0;
                state = (state + 1) % 2;

                return true;
            }

            return false;
        }
    }
}
