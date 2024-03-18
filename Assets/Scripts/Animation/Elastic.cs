using System.Collections;
using System.Collections.Generic;
using System.Text;

using UnityEngine;

using CardGame.Mono;
using CardGame.Mono.Orientation;
using CardGame.Data;

namespace CardGame.Mono.Animation
{
    public enum ElasticStatus
    {
        ZERO = 1,
        ONE = 2,
        FORWARD = 3,
        BACKWARD = 4,
        STATIC = 5
    }

    public enum ElasticTriggerStatus
    {
        FREE = 0,
        FORWARD = 1,
        BACKWARD = 2
    }

    public class ElasticTriggerV2
    {
        public ElasticTriggerStatus status = ElasticTriggerStatus.FREE;
        public ElasticTriggerOffsetV2[] left_offsets = new ElasticTriggerOffsetV2[5];
        public ElasticTriggerOffsetV2[] right_offsets = new ElasticTriggerOffsetV2[5];

        public bool floating;
        public float delay;

        // 0 = idle, 1 = forward, 2 = backward
        public int anim_state = 0;

        public float anim_floating_time;
        public float anim_floating_t;
        public Vector3 anim_floating_offset;
         
        public float anim_time;
        public float anim_t;
        public Vector3 anim_offset;

        public float computed_y_offset;
        public Vector3 computed_scale;
        public Vector3 computed_rotation;

        public Vector3 delta_offset;
        public Vector3 forward_offset;
        public Vector3 floating_offset;
    }

    public struct ElasticTriggerOffsetV2
    {
        public bool enabled;
        public float time;
        public float t;
        public int index;
        public float offset_val;
    }

    public struct ElasticTriggerAnimation
    {
        // Vertical animation.
        public int state; // 0 - idle / 1 - forward / 2 - backward
        public float time;
        public float t;
        public float scale;
        public Vector3 offset;

        // Selection animation.
        public float selection_time;
        public float selection_t;
        public Vector3 selection_offset;
    }

    public class Elastic : MonoBehaviour, IDynamicBehaviour
    {
        public bool horizontal_offset = false;

        // Maximum amount of cards to be animated.
        public int capacity = 10;

        // Total animation duration (seconds).
        public float duration_sec = 1.0f;

        // Maximum offset amount to shift a card.
        public float magnitude = 0.4f;

        [Range(1, 5)]
        public int offset_count = 1;

        // Maximum 5 offsets, (0, 1) interval, value = offset * magnitude.
        public float[] offsets = new float[5];

        // Fixed offset that will lift a card when the mouse is over.
        public float vertical_offset = 0.1f;
        public float scale = 1.2f;

        [HideInInspector] public int active_index = -1;
        [HideInInspector] public CardOrientation orientation = null;
        [HideInInspector] public CardLocation location = CardLocation.LOCATION_NONE;

        float one_over_duration_sec = 1;
        List<CardMono> cards = new List<CardMono>();

        CardMono last_selection = null;

        ElasticTriggerV2[] trigger_list = new ElasticTriggerV2[10];

        int current_over = -1;

        // ElasticStatus[] target_status;
        // ElasticStatus[] segment_status;
        // float[] target_time;
        // float[] segment_time;
        // float[] segment_magnitude;

        // Controls how much of the magnitude is applied to the segment cards.
        float offset_a = 0.9f;
        // Controls how fast the elasticity diminishes based on the distance from the target card.
        float offset_b = 0.1f;
        // Controls the magnitude multiplier based on the number of cards in the animation,
        // less cards means a lower portion of the magnitude is applied, more cards means a
        // larger portion of the magnitude is applied.
        float offset_c = 0.6f;

        void OnEnable()
        {
            if (capacity <= 0)
                capacity = 10;

            one_over_duration_sec = 1.0f / duration_sec;
            _init_triggers();
        }

        bool _is_card_in_transit(int index)
        {
            if (index < 0 || index >= cards.Count)
                return true;

            CardMono card = cards[index];
            return (card.flags & CardMono.IN_TRANSIT) > 0;
        }

