using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;
using TMPro;

using CardGame.Animation;
using CardGame.Data;
using CardGame.Managers;

namespace CardGame.Mono
{
    // public enum CardStatus : uint
    // {
    //     NONE = 0,
    //     IN_TRANSIT = 1,
    //     IN_PLACE = 2,
    //     NEEDS_REPOSITIONING = 3,
    //     WAITING_FOR_TRANSIT = 4
    // }

    public class CardInterpolators
    {
        public Vector3 start_rotation;
        public Vector3 end_rotation;

        public float start_t_position;
        public float end_t_position;

        public float t_position;
        public float time;
    }

    public class CardMono : MonoBehaviour
    {
        // Flags.
        public const uint FLIPPED = 1;
        public const uint IN_TRANSIT = 2;
        public const uint INSPECTED = 3;

        static int _id = 0;

        [System.NonSerialized] public int id = 0;
        [System.NonSerialized] public uint flags = 0;
        [System.NonSerialized] public Card card;
        [System.NonSerialized] public float base_position_rel;
        [System.NonSerialized] public float over_delay = 0;
        [System.NonSerialized] public int index = 0;
        [System.NonSerialized] public Vector3[] cache_positions = new Vector3[3];
        [System.NonSerialized] public Vector3 base_position = Vector3.zero;
        [System.NonSerialized] public Vector3 base_rotation = Vector3.zero;
        [System.NonSerialized] public float base_position_t = 0;
        [System.NonSerialized] public PlayerID owner = PlayerID.NONE;
        [System.NonSerialized] public CardLocation location = CardLocation.LOCATION_NONE;
        [System.NonSerialized] public CardTemplate template = null;
        [System.NonSerialized] public string base_material_key = "";
        [System.NonSerialized] public string base_material_over_key = "";
        [System.NonSerialized] public string art_material_key = "";
        [System.NonSerialized] public Material base_material = null;
        [System.NonSerialized] public Material over_material = null;
        [System.NonSerialized] public Material art_material = null;
        [System.NonSerialized] public Renderer base_renderer;
        [System.NonSerialized] public Renderer art_renderer;
        [System.NonSerialized] public TextMeshProUGUI title;
        [System.NonSerialized] public Text description;

        void Awake()
        {
            id = ++_id;

            // card_renderer = gameObject.GetComponent<Renderer>();

            // card_v2 has 3 children.
            if (transform.childCount == 3)
            {
                Transform base_t = transform.GetChild(0);
                Transform art_t = transform.GetChild(1);
                Transform ui_t = transform.GetChild(2);

                if (base_t != null)
                    base_renderer = base_t.GetComponent<Renderer>();

                if (base_t != null)
                    art_renderer = art_t.GetComponent<Renderer>();

                if (ui_t != null)
                {
                    GameObject title_obj = ui_t.Find("Title").gameObject;
                    GameObject description_obj = ui_t.Find("Description").gameObject;

                    Renderer card_font_renderer = title_obj.GetComponent<Renderer>();
                    title = title_obj.GetComponent<TextMeshProUGUI>();
                    description = description_obj.GetComponent<Text>();
                }
            }

            // Default style setup.
            // CardStyle selected_style = GameSystems.GAME.library.card_styles[(int) CardStyleEnum.STYLE_DEFAULT];
            // set_style(selected_style);
        }

        public void set_normal() 
        {
            if (base_material != null)
                base_renderer.sharedMaterial = base_material;
        }

        public void set_over()
        {
            if (over_material != null)
                base_renderer.sharedMaterial = over_material;
        }

        public void set_font_color(Color c)
        {}

        public bool has_flag(uint flag) {
            return (flags & flag) == flag;
        }

        public void set_flag(uint flag) {
            flags |= flag;
        }

        public void unset_flag(uint flag) {
            flags &= ~flag;
        }

        public void toggle_flag(uint flag) {
            flags ^= flag;
        }

        public void flip()
        {
            toggle_flag(CardMono.FLIPPED);
            Vector3 rotation = transform.localRotation.eulerAngles;
            rotation.z += 180;
            transform.localRotation = Quaternion.Euler(rotation.x, rotation.y, rotation.z);
        }
    }
}
