using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using CardGame.Mono;
using CardGame.Managers;

namespace CardGame.Data
{
    public enum CardTemplateEnum : int
    {
        INFLUENCE = 0,
        CREDITS = 1,
        PROPAGANDA = 2,
    }

    public struct CardStyleUIParams
    {
        public Vector3 title_position;
        public Vector3 description_position;
        public Vector2 title_size;
        public Vector2 description_size;
    }

    public class CardLibrary
    {
        public static readonly string[][] CARD_ATTRIBUTES = new string[][]
        {
            // Styles (0).
            new string[] 
            {
                "default", 
                "style_0"
            },

            // Effects (1).
            new string[]
            {
                "none",
                "flame",
                "flow",
                "flow_texture",
                "fx0",
            },

            // Art (2).
            new string[]
            {
                "art_0", "art_1", "art_2", "art_3", "art_4",
                "art_5", "art_6", "art_7", "art_8", "art_9",
                "art_10"
            }
        };
        
        public static readonly string[] CARD_TEMPLATES = new string[] {
            "influence",
            "credits",
            "propaganda"
        };

        public string current_style = "default";

        public CardLibrary()
        {}
        
        CardStyleUIParams style_ui_params(string style_name)
        {
            CardStyleUIParams res = new CardStyleUIParams();

            if (style_name == "default")
            {
                res.title_position = new Vector3(0, 208, 0);
                res.description_position = new Vector3(0, 103, 0);
                res.title_size = new Vector2(270, 50);
                res.description_size = new Vector2(240, 120);
            }

            else if (style_name == "style_0")
            {
                res.title_position = new Vector3(0, -58, 0);
                res.description_position = new Vector3(0, 90, 0);
                res.title_size = new Vector2(280, 50);
                res.description_size = new Vector2(280, 120);
            }

            return res;
        }

        // ----------------------------------------------------------------------------------
        // -- Main card generation routine.
        // ----------------------------------------------------------------------------------

        public CardMono create_card(CardTemplateEnum template, PlayerID id)
        {
            string template_key = CardLibrary.CARD_TEMPLATES[(int) template];
            CardTemplate tpl = GameSystems.asset_manager.load_asset(BundleEnum.CARD_TEMPLATES, template_key) as CardTemplate;

            if (tpl == null)
            {
                Debug.LogWarning($"CardMono: Invalid template ({template}) !");
                return null;
            }

            string[] style_list = CARD_ATTRIBUTES[0];
            current_style = style_list[GameSystems.game.style_index];

            CardStyleUIParams ui_params = style_ui_params(current_style);
            GameObject obj = GameSystems.asset_manager.aquire_prefab(BundleEnum.PREFABS, "card_base");
            CardMono card = obj.GetComponent<CardMono>();

            // NOTE(gabic): Trebuie sa curat codul aici si in CardMono.
            if (card != null)
            {
                card.template = tpl;
                card.gameObject.layer = Constants.LAYER_PLAYER[(int) id];
                card.gameObject.SetActive(true);
                card.transform.parent = null;                

                // -- UI setup.

                Transform ui_t = obj.transform.GetChild(2);
                Transform title_obj = ui_t.Find("Title");
                Transform description_obj = ui_t.Find("Description");

                RectTransform title_rect = title_obj.GetComponent<RectTransform>();
                RectTransform description_rect = description_obj.GetComponent<RectTransform>();

                title_rect.anchoredPosition3D = ui_params.title_position;
                description_rect.anchoredPosition3D = ui_params.description_position;

                card.title.text = tpl.name;
                card.description.text = tpl.description;

                // -- Material setup.
                
                string[] effect_list = CARD_ATTRIBUTES[1];
                string[] art_list = CARD_ATTRIBUTES[2];

                string unlit = GameConfig.card.use_unlit ? "_unlit" : "";
                
                card.base_material_key = $"card_{current_style}_{effect_list[tpl.effect_index]}{unlit}";
                card.base_material_over_key = $"{card.base_material_key}_over";
                card.art_material_key = $"{current_style}_{art_list[tpl.art_index]}{unlit}";

                card.base_material = GameSystems.asset_manager.aquire_material(BundleEnum.MATERIALS, card.base_material_key);
                card.over_material = GameSystems.asset_manager.aquire_material(BundleEnum.MATERIALS, card.base_material_over_key);
                card.art_material = GameSystems.asset_manager.aquire_material(BundleEnum.MATERIALS, card.art_material_key);

                card.base_renderer.sharedMaterial = card.base_material;
                card.art_renderer.sharedMaterial = card.art_material;
            }

            return card;
        }

        public void destroy_card(CardMono card) {
            GameSystems.asset_manager.return_prefab("card_base", card.gameObject);
        }
    }
}