        void _init_triggers()
        {
            for (int i = 0; i < 10; ++i)
            {
                ElasticTriggerV2 trigger = new ElasticTriggerV2();

                trigger.status = ElasticTriggerStatus.FREE;

                // Left offsets.
                for (int j = 0; j < 5; ++j)
                {
                    ElasticTriggerOffsetV2 offset = trigger.left_offsets[j];

                    offset.enabled = false;
                    offset.time = 0;
                    offset.t = 0;
                    offset.index = i - (j + 1);
                    offset.offset_val = -offsets[j] * magnitude;

                    trigger.left_offsets[j] = offset;
                }

                // Right offsets.
                for (int j = 0; j < 5; ++j)
                {
                    ElasticTriggerOffsetV2 offset = trigger.right_offsets[j];

                    offset.enabled = false;
                    offset.time = 0;
                    offset.t = 0;
                    offset.index = i + (j + 1);
                    offset.offset_val = offsets[j] * magnitude;

                    trigger.right_offsets[j] = offset;
                }

                // debug_offsets(trigger);

                trigger.anim_state = 0;
                trigger.anim_time = 0;
                trigger.anim_t = 0;
                trigger.anim_offset = Vector3.zero;

                trigger_list[i] = trigger;
            }
        }

        void _activate_trigger(int i)
        {
            if (i < 0 || i >= 10)
                return;

            ElasticTriggerV2 trigger = trigger_list[i];
            trigger.status = ElasticTriggerStatus.FORWARD;

            // trigger.anim_state = 1;
            trigger.anim_time = 0;
            trigger.anim_t = 0;
            trigger.anim_offset = new Vector3(0, 0, vertical_offset);

            // Adjust the offsets.
            for (int j = 0; j < offset_count; ++j)
            {
                ElasticTriggerOffsetV2 offset = trigger.left_offsets[j];

                float tt = Easing.quart_in_to_out_t(offset.time * one_over_duration_sec);
                offset.time = tt * duration_sec;

                trigger.left_offsets[j] = offset;
            }

            // Right offsets.
            for (int j = 0; j < offset_count; ++j)
            {
                ElasticTriggerOffsetV2 offset = trigger.right_offsets[j];

                float tt = Easing.quart_in_to_out_t(offset.time * one_over_duration_sec);
                offset.time = tt * duration_sec;

                trigger.right_offsets[j] = offset;
            }
        }

        void _deactivate_trigger(int i)
        {
            if (i < 0 || i >= 10)
                return;

            ElasticTriggerV2 trigger = trigger_list[i];
            trigger.status = ElasticTriggerStatus.BACKWARD;

            // trigger.anim_state = 2;
            trigger.anim_time = 0;
            trigger.anim_t = 0;
            trigger.anim_offset = new Vector3(0, 0, vertical_offset);

            // Adjust the offsets.
            for (int j = 0; j < offset_count; ++j)
            {
                ElasticTriggerOffsetV2 offset = trigger.left_offsets[j];

                float tt = Easing.quart_out_to_in_t(offset.time * one_over_duration_sec);
                offset.time = tt * duration_sec;

                trigger.left_offsets[j] = offset;
            }

            // Right offsets.
            for (int j = 0; j < offset_count; ++j)
            {
                ElasticTriggerOffsetV2 offset = trigger.right_offsets[j];

                float tt = Easing.quart_out_to_in_t(offset.time * one_over_duration_sec);
                offset.time = tt * duration_sec;

                trigger.right_offsets[j] = offset;
            }
        }

        void _reset_trigger(int i)
        {
            ElasticTriggerV2 trigger = trigger_list[i];
            trigger.status = ElasticTriggerStatus.FREE;
            // trigger.anim_state = 0;

            // Clear the offsets.
            // Left offsets.
            for (int j = 0; j < offset_count; ++j)
            {
                ElasticTriggerOffsetV2 offset = trigger.left_offsets[j];

                offset.enabled = false;  
                offset.time = 0;
                offset.t = 0;

                trigger.left_offsets[j] = offset;
            }

            // Right offsets.
            for (int j = 0; j < offset_count; ++j)
            {
                ElasticTriggerOffsetV2 offset = trigger.right_offsets[j];

                offset.enabled = false;  
                offset.time = 0;
                offset.t = 0;

                trigger.right_offsets[j] = offset;
            }
        }

        void _reset_triggers()
        {
            for (int k = 0; k < trigger_list.Length; ++k) {
                _reset_trigger(k);
            }
        }

