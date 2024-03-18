using System.Collections;
using System.Collections.Generic;
// using System.Reflection;
// using System.Diagnostics;
using Debug = UnityEngine.Debug;

using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

using CardGame.Managers;
using CardGame.Mono.Animation;
using CardGame.Mono.Orientation;
using CardGame.Data;
using CardGame.Animation;

namespace CardGame.Mono
{
    public class CardContainerMono : MonoBehaviour
    {
        Renderer border_renderer = null;
        Material material = null;

        [HideInInspector] public List<CardMono> cards = new List<CardMono>();

        public string id = "default_container";

        [HideInInspector] public PlayerID owner = PlayerID.NONE;
        [HideInInspector] public CardLocation location = CardLocation.LOCATION_NONE;

        public MouseEventMask event_mask = MouseEventMask.NONE;        

        // ----------------------------------------------------------------------------------
        // -- Possible container components.
        // ----------------------------------------------------------------------------------

        [HideInInspector] public CardOrientation orientation = null;
        [HideInInspector] public Reposition reposition_animation = null;
        [HideInInspector] public Elastic elastic_animation = null;

        // ----------------------------------------------------------------------------------

        #if UNITY_EDITOR
        // Debugging.
        [HideInInspector] public Bezier debug_curve = null;
        #endif

        // ----------------------------------------------------------------------------------

        void Awake()
        {
            border_renderer = transform.GetChild(0).gameObject.GetComponent<Renderer>();
            material = border_renderer.material;

            orientation = gameObject.GetComponent<CardOrientation>();
            reposition_animation = gameObject.GetComponent<Reposition>();
            elastic_animation = gameObject.GetComponent<Elastic>();

            if (orientation == null)
                Debug.LogWarning($"{id}: missing orientation !");

            if (reposition_animation != null)
            {
                reposition_animation.location = location;
                reposition_animation.orientation = orientation;
            }

            if (elastic_animation != null)
            {
                elastic_animation.location = location;
                elastic_animation.orientation = orientation;
            }
        }

        void OnEnable() {}
        void OnDisable() {}

        void Start()
        {
            if (reposition_animation != null)
                GameSystems.reposition_list.Add(this);

            if (elastic_animation != null)
                GameSystems.elastic_list.Add(this);
        }

        #if UNITY_EDITOR
        void OnDrawGizmos()
        {
            if (debug_curve != null)
            {
                Vector3 point_0 = transform.TransformPoint(debug_curve.points[0]);
                Vector3 point_1 = transform.TransformPoint(debug_curve.points[1]);
                Vector3 control_0 = transform.TransformPoint(debug_curve.controls[0]);
                Vector3 control_1 = transform.TransformPoint(debug_curve.controls[1]);

                Gizmos.color = Color.green;
                Gizmos.DrawLine(point_0, control_0);
                Gizmos.DrawLine(point_1, control_1);

                Gizmos.color = Color.white;
                Gizmos.DrawSphere(control_0, 0.05f);
                Gizmos.DrawSphere(control_1, 0.05f);

                Gizmos.color = Color.red;
                Gizmos.DrawSphere(point_0, 0.05f);
                Gizmos.DrawSphere(point_1, 0.05f);

                Handles.DrawBezier(point_0, point_1, control_0, control_1, Color.white, null, 2.0f);
            }
        }
        #endif

        // ----------------------------------------------------------------------------------
        // -- Container functions.
        // ----------------------------------------------------------------------------------

        public void set_orientation(CardOrientation orientation) {
            this.orientation = orientation;
        }

        public void set_line_color(Color c) {
            material.SetColor("_line_color", c);
        }

        public void set_line_speed(float v) {
            material.SetFloat("_line_speed", v);
        }

        public void set_line_intensity(float v) {
            material.SetFloat("_line_intensity", v);
        }

        public void update_transform_at(CardMono card, int index)
        {
            if (orientation != null)
            {
                card.transform.localPosition = orientation.position(index);
                card.transform.localRotation = Quaternion.Euler(0, orientation.rotation(index), 0);
                card.base_position_rel = orientation.relative_position(index);
            }
        }

