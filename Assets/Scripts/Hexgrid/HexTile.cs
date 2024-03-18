using UnityEngine;

using CardGame.Data;
using CardGame.Managers;
using TMPro;

namespace CardGame.Hexgrid
{
    public struct HexTileEdge
    {
        public Vector3 p0;
        public Vector3 p1;
    }

    public enum HexTileState
    {
        STATE_NORMAL = 1,
        STATE_DASHED = 2,
        STATE_POCKET = 4,
    }

    public enum HexTileInfoState
    {
        STATE_HIDDEN = 1,
        STATE_ID = 2,
        STATE_COORDINATES = 3,
        STATE_RESISTANCE = 4,
    }

    public class HexTile
    {
        public const int EDGE_NE = 0;
        public const int EDGE_E = 1;
        public const int EDGE_SE = 2;
        public const int EDGE_SW = 3;
        public const int EDGE_W = 4;
        public const int EDGE_NW = 5;

        static int counter = 0;
        public static void reset_counter() {
            counter = 0;
        }

        public static Vector2Int[] edge_offsets = new Vector2Int[] {
            new Vector2Int(-1, 1),  // NE
            new Vector2Int(0, 1),   // E
            new Vector2Int(1, 0),   // SE
            new Vector2Int(1, -1),  // SW
            new Vector2Int(0, -1),  // W
            new Vector2Int(-1, 0),  // NW
        };

        public const uint FLAG_TILE = 1;
        // public const uint FLAG_TRAVERSAL = 2;

        public static Vector3[] edge_base_vectors = new Vector3[6];
        public HexTile[] neighbours = new HexTile[6];

        public int id = 0;
        int _stat_resistance = 0;

        // ----------------------------------------------------------------------------------
        // -- Packed data.
        // ----------------------------------------------------------------------------------

        public PlayerID owner = PlayerID.NONE;
        public Vector2Int virtual_coordinates = new Vector2Int();
        public Vector2Int real_coordinates = new Vector2Int();
        public uint flags = 0;

        // ----------------------------------------------------------------------------------

        public HexRegion region = null;
        public HexTileEdge[] edges = new HexTileEdge[6];

        // Locks the current tile state from changing when moving the mouse on the map.
        public bool locked_state = false;
        HexTileState _tile_state = HexTileState.STATE_NORMAL;
        public HexTileState tile_state { get {return _tile_state;} }

        Transform base_t;
        Transform tile_t;

        Transform info_base;
        TextMeshProUGUI[] info_labels = new TextMeshProUGUI[3];

        Renderer base_renderer;
        Renderer tile_renderer;

        Material mat_base_on;
        Material mat_base_off;

        Material[] mat_tile_on = new Material[(int) PlayerID.PLAYER_COUNT + 1];
        Material[] mat_tile_off = new Material[(int) PlayerID.PLAYER_COUNT + 1];
        Material[] mat_tile_dashed_on = new Material[(int) PlayerID.PLAYER_COUNT + 1];
        Material[] mat_tile_dashed_off = new Material[(int) PlayerID.PLAYER_COUNT + 1];