        void _trigger_reset_computed()
        {
            for (int i = 0; i < cards.Count; ++i)
            {
                ElasticTriggerV2 trigger = trigger_list[i];
                CardMono card = cards[i];

                trigger.computed_y_offset = 0;
                trigger.computed_scale = Vector3.one;
                trigger.computed_rotation = card.base_rotation;
                trigger.delta_offset = Vector3.zero;
                trigger.forward_offset = Vector3.zero;
                trigger.floating_offset = Vector3.zero;
            }
        }

        void _update_trigger_delay()
        {
            for (int i = 0; i < 10; ++i)
            {
                ElasticTriggerV2 trigger = trigger_list[i];

                if (trigger.delay > 0)
                {
                    trigger.delay -= Time.deltaTime;

                    if (trigger.delay < 0)
                        trigger.delay = 0;
                }
            }
        }

        void _trigger_forward(int i)
        {
            if (i < 0 || i >= cards.Count)
                return;

            ElasticTriggerV2 trigger = trigger_list[i];

            trigger.computed_y_offset = 0.1f;
            trigger.forward_offset = new Vector3(0, 0, vertical_offset);
            trigger.computed_scale = new Vector3(scale, 1, scale);
            trigger.computed_rotation = Quaternion.identity.eulerAngles;

            // Left offsets.
            for (int k = 0; k < offset_count; ++k)
            {
                ElasticTriggerOffsetV2 offset = trigger.left_offsets[k];

                if (_is_card_in_transit(offset.index))
                    continue;

                offset.enabled = true;

                if (offset.t < 1.0f)
                {
                    offset.time += Time.deltaTime;
                    offset.t = Mathf.Clamp01(offset.time * one_over_duration_sec);
                }

                float local_t = Easing.ease_out_quart(offset.t);

                if (offset.index >= 0 && offset.index < cards.Count)
                {
                    ElasticTriggerV2 target = trigger_list[offset.index];
                    target.delta_offset += new Vector3(local_t * offset.offset_val, 0, 0);
                }

                trigger.left_offsets[k] = offset;
            }

            // Right offsets.
            for (int k = 0; k < offset_count; ++k)
            {
                ElasticTriggerOffsetV2 offset = trigger.right_offsets[k];

                if (_is_card_in_transit(offset.index))
                    continue;

                offset.enabled = true;

                if (offset.t < 1.0f)
                {
                    offset.time += Time.deltaTime;
                    offset.t = Mathf.Clamp01(offset.time * one_over_duration_sec);
                }

                float local_t = Easing.ease_out_quart(offset.t);

                if (offset.index >= 0 && offset.index < cards.Count)
                {
                    ElasticTriggerV2 target = trigger_list[offset.index];
                    target.delta_offset += new Vector3(local_t * offset.offset_val, 0, 0);
                }

                trigger.right_offsets[k] = offset;
            }
        }

