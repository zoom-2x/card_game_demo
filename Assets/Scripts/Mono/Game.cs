using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

using CardGame.Mono.UI;
using CardGame.Attributes;
using CardGame.Hexgrid;
using CardGame.Data;
using CardGame.IO;
using CardGame.CGDebug;

namespace CardGame.Mono
{
    public enum GameState
    {
        STATE_CARD_PLAY = 1,
        STATE_MAP_PLAY = 2
    }

    public class Game : MonoBehaviour
    {
        public GameState state = GameState.STATE_CARD_PLAY;

        int player_index = 0;
        public Player[] players = new Player[4];

        public int max_vp = 30;
        public int max_turns = 0;
        public int max_players = 2;

        [CardArraySelect(0, "Card style")] public int style_index;

        [System.NonSerialized] public HexMap map;
        [System.NonSerialized] public GameUI ui;
        [System.NonSerialized] public Player current_player;

        [System.NonSerialized] public bool enable_reposition = true;
        [System.NonSerialized] public bool enable_elastic = true;

        [System.NonSerialized] public Camera scene_camera = null;
        [System.NonSerialized] public CardLibrary card_library = new CardLibrary();

        LevelFade fade;
        bool initialized = false;
        int _current_turn = 1;
        public int current_turn { get { return _current_turn; } }

        public bool debug_play = true;
        public bool debug_load = false;
        public string debug_map_name = "test_map_v2";

        [System.NonSerialized] public CardPlayStateManager card_play_sm = null;
        [System.NonSerialized] public MapPlayStateManager map_play_sm = null;
        [System.NonSerialized] public IStateManager current_sm = null;

        void Start()
        {
            GameSystems.game = this;
            scene_camera = GameObject.Find("MAIN_CAMERA").GetComponent<Camera>();

            MonoUtils.cache_init();

            if (debug_load)
                DebugScenes.load_bundles_sync();

            ui = GameObject.Find("UI").transform.GetComponent<GameUI>();

            SceneManager.LoadScene("room", LoadSceneMode.Additive);
            fade = MonoUtils.get_level_fade();

            if (fade != null)
            {
                fade.fade_in_finished_event += fade_in_callback;
                fade.fade_out_finished_event += fade_out_callback;
                fade.set_state(LevelFade.STATE_FADE_IN);
            }
        }

        void fade_in_callback() {}
        void fade_out_callback() {}

        void initialize()
        {
            if (initialized)
                return;

            initialized = true;

            GameConfig.load();
            Easing.init();

            DebugFile.enabled = GameConfig.game.debug_mode;

            Application.targetFrameRate = GameConfig.game.frame_rate;
            UniversalRenderPipelineAsset data = GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;

            if (data) {
                data.msaaSampleCount = 4;
            }

            // Player initialization (doar primul jucator momentan).
            players[0].initialize();
            players[1].initialize();
            // players[2].initialize();
            // players[3].initialize();

            current_player = players[player_index];

            card_play_sm = new CardPlayStateManager();
            map_play_sm = new MapPlayStateManager();
            
            if (debug_play)
            {
                if (state == GameState.STATE_CARD_PLAY)
                    CGDebug.DebugScenes.load();
                else if (state == GameState.STATE_MAP_PLAY)
                {
                    HexRegion.global = HexRegion.FLAG_CURVED_BORDER;
                    map = CGDebug.DebugScenes.test_map_play(debug_map_name);
                }
            }

            players[0].map = map;
            players[0].res.vp = 12;

            players[1].map = map;
            players[1].res.vp = 15;
            // players[1].generate_target_tiles();
        }

        void Update()
        {
            if (!initialized)
                initialize();
            else
            {
                if (fade != null)
                    fade.begin();

                GameSystems.input.set_collision_mask(get_current_player_mask());
                // GameSystems.input.set_collision_mask(Constants.LAYER_MASK_MARKETPLACE);
                GameSystems.input.update();

                // State processing here.
                if (current_sm != null)
                    current_sm.frame_update();

                if (map != null)
                    map.update();

                if (GameSystems.game.ui.tile_action_popup != null)
                    GameSystems.game.ui.tile_action_popup.update_ui_position();
            }
        }

        void switch_state(IStateManager sm)
        {
            if (current_sm != null)
                current_sm.set_active(false);

            current_sm = sm;
            current_sm.set_active(true);
        }

        public void enter_card_play_mode(CardPlayState state = CardPlayState.NONE)
        {
            if (state == CardPlayState.NONE)
                state = CardPlayState.DRAW;

            switch_state(card_play_sm); 
            card_play_sm.set_state(state);
        }

        public void enter_map_play_mode() {
            switch_state(map_play_sm);
        }

        public Player get_current_player() {
            return players[player_index];
        }

        public Player get_player(PlayerID id) {
            return players[(int) id];
        }

        public void change_player() {
            player_index = (++player_index) % max_players;
        }

        public int get_current_player_mask() {
            return Constants.LAYER_MASK_PLAYER[player_index];
        }

        public void other_players_tile_info_state(HexTileInfoState state)
        {
            for (int i = 0; i < 4; ++i)
            {
                Player p = players[i];

                if (p.id == current_player.id)
                    continue;
                
                p.set_tile_info_state(state);
            }
        }
    }
}
