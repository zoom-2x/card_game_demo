using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

using CardGame.Hexgrid;
using CardGame.IO;
using CardGame.UI;
using CardGame.Mono;
using CardGame.MapEditor.Panels;

using gc_components;

namespace CardGame.MapEditor
{
    public class UIManager
    {
        const int NEW_MAP_PANEL = 0;
        const int SAVE_LOAD_MAP_PANEL = 1;
        const int REGION_PANEL = 2;
        const int TILE_PANEL = 3;
        const int LINKS_PANEL = 4;
        const int MAIN_MENU = 5;

        VisualElement root = null;

        HexMap map = null;
        MainMenuPanel main_menu = null;
        StatusBarPanel status_bar = null;
        BasePanel[] panels = new BasePanel[5];

        int opened_panel = -1;
        int selected_tile_region_index = -1;
        bool edit_mode = false;
        bool mouse_over_ui = false;

        public UIManager(VisualElement root)
        {
            this.root = root;

            main_menu_init();
            status_bar_init();

            new_map_panel_init();
            save_load_map_panel_init();
            region_panel_init();
            tile_panel_init();
            links_panel_init();
            
            HexMap.global = HexMap.FLAG_EDITOR_MODE;
            HexRegion.global = HexRegion.FLAG_EDITOR_MODE | 
                               HexRegion.FLAG_CURVED_BORDER;
        }


        void ui_enter_leave_callback(bool enter)
        {
            mouse_over_ui = enter;
            CameraMovement.enabled = !enter;
        }

        // -----------------------------------------------------------
        // -- Main menu.
        // -----------------------------------------------------------

        LevelFade fade;

        void main_menu_init()
        {
            main_menu = new MainMenuPanel(root);
            main_menu.register_enter_leave_callback(ui_enter_leave_callback);
            main_menu.menu_click += menu_click;

            fade = MonoUtils.get_level_fade();

            if (fade != null)
                fade.fade_out_finished_event += fade_out_callback;
        }

        void fade_out_callback() {
            SceneManager.LoadScene("main_menu");
        }

        public void menu_click(int i)
        {
            if (fade.running)
                return;

            if (i == MAIN_MENU)
            {
                fade.set_state(LevelFade.STATE_FADE_OUT);
                fade.begin();
            }

            if (i < 0 || i >= panels.Length)
                return;

            bool must_open = false;
            edit_mode = false;

            if (!panels[i].is_open())
                must_open = true;

            for (int j = 0; j < 5; ++j)
            {
                main_menu.state_off(j);
                panels[j].close();
            }

            if (must_open)
            {
                main_menu.state_on(i);
                panels[i].open();

                if (i == TILE_PANEL)
                    edit_mode = true;
            }

            if (must_open)
                opened_panel = i;
            else
                opened_panel = -1;
        }

        // -----------------------------------------------------------
        // -- Status bar.
        // -----------------------------------------------------------

        void status_bar_init()
        {
            status_bar = new StatusBarPanel(root);
            status_bar.register_enter_leave_callback(ui_enter_leave_callback);
            status_bar.set_map_size(0, 0);
            status_bar.set_message("");
        }

        // -----------------------------------------------------------
        // -- New map panel.
        // -----------------------------------------------------------

        void new_map_panel_init()
        {
            NewMapPanel.generate_event += generate_new_map_callback;

            NewMapPanel panel = new NewMapPanel(root);
            panel.register_enter_leave_callback(ui_enter_leave_callback);
            panels[NEW_MAP_PANEL] = panel;
        }

        void generate_new_map_callback(int rows, int cols)
        {
            Debug.Log("generating map...");

            if (map != null)
                map.destroy();

            HexTile.reset_counter();
            map = new HexMap(rows, cols);
            // HexMapInput.map = map;
            status_bar.set_map_size(rows, cols);

            update_region_list();
            update_tile_region_list();
            update_link_list();
        }

        // -----------------------------------------------------------
        // -- Save / Load map panel.
        // -----------------------------------------------------------

