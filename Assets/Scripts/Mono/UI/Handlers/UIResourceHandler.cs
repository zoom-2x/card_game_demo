using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

using CardGame.Data;

namespace CardGame.Mono.UI
{
    public class UIResourceHandler : MonoBehaviour, 
                                     IPointerEnterHandler, 
                                     IPointerExitHandler
    {
        public Resource type = Resource.CREDITS;

        GameUI game_ui;
        Image background;

        int parent_padding = 4;
        Color over_color = new Color(0.682f, 1.0f, 1.0f, 1.0f);

        void Start()
        {
            game_ui = GameObject.Find("UI").GetComponent<GameUI>();
            background = transform.GetComponent<Image>();
        }

        // Position next to the right of the transform rectangle relative
        // to it's parent position.
        Vector2 get_parent_relative_position() 
        {
            RectTransform pt = transform.parent as RectTransform;
            RectTransform t = transform as RectTransform;

            // Debug.Log($"{pt.anchoredPosition} / {t.anchoredPosition} / {t.rect.size}");

            return (pt as RectTransform).anchoredPosition + 
                   (t as RectTransform).anchoredPosition + 
                   new Vector2((t as RectTransform).rect.size.x, parent_padding); 
        }

        public void OnPointerEnter(PointerEventData data)
        {
            background.color = over_color; 

            UIResourceHandler h = transform.GetComponent<UIResourceHandler>();

            Vector2 position = get_parent_relative_position() + new Vector2(10, 0);

            // game_ui.info_popup.set_title("");
            game_ui.info_popup.set_title(h.type.ToString());

            MockData.generate_popup_sample_data(game_ui.info_popup.data);
            game_ui.debug_display_popup(position);

            // TODO(gabic): Sa preiau datele din player-ul curent.
        }

        public void OnPointerExit(PointerEventData data)
        {
            background.color = Color.white;
            game_ui.hide_popup();
        }
    }
}
