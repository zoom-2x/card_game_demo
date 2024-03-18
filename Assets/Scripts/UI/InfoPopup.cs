using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using TMPro;

namespace CardGame.UI
{
    public struct InfoData
    {
        // 0 = plus, 1 = minus
        public int type;
        public string text;
    }

    public class InfoPopup 
    {
        public List<InfoData> data = new List<InfoData>();

        TextMeshProUGUI popup_title_tpro;
        RectTransform popup_ui;
        RectTransform popup_content;

        public InfoPopup(Transform ui_root)
        {
            if (ui_root != null)
            {
                popup_ui = ui_root.Find("popup") as RectTransform;
                // popup_content = popup_ui.GetChild(0).GetChild(0) as RectTransform;
                popup_title_tpro = popup_ui.GetChild(0).GetComponent<TextMeshProUGUI>();
                popup_content = popup_ui.GetChild(1) as RectTransform;
                hide();
            }
        }

        public void show() {
            popup_ui.gameObject.SetActive(true);
        }

        public void hide() {
            popup_ui.gameObject.SetActive(false);
        }

        void clear_data()
        {
            // Debug.Log(popup_content.childCount);

            for (int i = 0; i < popup_content.childCount; ++i)
            {
                Transform item = popup_content.GetChild(i);
                GameSystems.asset_manager.return_popup_item(item.gameObject);
            }
        }

        public void set_title(string title) 
        {
            if (String.IsNullOrEmpty(title))
                popup_title_tpro.gameObject.SetActive(false);
            else
            {
                popup_title_tpro.gameObject.SetActive(true);
                popup_title_tpro.text = title;
            }
        }

        public void set_data()
        {
            clear_data();

            for (int i = 0; i < data.Count; ++i)
            {
                InfoData v = data[i];
                Transform item = null;
                
                if (v.type == 0)
                    item = GameSystems.asset_manager.aquire_popup_item_plus();
                else if (v.type == 1)
                    item = GameSystems.asset_manager.aquire_popup_item_minus();

                if (item != null)
                {
                    item.gameObject.SetActive(true);
                    item.SetParent(popup_content);

                    TextMeshProUGUI item_text = item.GetChild(0).GetComponent<TextMeshProUGUI>();
                    item_text.text = v.text;
                }
            }
        }

        public void set_position(Vector2 position)
        {
            if (popup_ui != null)
            {
                // position.y *= -1;
                popup_ui.anchoredPosition = position;
            }
        }
    }
}