        void save_load_map_panel_init()
        {
            SaveLoadMapPanel.save_map_event += save_map_callback;
            SaveLoadMapPanel.load_map_event += load_map_callback;

            SaveLoadMapPanel panel = new SaveLoadMapPanel(root);
            panel.register_enter_leave_callback(ui_enter_leave_callback);
            panels[SAVE_LOAD_MAP_PANEL] = panel;
        }

        void save_map_callback(string name, string filepath)
        {
            if (map == null)
                status_bar.set_message("No map was generated");
            else if (String.IsNullOrEmpty(name))
                status_bar.set_message("Missing map name");
            else if (String.IsNullOrEmpty(filepath))
                status_bar.set_message("Missing map filepath");
            else
            {
                map.name = name;
                MapLoader.save(map, filepath);
                status_bar.set_message("Map saved");
            }
        }

        public void load_map_callback(string filepath)
        {
            if (String.IsNullOrEmpty(filepath))
                status_bar.set_message("Missing map filepath");
            else
            {
                HexTile.reset_counter();
                HexMap loaded_map = MapLoader.load(filepath);

                if (map != null)
                    map.destroy();

                map = loaded_map;
                map.generate_borders();

                SaveLoadMapPanel save_load_map_panel = panels[SAVE_LOAD_MAP_PANEL]  as SaveLoadMapPanel;

                // HexMapInput.map = map;
                status_bar.set_map_size(map.rows, map.cols);
                save_load_map_panel.set_map_name(map.name);
                save_load_map_panel.set_map_filepath(filepath);

                update_region_list();
                update_tile_region_list();
                update_link_list();
            }
        }

        // -----------------------------------------------------------
        // -- Region panel.
        // -----------------------------------------------------------

        void region_panel_init()
        {
            RegionPanel.new_region_event += new_region_callback;
            RegionEditPanel.update_region_event += update_region_callback;
            RegionEditPanel.delete_region_event += delete_region_callback;

            RegionPanel panel = new RegionPanel(root);
            panel.register_enter_leave_callback(ui_enter_leave_callback);
            panels[REGION_PANEL] = panel;
        }

        void new_region_callback()
        {
            if (map == null)
            {
                status_bar.set_message("No map was generated");
                return;
            }

            HexRegion region = new HexRegion(Color.black);
            map.add_region(region);

            update_region_list();
        }

        void update_region_callback(RegionData data, int region_index)
        {
            HexRegion region_object = map.region(region_index);

            // region_object.name = data.name;
            region_object.change_name(data.name);
            // region_object.change_color(data.color);
            // region_object.color = data.color;
            // region_object.resource_mask = data.resource_mask;
            // region_object.resource_value_mask = data.resource_value_mask;
            region_object.set_resources(data.resource_mask, data.resource_value_mask);

            update_region_list();
        }

        void delete_region_callback(int region_index)
        {
            map.remove_region(region_index);
            update_region_list();
        }

        List<RegionData> region_data_list = new List<RegionData>();

        public void update_region_list()
        {
            if (map == null)
                return;

            update_tile_region_list();
            region_data_list.Clear();

            for (int i = 0; i < map.region_count; ++i)
            {
                HexRegion region = map.region(i);
                RegionData data = new RegionData();

                data.name = region.name;
                data.color = region.color;
                data.resource_mask = region.resource_mask;
                data.resource_value_mask = region.resource_value_mask;

                region_data_list.Add(data);
            }

            RegionPanel panel = panels[REGION_PANEL] as RegionPanel;
            panel.update_list(region_data_list);
        }

        // -----------------------------------------------------------
        // -- Tile panel.
        // -----------------------------------------------------------

        void tile_panel_init()
        {
            TilePanel panel =  new TilePanel(root);
            panel.register_enter_leave_callback(ui_enter_leave_callback);
            panels[3] = panel;

            TilePanel.select_tile_region_event += select_tile_region_callback;
        }

        void select_tile_region_callback(int region_index)
        {
            selected_tile_region_index = region_index;

            if (selected_tile_region_index > 0)
            {
                HexRegion region = map.region(selected_tile_region_index - 1);
                status_bar.set_tile_region(region.name);
            }
        }