        void _trigger_backward(int i)
        {
            if (i < 0 || i >= cards.Count)
                return;

            ElasticTriggerV2 trigger = trigger_list[i];
            CardMono card = cards[i];

            int status = 0;

            // ----------------------------------------------------------------------------------
            // -- Trigger card animation.
            // ----------------------------------------------------------------------------------

            if (trigger.floating)
            {
                trigger.anim_floating_time -= Time.deltaTime;
                trigger.anim_floating_t = Mathf.Clamp01(trigger.anim_floating_time * one_over_duration_sec);

                float local_t = Easing.ease_in_quart(trigger.anim_floating_t);
                trigger.floating_offset = trigger.anim_floating_offset * local_t;
                trigger.forward_offset = Vector3.zero;

                if (trigger.anim_floating_t == 0)
                    status |= 1;
            }

            else
            {
                trigger.anim_time += Time.deltaTime;
                trigger.anim_t = Mathf.Clamp01(trigger.anim_time * one_over_duration_sec);

                trigger.forward_offset = Utils.ease_out_quart(trigger.anim_offset, Vector3.zero, trigger.anim_t);
                trigger.computed_scale = Utils.ease_out_quart(new Vector3(scale, 1, scale), Vector3.one, trigger.anim_t);
                trigger.computed_rotation = Utils.ease_out_quart(Quaternion.identity.eulerAngles,
                                                                 Utils.extract_euler_direction(card.base_rotation),
                                                                 trigger.anim_t);

                if (trigger.anim_t == 1.0f)
                    status |= 1;
            }

            // ----------------------------------------------------------------------------------
            // -- Trigger offsets animations.
            // ----------------------------------------------------------------------------------

            if (trigger.status == ElasticTriggerStatus.BACKWARD)
            {
                int running_count = 0;

                // Left offsets.
                for (int j = 0; j < offset_count; ++j)
                {
                    ElasticTriggerOffsetV2 offset = trigger.left_offsets[j];

                    if (offset.index < 0 || offset.index >= cards.Count || offset.t == 0.0f || !offset.enabled)
                        continue;
                    
                    offset.time -= Time.deltaTime;
                    offset.t = Mathf.Clamp01(offset.time * one_over_duration_sec);

                    float local_t = Easing.ease_in_quart(offset.t);

                    ElasticTriggerV2 target = trigger_list[offset.index];
                    target.delta_offset += new Vector3(local_t * offset.offset_val, 0, 0);
                    running_count++;

                    trigger.left_offsets[j] = offset;
                }

                // Right offsets.
                for (int j = 0; j < offset_count; ++j)
                {
                    ElasticTriggerOffsetV2 offset = trigger.right_offsets[j];

                    if (offset.index < 0 || offset.index >= cards.Count || offset.t == 0.0f || !offset.enabled)
                        continue;
                    
                    offset.time -= Time.deltaTime;
                    offset.t = Mathf.Clamp01(offset.time * one_over_duration_sec);

                    float local_t = Easing.ease_in_quart(offset.t);

                    ElasticTriggerV2 target = trigger_list[offset.index];
                    target.delta_offset += new Vector3(local_t * offset.offset_val, 0, 0);
                    running_count++;

                    trigger.right_offsets[j] = offset;
                }

                if (running_count == 0)
                    status |= 2;
            }

            if (status == 3)
                trigger.status = ElasticTriggerStatus.FREE;
        }

        // void init_triggers_old()
        // {
        //     for (int i = 0; i < 10; ++i)
        //     {
        //         ElasticTrigger trigger = new ElasticTrigger();
        //         trigger.arr = new ElasticTriggerOffset[2 * offset_count];

        //         for (int k = 0; k < offset_count; ++k)
        //         {
        //             ElasticTriggerOffset off = trigger.arr[k];
        //             off.time = 0;
        //             off.t = 0;
        //             off.offset = 0;
        //         }

        //         triggers[i] = trigger;
        //     }

        //     trigger_offset_scale = new float[2 * offset_count];

        //     for (int i = offset_count - 1, left = 0, right = trigger_offset_scale.Length - 1; i >= 0; --i, ++left, --right)
        //     {
        //         trigger_offset_scale[left] = -offsets[i] * magnitude;
        //         trigger_offset_scale[right] = offsets[i] * magnitude;
        //     }
        // }

        // ElasticTrigger find_trigger(CardMono card)
        // {
        //     if (card != null)
        //     {
        //         for (int i = 0; i < 10; ++i)
        //         {
        //             if (triggers[i].card_id == card.id)
        //                 return triggers[i];
        //         }
        //     }

        //     return null;
        // }

        // ElasticTrigger aquire_trigger(CardMono card)
        // {
        //     for (int i = 0; i < 10; ++i)
        //     {
        //         if (triggers[i].status == ElasticTriggerStatus.FREE)
        //         {
        //             ElasticTrigger trigger = triggers[i];

        //             trigger.card_id = card.id;
        //             trigger.index = card.index;
        //             trigger.status = ElasticTriggerStatus.FORWARD;

        //             // Left side.
        //             for (int j = offset_count - 1, ji = card.index - 1; j >= 0; --j, --ji)
        //             {
        //                 ElasticTriggerOffset off = trigger.arr[j];

        //                 off.index = ji;
        //                 off.time = 0;
        //                 off.t = 0;
        //                 off.offset = trigger_offset_scale[j];

        //                 trigger.arr[j] = off;
        //             }

        //             // Right side.
        //             for (int j = offset_count, ji = card.index + 1; j < trigger.arr.Length; ++j, ++ji)
        //             {
        //                 ElasticTriggerOffset off = trigger.arr[j];

        //                 off.index = ji;
        //                 off.time = 0;
        //                 off.t = 0;
        //                 off.offset = trigger_offset_scale[j];

