using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using CardGame.Animation;
using CardGame.Data;
using CardGame.Hexgrid;
using CardGame.Mono.State;
using CardGame.Mono.UI;

namespace CardGame.Mono
{
    public class MapPlayStateManager : IStateManager
    {
        public const int STATE_NONE = 0;
        public const int STATE_EXPANSION = 1;
        public const int STATE_EXPANSION_POPUP = 2;
        public const int STATE_RESISTANCE = 3;
        public const int STATE_RESISTANCE_POPUP = 4;
        public const int STATE_INFLUENCE = 5;
        public const int STATE_INFLUENCE_POPUP = 6;

        bool _active = false;

        int state = STATE_NONE;
        bool state_initialized = false;

        bool popup_confirm = false;
        bool popup_update = false;

        // int resource_incr = 0;
        // int resource_start_points = 0;
        // int resource_used_points = 0;

        int[] popup_start_resources = new int[PlayerResources.COUNT];

        HexTile popup_over_tile;

        public MapPlayStateManager()
        {
            GameSystems.game.ui.action_callback += action_click;
            GameSystems.game.ui.filter_callback += filter_click;

            GameSystems.game.ui.tile_action_popup.bonus_click_callback += action_popup_bonus_click;
            GameSystems.game.ui.tile_action_popup.confirm_callback += action_popup_confirm_click;
        }

        public void set_active(bool active) {
            _active = active;
        }

        // -----------------------------------------------------------
        // -- UI callbacks.
        // -----------------------------------------------------------

        void action_click(Resource r)
        {
            Debug.Log("action click...");

            if (!_active)
                return;

            reset();

            switch (r)
            {
                case Resource.EXPANSION:
                    state = STATE_EXPANSION;
                    break;

                case Resource.RESISTANCE:
                    state = STATE_RESISTANCE;
                    break;

                case Resource.INFLUENCE:
                    state = STATE_INFLUENCE;
                    break;
            }

            GameSystems.game.ui.set_action_state(r.ToString());
        }

        void filter_click(HexTileInfoState info_state)
        {
            if (!_active)
                return;

            GameSystems.game.current_player.default_tile_info_state = info_state;

            if (state == STATE_NONE)
                GameSystems.game.map.set_tile_info_state(info_state);
        }

        // used resources points (variable) / initial resource cost / starting total resource points
        Vector3Int resource_limits = Vector3Int.zero;

        void action_popup_bonus_click(int i)
        {
            if (!_active)
                return;

            // resource_incr = 0;

            if (TileActionPopup.BONUS_PLUS == i)
            {
                popup_update = true;
                resource_limits.x++;
                // resource_used_points++;
                // resource_incr = 1;
            }

            else if (TileActionPopup.BONUS_MINUS == i)
            {
                popup_update = true;
                resource_limits.x--;
                // resource_used_points--;
                // resource_incr = -1;
            }

            clamp_resource_limits();
        }

        void action_popup_confirm_click() 
        {
            if (!_active)
                return;

            popup_confirm = true;
        }

        bool clamp_resource_limits()
        {
            bool res = false;

            if (resource_limits.x < resource_limits.y)
            {
                res = true;
                resource_limits.x = resource_limits.y;
            }

            // One point is allocated by default when the tile is selected.
            if (resource_limits.x > resource_limits.z)
            {
                res = true;
                resource_limits.x = resource_limits.z;
            }

            return res;
        }

        void reset()
        {
            GameSystems.game.map.reset_tile_state();
            GameSystems.game.map.selection_filter_flags = 0;
            GameSystems.game.map.set_default_tile_info_state();

            state_initialized = false;
            state = STATE_NONE;
            GameSystems.game.ui.set_action_state(null);
        }

        // -----------------------------------------------------------
        // -- Expansion.
        // -----------------------------------------------------------

        void state_expansion()
        {
            Player p = GameSystems.game.current_player;
            resource_limits.z = p.res.get(Resource.EXPANSION);

            if (!state_initialized)
            {
                state_initialized = true;
                p.generate_border_target_tiles();
                // GameSystems.game.other_players_tile_info_state(HexTileInfoState.STATE_RESISTANCE);
                GameSystems.game.map.set_tile_info_state(HexTileInfoState.STATE_RESISTANCE);
                GameSystems.game.map.selection_filter_flags = (uint) HexTileState.STATE_DASHED;
            }

            // A tile was selected for expansion.
            if (Input.GetMouseButtonDown(0) && GameSystems.game.map.over_tile != null && resource_limits.z > 0)
            {
                // The basic action costs 1 point of expansion.
                resource_limits.x = 1;
                resource_limits.y = 1;

                p.res.decr(Resource.EXPANSION);

                popup_over_tile = GameSystems.game.map.over_tile;
                set_popup_expansion_text(GameRules.get_expansion_strength(p, popup_over_tile) +
                                         resource_limits.x - resource_limits.y,
                                         popup_over_tile.stat_resistance);

                GameSystems.game.ui.tile_action_popup.show(popup_over_tile);
                state = STATE_EXPANSION_POPUP;
            }

            // Exit the expansion mode.
            else if (Input.GetMouseButtonDown(1))
                reset();
        }

