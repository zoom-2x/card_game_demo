using System;
using System.IO;
using System.Text;

using UnityEngine;

using CardGame.Hexgrid;
using CardGame.Data;

namespace CardGame.IO
{
    public class MapLoader
    {
        public static HexMap load(string filepath)
        {
            HexMap map = null;

            if (String.IsNullOrEmpty(filepath))
                Debug.LogWarning("MapLoader: Missing filepath !");

            else
            {
                try
                {
                    #if UNITY_ENGINE
                    string full_path = Path.Combine(Utils.basepath, "game/maps", $"{filepath}.dat");
                    #else
                    string full_path = Path.Combine(Utils.basepath, "game/maps", $"{filepath}.dat");
                    #endif

                    Debug.Log(full_path);

                    using (FileStream stream = File.Open(full_path, FileMode.Open))
                    {
                        using (BinaryReader reader = new BinaryReader(stream, Encoding.UTF8, false))
                        {
                            char[] header = reader.ReadChars(4);

                            if (header[0] != 'G' || header[1] != 'M' || header[2] != 'A' || header[3] != 'P')
                                Debug.LogWarning("MapLoader: Invalid map header !");
                            else
                            {
                                string map_name = reader.ReadString();
                                int rows = reader.ReadInt32();
                                int cols = reader.ReadInt32();
                                int tile_count = reader.ReadInt32();
                                int region_count = reader.ReadInt32();
                                int link_count = reader.ReadInt32();

                                map = new HexMap(rows, cols);
                                map.name = map_name;

                                // -----------------------------------------------------------
                                // -- Tiles.
                                // -----------------------------------------------------------

                                for (int r = 0; r < rows; ++r)
                                {
                                    for (int c = 0; c < cols; ++c)
                                    {
                                        HexTile tile = map.tile(r, c); 

                                        tile.owner = (PlayerID) reader.ReadUInt32();
                                        tile.virtual_coordinates.x = reader.ReadInt32();
                                        tile.virtual_coordinates.y = reader.ReadInt32();
                                        tile.flags = reader.ReadUInt32();
                                    }
                                }
                                
                                // -----------------------------------------------------------
                                // -- Regions.
                                // -----------------------------------------------------------

                                for (int i = 0; i < region_count; ++i)
                                {
                                    string region_name = reader.ReadString();

                                    float r = reader.ReadSingle();
                                    float g = reader.ReadSingle();
                                    float b = reader.ReadSingle();
                                    float a = reader.ReadSingle();

                                    HexRegion region = new HexRegion(new Color(r, g, b, a), region_name);

                                    uint resource_mask = reader.ReadUInt32();
                                    uint resource_value_mask = reader.ReadUInt32();

                                    region.set_resources(resource_mask, resource_value_mask);

                                    // Read the tiles.
                                    int region_tile_count = reader.ReadInt32();

                                    for (int j = 0; j < region_tile_count; ++j)
                                    {
                                        int tr = reader.ReadInt32();
                                        int tc = reader.ReadInt32();

                                        HexTile tile = map.tile(tr, tc);
                                        region.add_tile(tile);
                                    }

                                    map.add_region(region);
                                }

                                // -----------------------------------------------------------
                                // -- Links.
                                // -----------------------------------------------------------

                                Vector2Int start_rc = new Vector2Int();
                                Vector2Int middle_rc = new Vector2Int();
                                Vector2Int end_rc = new Vector2Int();

                                for (int i = 0; i < link_count; ++i)
                                {
                                    HexTileLink link = new HexTileLink();
                                    map.add_link(link);

                                    start_rc.x = reader.ReadInt32();
                                    start_rc.y = reader.ReadInt32();

                                    middle_rc.x = reader.ReadInt32();
                                    middle_rc.y = reader.ReadInt32();

                                    end_rc.x = reader.ReadInt32();
                                    end_rc.y = reader.ReadInt32();

                                    // if (i != 2)
                                        // continue;

                                    HexTile start_tile = map.get_tile_from_real_offset(start_rc);
                                    HexTile middle_tile = map.get_tile_from_real_offset(middle_rc);
                                    HexTile end_tile = map.get_tile_from_real_offset(end_rc);

                                    // Debug.Log($"id: {link.line_id}");
                                    // Debug.Log($"start: {start_rc} / middle: {middle_rc} / end: {end_rc}");

                                    // link.enable_line_debug();
                                    link.set_tiles(start_tile, middle_tile, end_tile);
                                }
                            }
                        }
                    }
                }

                catch (Exception e) {
                    Debug.Log(e.ToString());
                }
            }

            return map;
        }