        List<TileRegionData> tile_region_data_list = new List<TileRegionData>();

        public void update_tile_region_list()
        {
            if (map == null)
                return;

            tile_region_data_list.Clear();
            tile_region_data_list.Add(new TileRegionData() {index = -1, name = "None", color = Color.black});

            for (int i = 0; i < map.region_count; ++i)
            {
                HexRegion region = map.region(i);
                TileRegionData data = new TileRegionData();

                data.name = region.name;
                data.color = region.color;

                tile_region_data_list.Add(data);
            }

            TilePanel panel = panels[TILE_PANEL] as TilePanel;
            panel.update_list(tile_region_data_list);
        }
        
        // -----------------------------------------------------------
        // -- Links panel.
        // -----------------------------------------------------------

        void links_panel_init()
        {
            LinkListPanel panel =  new LinkListPanel(root);
            LinkEditPanel.save_link_event += save_link_callback;
            panel.register_enter_leave_callback(ui_enter_leave_callback);
            panels[4] = panel;

            LinkListPanel.new_link_event += new_link_callback;
        }

        void new_link_callback()
        {
            if (map == null)
                return;

            HexTileLink link = new HexTileLink();
            map.add_link(link);
            update_link_list();
        }

        void save_link_callback(Vector2Int start_tile, Vector2Int middle_tile, Vector2Int end_tile, int index)
        {
            if (index >= 0 && index < map.link_count)
            {
                HexTileLink link = map.link(index);

                HexTile start = map.get_tile_from_real_offset(start_tile);
                HexTile middle = map.get_tile_from_real_offset(middle_tile);
                HexTile end = map.get_tile_from_real_offset(end_tile);

                link.set_tiles(start, middle, end);
                update_link_list();
            }
        }

        List<LinkData> link_data_list = new List<LinkData>();

        void update_link_list()
        {
            if (map == null)
                return;

            link_data_list.Clear();

            for (int i = 0; i < map.link_count; ++i)
            {
                HexTileLink link = map.link(i);
                LinkData data = new LinkData();

                if (link.tiles[0] != null)
                    data.start_tile = link.tiles[0].real_coordinates;
                else
                    data.start_tile = new Vector2Int();

                if (link.tiles[1] != null)
                    data.middle_tile = link.tiles[1].real_coordinates;
                else
                    data.middle_tile = new Vector2Int();

                if (link.tiles[2] != null)
                    data.end_tile = link.tiles[2].real_coordinates;
                else
                    data.end_tile = new Vector2Int();

                link_data_list.Add(data);
            }

            LinkListPanel panel = panels[LINKS_PANEL] as LinkListPanel;
            panel.update_list(link_data_list);
        }

        // -----------------------------------------------------------
        // -- Frame update.
        // -----------------------------------------------------------

        // TODO(gabic): Cand schimb selectia in menu se poate in continuare sa 
        // adaug tile-uri, trebuie restrictionata operatia in functie de ce 
        // este selectat in meniu.
        
        public void update()
        {
            if (!mouse_over_ui)
            {
                if (edit_mode && map != null)
                {
                    if (Input.GetMouseButton(0) && map.over_tile != null && selected_tile_region_index >= 0)
                    {
                        // HexTile tile = map.get_tile_from_real_offset(HexMapInput.real_coordinates);
                        HexTile tile = map.over_tile;

                        if (selected_tile_region_index == 0)
                        {
                            if (tile.region != null)
                                tile.region.remove_tile(tile, true);
                        }

                        else
                        {
                            HexRegion region = map.region(selected_tile_region_index - 1);

                            if (tile != null && !region.contains(tile))
                            {
                                if (tile.region != null && tile.region.id != region.id)
                                    tile.clear_region();

                                region.add_tile(tile, true);
                            }
                        }
                    }
                }
            }

            if (Time.frameCount > 1)
            {
                // main_menu.debug();
            }

            if (map != null)
            {
                status_bar.set_coordinates(map.over_tile_rc);
                status_bar.update();
                map.update();
            }
        }
    }
}