        //                 trigger.arr[j] = off;
        //             }

        //             return trigger;
        //         }
        //     }

        //     return null;
        // }

        protected float magnitude_func(float t)
        {
            float res = 0;

            if (t < 0.5f) {
                res = 4 * t * t * t;
            }
            else if (t >= 0.5f)
            {
                float tmp = -2 * t + 2;
                res = 1 - tmp * tmp * tmp * 0.5f;
            }

            return res;
        }

        protected float magnitude_filter(float t)
        {
            // return Mathf.Clamp01(offset_a - offset_b * t * t);
            return offset_a * (1.0f - magnitude_func(t + offset_b));
        }

        // Magnitude value is based on the orientation card spacing and a multiplier function.
        protected float get_magnitude()
        {
            float spacing = 0.5f * orientation.spacing_t();
            float mult = (1.0f - offset_c) * magnitude_func(orientation.virtual_count / orientation.recommended_count) + offset_c;

            return spacing * mult;
        }

        protected float get_magnitude_abs()
        {
            float spacing = 0.5f * orientation.spacing();
            float mult = (1.0f - offset_c) * magnitude_func(orientation.virtual_count / orientation.recommended_count) + offset_c;

            return spacing * mult;
        }

        // public void activate_segment_old(int i)
        // {
        //     if (i < 0 || i >= cards.Count)
        //         return;

        //     ElasticTarget t = targets[i];
        //     ElasticSegment s = segments[i];

        //     if (s.status == ElasticStatus.BACKWARD || s.status == ElasticStatus.STATIC)
        //     {
        //         float tt = Easing.quart_in_to_out_t(s.time * one_over_duration_sec);

        //         s.time = tt * duration_sec;
        //         s.status = ElasticStatus.FORWARD;
        //         t.status = ElasticStatus.FORWARD;
        //         active_index = i;

        //         targets[i] = t;
        //         segments[i] = s;
        //     }
        // }

        // public void deactivate_segment(int i)
        // {
        //     if (i < 0 || i >= cards.Count)
        //         return;

        //     ElasticTarget t = targets[i];
        //     ElasticSegment s = segments[i];

        //     if (s.status == ElasticStatus.FORWARD || s.status == ElasticStatus.STATIC)
        //     {
        //         float tt = Easing.quart_out_to_in_t(s.time * one_over_duration_sec);

        //         s.time = tt * duration_sec;
        //         s.status = ElasticStatus.BACKWARD;
        //         t.status = ElasticStatus.BACKWARD;
        //         active_index = -1;

        //         targets[i] = t;
        //         segments[i] = s;
        //     }
        // }

        // NOTE(gabic): Atunci cand se adauga o carte trebuie verificate animatiile.
        public void add_card(CardMono card)
        {
            if (cards.Count < capacity)
            {
                cards.Add(card);
                _reset_trigger(cards.Count - 1);
            }
        }

        public void remove_card(int i)
        {
            if (i >= 0 && i < cards.Count)
            {
                // deactivate_segment(i);
                active_index = -1;
                cards.RemoveAt(i);

                last_selection = null;
                current_over = -1;

                _reset_triggers();
                
                ElasticTriggerV2 trigger = trigger_list[i];
                trigger.delay = 0;

                // ElasticTarget t = targets[i];
                // ElasticSegment s = segments[i];

                // t.time = 0;
                // t.status = ElasticStatus.STATIC;
                // targets[i] = t;

                // s.status = ElasticStatus.BACKWARD;
                // removed_segments.Add(s);

                // for (int k = i; k < cards.Count - 1; ++k)
                // {
                //     ElasticSegment v = segments[k + 1];

                //     v.index = k;
                //     segments[k] = v;
                // }
            }
        }