        // Updates the cards based on the container's orientation.
        public void update_transforms()
        {
            for (int i = 0; i < cards.Count; ++i)
            {
                CardMono card = cards[i];
                Vector3 rotation = card_orientation_rotation(card);

                card.transform.localPosition = card_orientation_position(card);
                card.transform.localRotation = Quaternion.Euler(rotation.x, rotation.y, rotation.z);

                card.base_position_t = card_orientation_position_t(card);
                card.base_position = card.transform.localPosition;
                card.base_rotation = card.transform.localRotation.eulerAngles;
            }
        }

        public Vector3 card_orientation_position(CardMono card)
        {
            Vector3 position = Vector3.zero;

            if (orientation != null)
                position = orientation.position(card.index);

            return position;
        }

        public Vector3 card_orientation_rotation(CardMono card)
        {
            Vector3 rotation = Vector3.zero;

            if (orientation != null)
                rotation.y = orientation.rotation(card.index);

            return rotation;
        }

        public float card_orientation_position_t(CardMono card)
        {
            float t = 0;

            if (orientation != null)
                t = orientation.relative_position(card.index);

            return t;
        }

        protected void update_index()
        {
            for (int i = 0; i < cards.Count; ++i)
            {
                CardMono card = cards[i];
                card.index = i;
            }
        }

        public void reposition_reset(int virtual_count = 0)
        {
            if (reposition_animation != null)
            {
                orientation.push_count(virtual_count);
                reposition_animation.reset();
                orientation.pop_count();
            }
        }

        void reposition_add(CardMono card)
        {
            if (reposition_animation != null)
                reposition_animation.add_card(card);
        }

        void reposition_remove(int i)
        {
            if (reposition_animation != null)
                reposition_animation.remove_card(i);
        }

        public void elastic_reset()
        {
            // if (elastic_animation != null)
                // elastic_animation.reset();
        }

        void elastic_add(CardMono card)
        {
            if (elastic_animation != null)
                elastic_animation.add_card(card);
        }

        void elastic_remove(int i)
        {
            if (elastic_animation != null)
                elastic_animation.remove_card(i);
        }

        void _add_card(CardMono card)
        {
            card.index = cards.Count;

            card.owner = owner;
            card.location = location;
            card.transform.parent = transform;

            cards.Add(card);

            if (orientation != null)
                orientation.virtual_count++;
        }

        CardMono _remove_card(int i)
        {
            CardMono card = cards[i];

            card.owner = PlayerID.NONE;
            card.location = CardLocation.LOCATION_NONE;
            // card.transform.localScale = new Vector3(1, 1, 1);
            card.transform.parent = null;

            cards.RemoveAt(i);
            update_index();

            if (orientation != null)
                orientation.virtual_count--;

            return card;
        }

        public void push_card(CardMono card, bool update_transform = false)
        {
            _add_card(card);

            if (update_transform)
                update_transforms();

            reposition_add(card);
            elastic_add(card);            
        }

        public CardMono remove_card(int i, bool update_transform = false)
        {
            CardMono card = _remove_card(i);

            if (update_transform)
                update_transforms();

            reposition_remove(i);
            elastic_remove(i);
            
            return card;
        }

        // public void rearrange()
        // {
        //     for (int i = 0; i < cards.Count; ++i)
        //     {
        //         CardMono card = cards[i];
        //         card.index = i;
        //         update_transform_at(card, i);
        //     }
        // }

        public void debug_push_card_at(CardMono card, int dest_index, int forced_count)
        {
            card.index = cards.Count;

            card.owner = owner;
            card.location = location;

            int card_index = cards.Count;

            cards.Add(card);
            card.transform.parent = transform;

            card.transform.localPosition = Vector3.zero;
            card.transform.localRotation = Quaternion.identity;

            if (orientation != null)
                orientation.virtual_count++;

            int old_virtual_count = orientation.virtual_count;
            orientation.virtual_count = forced_count;
            update_transform_at(card, dest_index);
            orientation.virtual_count = old_virtual_count;

            if (reposition_animation)
                reposition_animation.add_card(card);

            if (elastic_animation)
                elastic_animation.add_card(card);
        }

        public bool valid_index(int index) {
            return index >= 0 && index < cards.Count;
        }
    }
}