        void state_expansion_popup()
        {
            Player p = GameSystems.game.current_player;

            if (popup_confirm || Input.GetMouseButtonDown(1))
            {
                if (popup_confirm)
                {
                    int attack_strength = GameRules.get_expansion_strength(p, popup_over_tile) +
                                          resource_limits.x - resource_limits.y;
                    int defence_strength = popup_over_tile.stat_resistance;

                    bool result = GameRules.roll(attack_strength, defence_strength);

                    // The expansion attempt was successful.
                    if (result)
                    {
                        GameRules.capture_tile(popup_over_tile, p);
                        GameSystems.game.current_player.generate_border_target_tiles();
                    }
                }

                else
                    p.res.set(Resource.EXPANSION, resource_limits.z);

                popup_confirm = false;
                popup_update = false;
                popup_over_tile = null;

                resource_limits.x = 0;
                resource_limits.y = 0;
                resource_limits.z = 0;

                state = STATE_EXPANSION;
                GameSystems.game.ui.tile_action_popup.hide();
            }

            else if (popup_update)
            {
                p.res.set(Resource.EXPANSION, resource_limits.z - resource_limits.x);

                popup_update = false;
                set_popup_expansion_text(GameRules.get_expansion_strength(p, popup_over_tile) +
                                         resource_limits.x - resource_limits.y,
                                         popup_over_tile.stat_resistance);
            }
        }

        // -----------------------------------------------------------
        // -- Resistance.
        // -----------------------------------------------------------

        int tile_start_resistance = 0;

        void state_resistance()
        {
            Player p = GameSystems.game.current_player;

            resource_limits.z = GameSystems.game.current_player.res.get(Resource.RESISTANCE);

            if (!state_initialized)
            {
                state_initialized = true;
                GameSystems.game.current_player.set_tile_info_state(HexTileInfoState.STATE_RESISTANCE);
                GameSystems.game.current_player.mark_own_tiles();
                GameSystems.game.map.selection_filter_flags = (uint) HexTileState.STATE_DASHED;
            }

            // A tile was selected for resistance.
            if (Input.GetMouseButtonDown(0) && GameSystems.game.map.over_tile != null && resource_limits.z > 0)
            {
                resource_limits.x = 0;
                resource_limits.y = 0;

                popup_over_tile = GameSystems.game.map.over_tile;
                tile_start_resistance = popup_over_tile.stat_resistance;
                set_popup_resistance_text();

                GameSystems.game.ui.tile_action_popup.show(popup_over_tile);
                state = STATE_RESISTANCE_POPUP;
            }

            if (Input.GetMouseButtonDown(1))
                reset();
        }

        void state_resistance_popup()
        {
            Player p = GameSystems.game.current_player;

            if (popup_confirm || Input.GetMouseButtonDown(1))
            {
                if (popup_confirm)
                {}
                else
                {
                    p.res.set(Resource.RESISTANCE, resource_limits.z);
                    popup_over_tile.set_tile_resistance(tile_start_resistance);
                }

                popup_confirm = false;
                popup_update = false;
                popup_over_tile = null;

                resource_limits.x = 0;
                resource_limits.y = 0;
                resource_limits.z = 0;

                state = STATE_RESISTANCE;
                GameSystems.game.ui.tile_action_popup.hide();
            }

            else if (popup_update)
            {
                popup_over_tile.set_tile_resistance(tile_start_resistance + resource_limits.x);
                p.res.set(Resource.RESISTANCE, resource_limits.z - resource_limits.x);

                popup_update = false;
            }
        }

        // -----------------------------------------------------------
        // -- Influence.
        // -----------------------------------------------------------

        void state_influence()
        {
            Player p = GameSystems.game.current_player;

            int current_resource_points = GameSystems.game.current_player.res.get(Resource.RESISTANCE);

            if (Input.GetMouseButtonDown(1))
            {
                GameSystems.game.map.reset_tile_state();
                GameSystems.game.map.selection_filter_flags = 0;

                state_initialized = false;
                state = STATE_NONE;
                GameSystems.game.ui.set_action_state(null);
            }
        }

        void state_influence_popup()
        {}

        public void frame_update()
        {
            switch (state)
            {
                case STATE_EXPANSION:
                    state_expansion();
                    break;

                case STATE_EXPANSION_POPUP:
                    state_expansion_popup();
                    break;

                case STATE_RESISTANCE:
                    state_resistance();
                    break;

                case STATE_RESISTANCE_POPUP:
                    state_resistance_popup();
                    break;

                case STATE_INFLUENCE:
                    state_influence();
                    break;

                case STATE_INFLUENCE_POPUP:
                    state_influence_popup();
                    break;
            }

            // HexMapInput.update();
        }

        void set_popup_expansion_text(int expansion_strength, int tile_resistance)
        {
            int tile_resistance_perc = tile_resistance * 10;
            int expansion_strength_perc = expansion_strength * 10;
            int result_perc = expansion_strength_perc - tile_resistance_perc;
            int final_result_perc = result_perc < 0 ? 0 : result_perc;

            GameSystems.game.ui.tile_action_popup.set_title($"POW: {expansion_strength_perc}% - RES: {tile_resistance_perc}% = {result_perc}% ({final_result_perc})%");
        }

        void set_popup_resistance_text() {
            // GameSystems.game.ui.tile_action_popup.set_title($"RES: {tile_resistance}i%");
            GameSystems.game.ui.tile_action_popup.set_title("");
        }
    }
}
