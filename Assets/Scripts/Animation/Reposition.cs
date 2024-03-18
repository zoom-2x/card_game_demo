using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using CardGame.Data;
using CardGame.Mono;
using CardGame.Managers;
using CardGame.Mono.Orientation;

namespace CardGame.Mono.Animation
{
    public struct RepositionAnimationUnit
    {
        public bool enabled;
        public float time;
        public float t;

        public float end_y_angle;
        public Vector3 position_start;
        public Vector3 position_end;
        public Vector3 rotation_start;
        public Vector3 rotation_end;
    }

    // NOTE(gabic): This animation works only with relative orientations. 
    public class Reposition : MonoBehaviour, IDynamicBehaviour
    {
        public float duration_sec = 1.0f;

        private float one_over_duration_sec = 0;

        [HideInInspector] public int target_count = 0;
        [HideInInspector] public CardOrientation orientation = null;
        [HideInInspector] public CardLocation location = CardLocation.LOCATION_NONE;

        List<CardMono> cards = new List<CardMono>();
        List<Interpolator> interps = new List<Interpolator>();
        RepositionAnimationUnit[] units = new RepositionAnimationUnit[10];
        int incoming_count = 0;

        void OnEnable() 
        {
            if (duration_sec <= 0)
                duration_sec = 1.0f;

            one_over_duration_sec = 1.0f / duration_sec;
        }

        public void add_card(CardMono card)
        {
            cards.Add(card);
            // GameSystems.memory_manager.fill_interpolators(interps, 1);

            // if (incoming_count > 0)
            //     incoming_count--;

            // reset();
        }

        public void remove_card(int i) {
            cards.RemoveAt(i);
        }

        public void signal_incoming() {
            incoming_count++;
        }

        int finished_count = 0;
        bool reposition_request = false;

        public void reset()
        {
            if (orientation == null || orientation.absolute)
                return;

            finished_count = 0;
            reposition_request = true;

            for (int i = 0; i < cards.Count; ++i)
            {
                CardMono card = cards[i];
                RepositionAnimationUnit unit = units[i];

                unit.enabled = false;
                unit.time = 0;
                unit.t = 0;

                // The interpolation is done between the current position 
                // and the base orientation position.
                unit.position_end = orientation.position(i);
                unit.end_y_angle = orientation.rotation(i);

                units[i] = unit;
            }
        }

        public void frame_update()
        {
            if (cards.Count == 0 || orientation == null || orientation.absolute || !reposition_request)
                return;
            
            for (int i = 0; i < cards.Count; ++i)
            {
                CardMono card = cards[i];
                RepositionAnimationUnit unit = units[i];

                if ((card.flags & CardMono.IN_TRANSIT) > 0 || unit.t == 1.0f)
                    continue;

                if (!unit.enabled)
                {
                    unit.enabled = true;

                    Vector3 rotation_start = Utils.extract_euler_direction(card.transform.localRotation.eulerAngles);
                    Vector3 rotation_end = rotation_start;
                    rotation_end.y = unit.end_y_angle;

                    unit.position_start = card.transform.localPosition;
                    unit.rotation_start = rotation_start;
                    unit.rotation_end = rotation_end;
                }

                unit.time += Time.deltaTime;
                unit.t = Mathf.Clamp01(unit.time * one_over_duration_sec);

                float local_t = Easing.ease_out_quad(unit.t);

                card.transform.localPosition = Vector3.Lerp(unit.position_start, unit.position_end, local_t);
                Vector3 rotation = Vector3.Lerp(unit.rotation_start, unit.rotation_end, local_t);
                card.transform.localRotation = Quaternion.Euler(rotation.x, rotation.y, rotation.z);

                // if (card.id == 11)
                    // Debug.Log($"reposition rotation: {unit.rotation_start} / {unit.rotation_end} / {rotation}");

                card.base_position = card.transform.localPosition;
                card.base_rotation = rotation;

                if (unit.t >= 1.0f)
                {
                    unit.t = 1.0f;
                    finished_count++;
                }

                units[i] = unit;
            }

            if (finished_count == cards.Count)
                reposition_request = false;
        }
    }
}