        public static void save(HexMap map, string filepath)
        {
            if (map == null)
                Debug.LogWarning("MapLoader: Invalid map !");

            try
            {

                #if UNITY_ENGINE
                string full_path = Path.Combine("Assets/game/maps", $"{filepath}.dat");
                #else
                string full_path = Path.Combine(Application.dataPath, "game/maps", $"{filepath}.dat");
                #endif

                using (FileStream stream = File.Open(full_path, FileMode.Create))
                {
                    using (BinaryWriter writer = new BinaryWriter(stream, Encoding.UTF8, false))
                    {
                        // -----------------------------------------------------------
                        // -- Map header.
                        // -----------------------------------------------------------
                        
                        writer.Write('G');
                        writer.Write('M');
                        writer.Write('A');
                        writer.Write('P');

                        writer.Write(map.name);
                        writer.Write(map.rows);
                        writer.Write(map.cols);

                        writer.Write(map.rows * map.cols);
                        writer.Write(map.region_count);
                        writer.Write(map.link_count);

                        // -----------------------------------------------------------
                        // -- Tile list.
                        // -----------------------------------------------------------
                        
                        for (int row = 0; row < map.rows; ++row)
                        {
                            for (int col = 0; col < map.cols; ++col)
                            {
                                HexTile tile = map.tile(row, col);

                                writer.Write((uint) tile.owner);
                                writer.Write(tile.virtual_coordinates.x);
                                writer.Write(tile.virtual_coordinates.y);
                                writer.Write(tile.flags);
                            }
                        }

                        // -----------------------------------------------------------
                        // -- Regions.
                        // -----------------------------------------------------------

                        for (int i = 0; i < map.region_count; ++i)
                        {
                            HexRegion region = map.region(i);

                            // int: str_len
                            // char[]: name
                            // float: color_red
                            // float: color_green
                            // float: color_blue
                            // float: color_alpha
                            // int resource_mask
                            // int resource_value_mask

                            writer.Write(region.name);
                            writer.Write(region.color.r);
                            writer.Write(region.color.g);
                            writer.Write(region.color.b);
                            writer.Write(region.color.a);
                            writer.Write(region.resource_mask);
                            writer.Write(region.resource_value_mask);

                            // Write a tile list (row, column) prefixed by the tile count.
                            writer.Write(region.tile_count);

                            for (int j = 0; j < region.tile_count; ++j)
                            {
                                HexTile tile = region.tile(j);

                                writer.Write(tile.virtual_coordinates.x);
                                writer.Write(tile.virtual_coordinates.y);
                            }
                        }

                        // -----------------------------------------------------------
                        // -- Links.
                        // -----------------------------------------------------------

                        for (int i = 0; i < map.link_count; ++i)
                        {
                            HexTileLink link = map.link(i);

                            HexTile start_tile = link.tiles[0];
                            HexTile middle_tile = link.tiles[1];
                            HexTile end_tile = link.tiles[2];

                            writer.Write(start_tile.real_coordinates.x);
                            writer.Write(start_tile.real_coordinates.y);

                            writer.Write(middle_tile.real_coordinates.x);
                            writer.Write(middle_tile.real_coordinates.y);

                            writer.Write(end_tile.real_coordinates.x);
                            writer.Write(end_tile.real_coordinates.y);
                        }
                    }
                }
            }
            
            catch (Exception e) {
                Debug.Log(e.ToString());
            }
        }
    }
}
