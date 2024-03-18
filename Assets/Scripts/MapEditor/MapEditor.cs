using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

using CardGame.MapEditor.Panels;
using CardGame.Hexgrid;
using CardGame.Mono;
using CardGame.Mono.UI;
using CardGame.UI;
using CardGame.Managers;
using CardGame.CGDebug;

namespace CardGame.MapEditor
{
    public class MapEditor : MonoBehaviour
    {
        public bool debug_load = false;
        public string debug_map = "";

        UIDocument main_ui_document = null;
        UIManager ui_manager = null;
        VisualElement root = null;

        bool initialized = false;
        LevelFade fade;

        void Awake()
        {
            if (debug_load)
                DebugScenes.load_bundles_sync();

            GameObject map_obj = GameObject.Find("MAP");

            main_ui_document = GameObject.Find("UI_MAIN").GetComponent<UIDocument>();
            root = main_ui_document.rootVisualElement;

            Popup.entry_template = GameSystems.asset_manager.aquire_ut_tree_asset(BundleEnum.UI, "popup_entry");
            MonoUtils.cache_init();
        }

        void Start()
        {
            fade = GameObject.Find("level_fade").GetComponent<LevelFade>();
            fade.set_state(LevelFade.STATE_FADE_IN);
            fade.begin();
        }

        void OnEnable()
        {}

        void OnDisable()
        {}

        void initialize()
        {
            initialized = true;

            RegionPanel.list_item_template = GameSystems.asset_manager.aquire_ut_tree_asset(BundleEnum.UI, "region_list_item");
            TilePanel.list_item_template = GameSystems.asset_manager.aquire_ut_tree_asset(BundleEnum.UI, "tile_region_list_item");
            LinkListPanel.list_item_template = GameSystems.asset_manager.aquire_ut_tree_asset(BundleEnum.UI, "link_list_item");
            ui_manager = new UIManager(root);

            if (!String.IsNullOrEmpty(debug_map))
                ui_manager.load_map_callback(debug_map);
        }

        void Update()
        {
            if (Input.GetKeyDown("f12"))
                SceneManager.LoadScene("main_menu");

            if (!initialized)
                initialize();                
            else 
                ui_manager.update();
        }

        void after_load_callback() {
        }
    }
}