        public void frame_update()
        {
            if (orientation == null || orientation.absolute)
                return;

            CardPlayEvent ev = GameSystems.input.card_play_event;
            Player p = GameSystems.game.current_player;

            _update_trigger_delay();

            // ----------------------------------------------------------------------------------
            // -- Check for mouse over | selected.
            // ----------------------------------------------------------------------------------

            // A card from this container (deck) was selected.
            if (p.card_selection.card != null && p.card_selection.card.location == location)
            {
                CardMono card_selection = p.card_selection.card;
                ElasticTriggerV2 trigger = trigger_list[card_selection.index];

                trigger.delay = 0.5f;
                last_selection = card_selection;

                Vector3 adjusted_position = p.card_selection.local_position;
                adjusted_position.y = card_selection.base_position.y;

                trigger.anim_floating_offset = adjusted_position - card_selection.base_position;
            }

            // Mouse over card (disabled when a card is selected).
            if (last_selection == null) 
            {
                if (ev.card != null && ev.card.location == location && (ev.events & GameInput.OVER) > 0)
                {
                    ElasticTriggerV2 trigger = trigger_list[ev.card.index];

                    if (!Utils.has_flag(ev.card.flags, CardMono.IN_TRANSIT) && 
                        trigger.delay == 0 && 
                        current_over != ev.card.index)
                    {
                        trigger.floating = false;
                        _deactivate_trigger(current_over);
                        _activate_trigger(ev.card.index);

                        current_over = ev.card.index;
                    }
                }

                else if (current_over >= 0)
                {
                    ElasticTriggerV2 trigger = trigger_list[current_over];

                    _deactivate_trigger(current_over);
                    current_over = -1;
                }
            }

            // Start the floating card return animation.
            if (p.card_selection.card == null && last_selection != null)
            {
                ElasticTriggerV2 trigger = trigger_list[last_selection.index];

                trigger.floating = true;
                trigger.anim_floating_time = duration_sec;
                trigger.anim_floating_t = 1.0f;

                last_selection = null;
                _deactivate_trigger(current_over);
                current_over = -1;
            }

            // ----------------------------------------------------------------------------------
            // -- Update the triggers.
            // ----------------------------------------------------------------------------------

            _trigger_reset_computed();

            for (int i = 0; i < cards.Count; ++i)
            {
                ElasticTriggerV2 trigger = trigger_list[i];

                if (trigger.status == ElasticTriggerStatus.FORWARD)
                    _trigger_forward(i);
                else if (trigger.status == ElasticTriggerStatus.BACKWARD)
                    _trigger_backward(i);
            }

            // ----------------------------------------------------------------------------------
            // -- Apply the triggers.
            // ----------------------------------------------------------------------------------

            for (int i = 0; i < cards.Count; ++i)
            {
                CardMono card = cards[i];
                ElasticTriggerV2 trigger = trigger_list[i];

                if ((card.flags & CardMono.IN_TRANSIT) > 0 || (card.flags & CardMono.INSPECTED) > 0)
                    continue;

                if (i == 9)
                {
                    // Debug.Log($"card9: {card.transform.localPosition} / {card.base_position} / {trigger.delta_offset} / {trigger.forward_offset}");
                    // Debug.Log($"card9: {trigger.status} / {trigger.computed_scale} / {trigger.computed_rotation}");
                }

                Vector3 computed_pos = card.base_position +
                                       trigger.delta_offset +
                                       trigger.forward_offset +
                                       trigger.floating_offset; 

                // if (trigger.anim_state == 2 && i == 6)
                    // Debug.Log($"state 2: {card.index} / {offset_vector} / {selection_offset_vector} / {computed_pos}");

                if (trigger.computed_y_offset > 0)
                    computed_pos.y = trigger.computed_y_offset;

                card.transform.localPosition = computed_pos;
                card.transform.localScale = trigger.computed_scale;
                card.transform.localRotation = Quaternion.Euler(trigger.computed_rotation.x,
                                                                trigger.computed_rotation.y,
                                                                trigger.computed_rotation.z);
            }
        }

        // ----------------------------------------------------------------------------------
        // -- Debugging.
        // ----------------------------------------------------------------------------------

        void debug_offsets(ElasticTriggerV2 trigger)
        {
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < trigger.left_offsets.Length; ++i)
            {
                ElasticTriggerOffsetV2 offset = trigger.left_offsets[i];

                sb.Append(offset.offset_val);
                sb.Append(", ");
            }

            for (int i = 0; i < trigger.right_offsets.Length; ++i)
            {
                ElasticTriggerOffsetV2 offset = trigger.right_offsets[i];

                sb.Append(offset.offset_val);

                if (i < trigger.right_offsets.Length - 1)
                    sb.Append(", ");
            }

            Debug.Log($"trigger offsets: [{sb.ToString()}]");
        }
    }
}