        public HexTile(Transform map, Vector2Int vc, Vector2Int rc, float base_scale, float tile_scale)
        {
            id = ++counter;

            virtual_coordinates = vc;
            real_coordinates = rc;

            // -----------------------------------------------------------
            // -- Base setup.
            // -----------------------------------------------------------

            base_t = GameSystems.asset_manager.aquire_hex_base();
            base_t.parent = map;
            base_t.gameObject.SetActive(true);

            info_base = base_t.GetChild(0);

            if (info_base != null)
            {
                info_base.gameObject.SetActive(true);

                info_labels[0] = info_base.GetChild(0).GetComponent<TextMeshProUGUI>();
                info_labels[1] = info_base.GetChild(1).GetComponent<TextMeshProUGUI>();
                info_labels[2] = info_base.GetChild(2).GetChild(0).GetComponent<TextMeshProUGUI>();

                info_labels[0].text = $"{id}";
                info_labels[1].text = $"{rc.x}, {rc.y}";
                
                set_tile_info_state(HexTileInfoState.STATE_HIDDEN);
            }

            base_renderer = base_t.GetComponent<Renderer>();

            mat_base_on = GameSystems.asset_manager.aquire_material(BundleEnum.MATERIALS, "hex_base_on");
            mat_base_off = GameSystems.asset_manager.aquire_material(BundleEnum.MATERIALS, "hex_base_off");

            base_state();

            // -----------------------------------------------------------
            // -- Tile setup.
            // -----------------------------------------------------------

            tile_t = GameSystems.asset_manager.aquire_hex_tile();
            tile_t.parent = map;
            tile_t.gameObject.SetActive(false);

            tile_renderer = tile_t.GetComponent<Renderer>();

            string[] tile_mat_on_names = new string[] {
                "hex_tile_on",
                "hex_tile_on_blue",
                "hex_tile_on_red",
            };

            string[] tile_mat_off_names = new string[] {
                "hex_tile_off",
                "hex_tile_off_blue",
                "hex_tile_off_red",
            };

            string[] tile_mat_dashed_on_names = new string[] {
                "hex_tile_dashed_on",
                "hex_tile_dashed_on_blue",
                "hex_tile_dashed_on_red",
            };

            string[] tile_mat_dashed_off_names = new string[] {
                "hex_tile_dashed_off",
                "hex_tile_dashed_off_blue",
                "hex_tile_dashed_off_red",
            };

            for (int i = 0; i < 3; ++i)
            {
                mat_tile_on[i] = GameSystems.asset_manager.aquire_material(BundleEnum.MATERIALS, tile_mat_on_names[i]);
                mat_tile_off[i] = GameSystems.asset_manager.aquire_material(BundleEnum.MATERIALS, tile_mat_off_names[i]);
                mat_tile_dashed_on[i] = GameSystems.asset_manager.aquire_material(BundleEnum.MATERIALS, tile_mat_dashed_on_names[i]);
                mat_tile_dashed_off[i] = GameSystems.asset_manager.aquire_material(BundleEnum.MATERIALS, tile_mat_dashed_off_names[i]);
            }

            set_tile_state(HexTileState.STATE_NORMAL);

            update_base_transform(Vector3.zero, Quaternion.identity, Vector3.one);
            update_tile_transform(Vector3.zero, Quaternion.identity, Vector3.one);

            set_default_tile_stats();
        }

        public void set_default_tile_stats()
        {
            _stat_resistance = (owner == PlayerID.NONE) ? 
                              GameConfig.game.resistance_base_empty_strength : 
                              GameConfig.game.resistance_base_strength;

            info_labels[2].text = $"{_stat_resistance}";
        }

        public int stat_resistance { get {return _stat_resistance;} }

        public void set_tile_resistance(int val, bool bonus = false)
        {
            if (bonus)
                _stat_resistance += val;
            else
                _stat_resistance = val;

            if (_stat_resistance < 0)
                _stat_resistance = 0;

            info_labels[2].text = $"{_stat_resistance}";
        }

        public void set_tile_info_state(HexTileInfoState state)
        {
            info_labels[0].gameObject.SetActive(false);
            info_labels[1].gameObject.SetActive(false);
            info_labels[2].transform.parent.gameObject.SetActive(false);

            if (!has_flag(FLAG_TILE))
                return;

            switch (state)
            {
                case HexTileInfoState.STATE_HIDDEN:
                    break;

                case HexTileInfoState.STATE_ID:
                    info_labels[0].gameObject.SetActive(true);
                    break;

                case HexTileInfoState.STATE_COORDINATES:
                    info_labels[1].gameObject.SetActive(true);
                    break;

                case HexTileInfoState.STATE_RESISTANCE:
                    info_labels[2].transform.parent.gameObject.SetActive(true);
                    break;
            }
        }

        // local-space.
        void generate_edges()
        {
            for (int i = 0; i < 6; ++i)
            {
                HexTileEdge edge = edges[i];

                edge.p0 = base_t.localPosition + edge_base_vectors[i];
                edge.p1 = base_t.localPosition + edge_base_vectors[(i + 1) % 6];

                edges[i] = edge;
            }
        }

