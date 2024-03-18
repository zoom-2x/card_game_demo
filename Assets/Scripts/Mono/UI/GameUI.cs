using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

using CardGame.UI;
using CardGame.CGDebug;
using CardGame.Data;
using CardGame.Hexgrid;

namespace CardGame.Mono.UI
{
    public class GameUI : MonoBehaviour
    {
        public const int BUTTON_VIEW_CARDS = 0;
        public const int BUTTON_END_TURN = 1;
        public const int BUTTON_MAIN_MENU = 2;

        public float ui_update_cycle_sec = 0.0f;
        float _ui_update_time = 0;

        [System.NonSerialized] public InfoPopup info_popup;
        [System.NonSerialized] public TileActionPopup tile_action_popup;

        public event System.Action end_turn_callback;
        public event System.Action view_cards_callback;
        public event System.Action<Resource> action_callback;
        public event System.Action<HexTileInfoState> filter_callback;
        event System.Action main_menu_callback;

        RectTransform root;
        LevelFade fade;
        string target_level = "";

        TextMeshProUGUI current_turn_tpro;

        Image[] vp_progress_image = new Image[(int) PlayerID.PLAYER_COUNT];
        TextMeshProUGUI[] vp_progress_tpro = new TextMeshProUGUI[(int) PlayerID.PLAYER_COUNT];

        Transform current_action_state_tr;
        TextMeshProUGUI current_action_state_tpro;

        TextMeshProUGUI[] filters_tpro = new TextMeshProUGUI[2];
        TextMeshProUGUI[] resources_tpro = new TextMeshProUGUI[PlayerResources.COUNT];

        // Tile action popup.
        Transform tile_action_popup_tr;

        void Start()
        {
            DebugScenes.load_bundles_sync();
            MonoUtils.cache_init();

            Transform root = transform.Find("main_ui_canvas");

            current_action_state_tr = GameObject.Find("UI/main_ui_canvas/current_action_state").transform;
            current_action_state_tpro = current_action_state_tr.GetChild(0).GetComponent<TextMeshProUGUI>();
            set_action_state(null);

            current_turn_tpro = transform.Find("main_ui_canvas/current_turn").GetChild(1).GetComponent<TextMeshProUGUI>();

            // -- VPs.

            Transform vp_base = transform.Find("main_ui_canvas/blue_vp");
            vp_progress_image[0] = vp_base.GetChild(0).GetComponent<Image>();
            vp_progress_tpro[0] = vp_base.GetChild(1).GetComponent<TextMeshProUGUI>();

            vp_base = transform.Find("main_ui_canvas/red_vp");
            vp_progress_image[1] = vp_base.GetChild(0).GetComponent<Image>();
            vp_progress_tpro[1] = vp_base.GetChild(1).GetComponent<TextMeshProUGUI>();

            // -- Resources.

            Transform resources = transform.Find("main_ui_canvas/player_resources");

            for (int i = 0; i < resources.childCount; ++i) {
                resources_tpro[i] = resources.GetChild(i).GetChild(0).GetChild(1).GetComponent<TextMeshProUGUI>();
            }

            // -- Filters.

            Transform filters = transform.Find("main_ui_canvas/filter_buttons");

            for (int i = 0; i < 2; ++i) {
                filters_tpro[i] = filters.GetChild(0).GetComponent<TextMeshProUGUI>();
            }
            
            // TODO(gabic): Sa "initializez" si celelalte componente.
            info_popup = new InfoPopup(root);
            tile_action_popup = transform.Find("region_labels_canvas/tile_action_popup").GetComponent<TileActionPopup>();

            fade = MonoUtils.get_level_fade();

            if (fade != null)
                fade.fade_out_finished_event += fade_out_callback;

            main_menu_callback += switch_to_main_menu;
        }

        void Update()
        {
            if (GameSystems.game != null)
            {
                _ui_update_time += Time.deltaTime;

                if (_ui_update_time >= ui_update_cycle_sec)
                {
                    // Debug.Log("UI update...");

                    _ui_update_time = 0;
                    current_turn_tpro.text = $"{GameSystems.game.current_turn}";

                    for (int i = 0; i < GameSystems.game.max_players; ++i)
                    {
                        Player p = GameSystems.game.players[i];
                        float n = (float) p.res.vp / GameSystems.game.max_vp;

                        vp_progress_image[i].fillAmount = n;
                        vp_progress_tpro[i].text = $"{p.res.vp}/{GameSystems.game.max_vp}";
                    }

                    // Update the ui based on the current player.
                    if (GameSystems.game.current_player != null)
                    {
                        Player p = GameSystems.game.current_player;

                        for (int j = 0; j < PlayerResources.COUNT; ++j) {
                            resources_tpro[j].text = $"{p.res.get((Resource) j)}";
                        }
                    }
                }
            }

            // if (Input.GetKeyDown("f1"))
                // debug_display_popup(new Vector2(100, 0));
        }

        void fade_out_callback() 
        {
            if (!String.IsNullOrEmpty(target_level))
            {
                SceneManager.LoadScene(target_level);
                target_level = "";
            }
        }

        void switch_to_main_menu()
        {
            if (fade != null)
            {
                target_level = "main_menu";
                fade.set_state(LevelFade.STATE_FADE_OUT);
                fade.begin();
            }
        }

        public void debug_display_popup(Vector2 position)
        {
            // info_popup.hide();
            info_popup.set_data();
            info_popup.set_position(position);
            info_popup.show();
        }

        public void display_popup(Vector2 position)
        {}

        public void hide_popup() {
            info_popup.hide();
        }

        public void show_popup_data(Vector2 position)
        {}

        public void button_click_callback(int i)
        {
            switch (i)
            {
                case BUTTON_VIEW_CARDS:
                    view_cards_callback?.Invoke();
                    break;

                case BUTTON_END_TURN:
                    end_turn_callback?.Invoke();
                    break;

                case BUTTON_MAIN_MENU:
                    main_menu_callback?.Invoke();
                    break;
            }
        }

        public void action_click_callback(int i) {
            action_callback?.Invoke((Resource) i);
        }

        public void filter_change(Toggle change)
        {
            HexTileInfoState i = HexTileInfoState.STATE_HIDDEN;

            switch (change.name)
            {
                case "filter_id":
                    i = change.isOn ? HexTileInfoState.STATE_ID : i;
                    break;

                case "filter_coordinates":
                    i = change.isOn ? HexTileInfoState.STATE_COORDINATES : i;
                    break;
            }

            filter_callback?.Invoke(i);
        }

        public void set_action_state(string s)
        {
            if (String.IsNullOrEmpty(s))
            {
                current_action_state_tr.gameObject.SetActive(false);
                current_action_state_tpro.text = "";
            }
            else
            {
                current_action_state_tr.gameObject.SetActive(true);
                current_action_state_tpro.text = s;
            }
        }
    }
}
