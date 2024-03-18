using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using CardGame.Managers;
using CardGame.Mono;
using CardGame.Data;
using CardGame.Mono.Orientation;

namespace CardGame.Animation
{
    internal struct Unit
    {
        public Vector3 start;
        public Vector3 end;
        public float time;
        public float t;
    }

    public class ToggleDiscardPreviewAnimation : AnimationAbstract
    {
        public const int CLOSED = 0;
        public const int OPEN = 1;

        public uint state = CLOSED;

        public float delay = 0;
        public Vector3 offset = Vector3.zero;
        public int count = 0;

        bool initialized = false;
        CardContainerMono discard_preview = null;

        List<Transform> placeholders = new List<Transform>();
        Unit[] units = new Unit[10];

        public ToggleDiscardPreviewAnimation()
        {
            Player p = GameSystems.game.current_player;
            discard_preview = p.get_container(CardLocation.LOCATION_DISCARD_PREVIEW);

            duration_sec = GameConfig.container.discard_preview_duration_sec;
            offset = GameConfig.container.discard_preview_offset;
            delay = GameConfig.container.discard_preview_delay_sec;

            set_duration(duration_sec);
        }

        void initialize()
        {
            initialized = true;
            CardOrientation orientation = discard_preview.orientation;
            orientation.recommended_count = count;

            Player p = GameSystems.game.current_player;

            for (int i = 0; i < count; ++i)
            {
                Transform placeholder = GameSystems.asset_manager.aquire_placeholder(p.id);
                Unit unit = units[i];

                placeholder.parent = discard_preview.transform;
                placeholder.localPosition = orientation.position(i);
                placeholder.localRotation = Quaternion.Euler(90, 0, 0);

                placeholders.Add(placeholder);

                Vector3 position = placeholder.localPosition;
                // Small offset for the card hit detection.
                position.y -= 0.01f;

                Vector3 start_position = position + offset;
                Vector3 end_position = position;

                Debug.Log($"start position: {start_position} / {offset}");

                unit.t = 0;
                unit.time = -delay * i;
                unit.start = start_position;
                unit.end = end_position;

                units[i] = unit;
                placeholder.localPosition = start_position;
            }
        }

        void cleanup()
        {
            initialized = false;

            for (int i = 0; i < count; ++i) {
                GameSystems.asset_manager.return_placeholder(placeholders[i].gameObject);
            }

            placeholders.Clear();
        }

        void reset_animation()
        {
            for (int i = 0; i < count; ++i) 
            {
                Unit unit = units[i];

                unit.time = - i * delay;
                unit.t = 0;
                units[i] = unit;
            }
        }

        public void placeholder_status(int filled)
        {
            if (filled >= 0 && filled <= count)
            {
                for (int i = 0; i < count; ++i)
                {
                    Placeholder placeholder = placeholders[i].gameObject.GetComponent<Placeholder>();

                    if (placeholder != null)
                    {
                        if (i < filled) 
                            placeholder.set_valid();
                        else
                            placeholder.set_invalid();
                    }
                }
            }
        }

        public override bool frame_update()
        {
            if (!initialized)
                initialize();

            int finished_count = 0;

            for (int i = 0; i < count; ++i)
            {
                Unit unit = units[i];
                Transform placeholder = placeholders[i];
                
                unit.time += Time.deltaTime;
                unit.t = unit.time * one_over_duration_sec;

                if (unit.t > 1.0f)
                    unit.t = 1.0f;

                if (unit.t >= 0 && unit.t <= 1.0f)
                {
                    if (state == CLOSED)
                        placeholder.localPosition = Utils.ease_out_back5(unit.start, unit.end, unit.t);
                    else if (state == OPEN)
                        placeholder.localPosition = Utils.ease_in_back5(unit.end, unit.start, unit.t);
                }

                units[i] = unit;

                if (unit.t >= 1.0f)
                    finished_count++;
            }

            if (finished_count == count)
            {
                state = (state + 1) % 2;
                reset_animation();

                if (state == CLOSED)
                    cleanup();

                return true;
            }

            return false;
        }
    }
}

