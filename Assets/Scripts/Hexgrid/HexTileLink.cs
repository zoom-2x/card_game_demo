using UnityEngine;
using CardGame.Managers;

using gc_components;

namespace CardGame.Hexgrid
{
    public class HexTileLink
    {
        ProceduralLine line = null;
        public HexTile[] tiles = new HexTile[3];

        public HexTileLink()
        {
            line = GameSystems.asset_manager.aquire_hex_tile_link();
            line.gameObject.SetActive(false);
        }

        public int line_id { get {return line.id;} }

        public void enable_line_debug() {
            line.enable_debug = true;
        }

        public bool is_valid()
        {
            if (tiles[0] == null || tiles[1] == null || tiles[2] == null)
                return false;

            if (!tiles[0].has_flag(HexTile.FLAG_TILE) ||
                !tiles[2].has_flag(HexTile.FLAG_TILE))
                return false;

            return true;
        }

        public void set_tiles(HexTile start_tile, HexTile middle_tile, HexTile end_tile)
        {
            tiles[0] = start_tile;
            tiles[1] = middle_tile;
            tiles[2] = end_tile;

            if (is_valid())
            {
                line.gameObject.SetActive(true);

                Transform p0t = tiles[0].base_transform;
                Transform p1t = tiles[1].base_transform;
                Transform p2t = tiles[2].base_transform;

                line.set_position(p0t.position, 0);
                line.set_position(p1t.position, 1);
                line.set_position(p2t.position, 2);

                line.update_line();
            }
        }

        public void set_parent(Transform p) {
            line.transform.parent = p;
        }

        public void destroy()
        {
            tiles[0] = null;
            tiles[1] = null;
            tiles[2] = null;

            GameSystems.asset_manager.return_hex_tile_link(line.gameObject);
        }
    }
}

