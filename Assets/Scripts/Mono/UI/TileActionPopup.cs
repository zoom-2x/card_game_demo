using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using TMPro;

using CardGame.Hexgrid;

namespace CardGame.Mono.UI
{
    public class TileActionPopup : MonoBehaviour
    {
        public const int BONUS_PLUS = 0;
        public const int BONUS_MINUS = 1;

        Camera scene_camera;
        TextMeshProUGUI title_tpro;
        HexTile tile;

        public event System.Action<int> bonus_click_callback;
        public event System.Action confirm_callback;

        void Start()
        {
            scene_camera = GameObject.Find("MAIN_CAMERA").GetComponent<Camera>();
            title_tpro = transform.GetChild(0).GetComponent<TextMeshProUGUI>(); 

            hide();
        }

        bool first = false;

        public void show(HexTile t)
        {
            tile = t;

            gameObject.SetActive(true);
            first = true;
            update_ui_position();
        }

        public void hide() {
            gameObject.SetActive(false);
        }

        public void set_title(string t) {
            title_tpro.text = t;
        }

        public void update_ui_position()
        {
            if (tile != null && scene_camera != null)
            {
                Vector3 screen_position = scene_camera.WorldToScreenPoint(tile.base_transform.position);
                screen_position.y += 40;
                
                if (first)
                    first = false;

                RectTransform rt = transform as RectTransform;
                rt.anchoredPosition = new Vector2(screen_position.x, screen_position.y);
            }
        }

        public void bonus_click(int i) {
            bonus_click_callback?.Invoke(i);
        }

        public void confirm() {
            confirm_callback?.Invoke();
        }
    }
}