        public void destroy()
        {
            GameSystems.asset_manager.return_hex_base(base_t.gameObject);
            GameSystems.asset_manager.return_hex_tile(tile_t.gameObject);
        }

        public Vector3 get_local_position() {
            return base_t.localPosition;
        }

        public void update_base_transform(Vector3 position, Quaternion rotation, Vector3 scale)
        {
            if (base_t == null)
                return;

            base_t.localPosition = position;
            base_t.localRotation = rotation;
            base_t.localScale = scale;

            generate_edges();
        }

        public void update_tile_transform(Vector3 position, Quaternion rotation, Vector3 scale)
        {
            if (tile_t == null)
                return;

            tile_t.localPosition = position;
            tile_t.localRotation = rotation;
            tile_t.localScale = scale;
        }

        public Transform base_transform { get{ return base_t; } }

        int owner_index {
            get { return (int) owner + 1; }
        }

        public void set_owner(PlayerID owner)
        {
            this.owner = owner;
            set_default_tile_stats();
            set_tile_state(HexTileState.STATE_NORMAL);
        }

        public void set_tile_state(HexTileState s, bool is_over = false)
        {
            // if (locked_state)
                // return;

            if (tile_renderer)
            {
                _tile_state = s;
                Material m = null;

                switch (s)
                {
                    case HexTileState.STATE_NORMAL:
                    {
                        m = is_over ? mat_tile_on[owner_index] : mat_tile_off[owner_index];
                        break;
                    }

                    case HexTileState.STATE_DASHED:
                    {
                        m = is_over ? mat_tile_dashed_on[owner_index] : mat_tile_dashed_off[owner_index];
                        break;
                    }
                }

                tile_renderer.sharedMaterial = m;
            }
        }

        public void base_state(bool is_on = false)
        {
            if (base_renderer)
                base_renderer.sharedMaterial = is_on ? mat_base_on : mat_base_off;
        }

        // public void tile_state(bool is_on = false)
        // {
        //     if (tile_renderer)
        //         tile_renderer.sharedMaterial = is_on ? mat_tile_on[owner_index] : mat_tile_off[owner_index];
        // }

        public void set_flags(uint f)
        {
            flags |= f;

            if ((f & FLAG_TILE) > 0)
                tile_t.gameObject.SetActive(true);
        }

        public void unset_flags(uint f)
        {
            flags &= ~f;

            if ((f & FLAG_TILE) > 0)
                tile_t.gameObject.SetActive(false);
        }

        public bool has_flag(uint flag) {
            return (flags & flag) > 0;
        }

        // public void enable_tile(Material mat_on, Material mat_off)
        public void enable_tile()
        {
            // mat_tile_on = mat_on;
            // mat_tile_off = mat_off;

            set_tile_state(HexTileState.STATE_NORMAL);
            set_flags(HexTile.FLAG_TILE);
        }

        public void disable_tile()
        {
            // mat_tile_on = null;
            // mat_tile_off = null;

            tile_renderer.sharedMaterial = null;
            unset_flags(HexTile.FLAG_TILE);
        }

        public bool is_border_tile(HexTile compare_tile)
        {
            if (compare_tile == null || compare_tile.region == null ||
               (region != null && compare_tile.region.id != region.id))
                return true;

            return false;
        }

        public bool is_border_tile(int edge) {
            return is_border_tile(get_neighbour(edge));
        }

        // Determine if the specified tile is on it's region border.
        public bool is_region_border()
        {
            // Debug.Log($"tile: {virtual_coordinates} / {real_coordinates}");

            // Check the neighbours (starting at NE, E, ...).
            for (int edge = 0; edge < 6; ++edge)
            {
                HexTile neighbour = neighbours[edge];

                // if (neighbour != null)
                    // Debug.Log($"neighbour: {neighbour.virtual_coordinates} / {neighbour.real_coordinates}");

                if (is_border_tile(edge))
                    return true;
            }

            return false;
        }

        public void set_region(HexRegion region)
        {
            this.region = region;
            enable_tile();
        }

        public void clear_region()
        {
            disable_tile();
            region = null;
        }

        public HexTile get_neighbour(int edge)
        {
            if (edge >= 0 && edge < 6)
                return neighbours[edge];

            return null;
        }
    }
}
