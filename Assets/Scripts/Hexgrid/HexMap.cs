using System.Collections.Generic;

using UnityEngine;
using TMPro;

using CardGame.CGDebug;
using CardGame.IO;
using CardGame.Mono;
using CardGame.Data;
using CardGame.Managers;

namespace CardGame.Hexgrid
{
    public class HexMap
    {
        public const int INVALID = 99999;

        static uint global_flags = 0;
        public static uint global {set {global_flags |= value;}} 
        public static uint FLAG_EDITOR_MODE = 1;

        bool flag(uint flag) {
            return (global_flags & flag) > 0;
        }

        // bool DEBUG_BORDERS = false;
        bool DEBUG_REGIONS = false;
        int debug_region = -1;

        // ----------------------------------------------------------------------------------
        // -- Packed data.
        // ----------------------------------------------------------------------------------

        public string name = "Unnamed map";
        public int rows = 0;
        public int cols = 0;
        public float label_hide_zoom = 5;

        HexTile[,] tiles = null;
        List<HexRegion> regions = new List<HexRegion>();
        List<HexTileLink> links = new List<HexTileLink>();

        // ----------------------------------------------------------------------------------

        Vector2Int center = Vector2Int.zero;
        Vector2Int real_center = Vector2Int.zero;

        float base_scale = 0.98f;
        float tile_scale = 0.98f;

        float radius;
        float tile_width;
        float tile_height;

        public Vector3 origin = Vector3.zero;
        Vector3 axis_0 = Vector3.zero;
        Vector3 axis_1 = Vector3.zero;

        Matrix4x4 m = Matrix4x4.identity;

        public Transform transform = null;
        GameObject map_obj = null;

        Camera camera = null;
        CameraMovement camera_movement = null;
        public Plane plane = new Plane(Vector3.up, Vector3.zero);

        // Order (pointy): E-NE-N-NW-W-SW-S-SE
        Vector2Int[] neighbours = new Vector2Int[6];
        Vector2Int[] offsets = new Vector2Int[6]
        {
            // row, col
            new Vector2Int(0, 1),
            new Vector2Int(-1, 1),
            new Vector2Int(-1, 0),
            new Vector2Int(0, -1),
            new Vector2Int(1, -1),
            new Vector2Int(1, 0)
        };

        public HexMap(int rows, int cols, float radius = 0.2f)
        {
            this.rows = rows;
            this.cols = cols;
            this.radius = radius;

            this.label_hide_zoom = GameConfig.game.label_hide_zoom;

            center.x = Mathf.FloorToInt(rows * 0.5f);
            center.y = Mathf.FloorToInt(cols * 0.5f);

            if (rows % 2 == 0)
                center.x--;

            if (cols % 2 == 0)
                center.y--;

            real_center = hex_virtual_to_real(center);

            camera = GameObject.Find("MAIN_CAMERA").GetComponent<Camera>();
            map_obj = MonoUtils.get_map();

            camera_movement = camera.GetComponent<CameraMovement>();

            transform = map_obj.transform;
            this.origin = transform.position;

            plane.normal = transform.rotation * Vector3.up;
            plane.distance = plane.GetDistanceToPoint(this.origin);

            _generate_tile_edge_base_vectors();
            _axis_setup();
            _tile_setup();
        }

        public void set_position(Vector3 p)
        {
            map_obj.transform.position = p;
            plane.SetNormalAndPosition(Vector3.up, p);
            camera_movement.set_plane(Vector3.up, p);
        }

        // clock-wise edge traversal.
        void _generate_tile_edge_base_vectors()
        {
            Vector3 start = new Vector3(0, 0, 1);
            float angle = 360 / 6;

            for (int i = 0; i < 6; ++i)
            {
                Quaternion rotation = Quaternion.Euler(0, i * angle, 0);
                HexTile.edge_base_vectors[i] = rotation * start * radius;
            }
        }

        public void destroy()
        {
            Debug.Log("> destroying map...");

            for (int i = 0; i < links.Count; ++i)
            {
                HexTileLink link = links[i];
                link.destroy();
            }

            for (int i = 0; i < regions.Count; ++i)
            {
                HexRegion region = regions[i];
                region.destroy();
            }

            for (int i = 0; i < rows; ++i)
            {
                for (int j = 0; j < cols; ++j)
                {
                    HexTile tile = tiles[i, j];
                    tile.destroy();
                }
            }
        }

        // Pointy hex tile orientation (local space).
        void _axis_setup()
        {
            float a = 2 * radius * 0.5f;
            float b = radius * 0.866025f;

            tile_width = 2 * b;
            tile_height = 2 * radius;

            axis_0.x = 2 * b;
            axis_0.y = 0;
            axis_0.z = 0;

            axis_1.x = b;
            axis_1.y = 0;
            axis_1.z = - (radius + a * 0.5f);

            m[0, 0] = axis_0.x;
            m[1, 0] = axis_0.y;
            m[2, 0] = axis_0.z;

            m[0, 1] = 0;
            m[1, 1] = 1;
            m[2, 1] = 0;

            m[0, 2] = axis_1.x;
            m[1, 2] = axis_1.y;
            m[2, 2] = axis_1.z;
        }

        void _tile_setup()
        {
            tiles = new HexTile[rows, cols];

            for (int r = 0; r < rows; ++r)
            {
                for (int c = 0; c < cols; ++c)
                {
                    Vector2Int vc = new Vector2Int(r, c);
                    Vector2Int rc = hex_virtual_to_real_offset(new Vector2Int(r, c));
                    HexTile tile = new HexTile(transform, vc, rc, base_scale, tile_scale);

                    tile.update_base_transform(
                        m.MultiplyPoint3x4(new Vector3(rc.y, 0, rc.x)),
                        Quaternion.identity,
                        new Vector3(base_scale, 1, base_scale)
                    );

                    Vector3 scale = new Vector3(tile_scale, 1, tile_scale);
                    // scale.y = Random.value * 5.0f + 0.2f;

                    tile.update_tile_transform(
                        m.MultiplyPoint3x4(new Vector3(rc.y, 0, rc.x)),
                        Quaternion.identity,
                        scale 
                    );

                    tiles[r, c] = tile;
                }
            }

            // Neighbour setup.
            for (int r = 0; r < rows; ++r)
            {
                for (int c = 0; c < cols; ++c) {
                    _tile_neighbour_setup(tiles[r, c]);
                }
            }
        }

        void _tile_neighbour_setup(HexTile tile)
        {
            for (int i = 0; i < 6; ++i)
            {
                HexTile t = get_edge_tile(tile, i);
                tile.neighbours[i] = t;
            }
        }

        // Virtual coordinates = array coordinates.
        // Real coordinates = hex grid coordinates.
        // Offset real coordinates = real coordinates relative to the grid center.

        public Vector2Int hex_virtual_to_real(Vector2Int v) {
            return new Vector2Int(v.x, v.y - (v.x >> 1));
        }

        public Vector2Int hex_real_to_virtual(Vector2Int r) {
           return new Vector2Int(r.x, r.y + (r.x >> 1));
        }

        public Vector2Int hex_virtual_to_real_offset(Vector2Int v) {
            return hex_virtual_to_real(v) - real_center;
        }

        public Vector2Int hex_real_to_virtual_offset(Vector2Int r) {
            return hex_real_to_virtual(r + real_center);
        }

        // World space.
        public Vector2Int world_point_to_real_offset(Vector3 point)
        {
            Vector3 local_space_point = map_obj.transform.InverseTransformPoint(point);
            Vector3 map_space_point = m.inverse.MultiplyPoint3x4(local_space_point);

            // Debug.Log($"world space point: {point} / local space point: {local_space_point} / map space point: {map_space_point}");

            Vector2Int tile = new Vector2Int(Mathf.RoundToInt(map_space_point.z), Mathf.RoundToInt(map_space_point.x));
            Vector3 local_space_tile_ref = m.MultiplyPoint3x4(new Vector3(tile.y, 0, tile.x));

            _get_neighbours(tile);

            Vector2Int closest_tile = tile;
            float closest_distance = (local_space_point - local_space_tile_ref).magnitude;

            for (int i = 0; i < 6; ++i)
            {
                Vector2Int current = neighbours[i];
                Vector3 local_space_current_ref = m.MultiplyPoint3x4(new Vector3(current.y, 0, current.x));

                float distance = (local_space_point - local_space_current_ref).magnitude;

                if (distance < closest_distance)
                {
                    closest_distance = distance;
                    closest_tile = current;
                }
            }

            if (!valid_real_offset(closest_tile))
            {
                closest_tile.x = INVALID;
                closest_tile.y = INVALID;
            }

            return closest_tile;
        }

        public bool valid_virtual(int row, int col) {
            return row >= 0 && row < rows && col >= 0 && col < cols;
        }

        public bool valid_real_offset(Vector2Int rc)
        {
            Vector2Int vc = hex_real_to_virtual_offset(rc);
            return valid_virtual(vc.x, vc.y);
        }

        public HexTile get_tile_from_real_offset(Vector2Int rc)
        {
            HexTile res = null;
            Vector2Int vc = hex_real_to_virtual_offset(rc);

            if (valid_virtual(vc.x, vc.y))
                res = tiles[vc.x, vc.y];

            return res;
        }

        public HexTile get_tile_from_virtual(int row, int col)
        {
            if (valid_virtual(row, col))
                return tiles[row, col];

            return null;
        }

        void _get_neighbours(Vector2Int real_tile)
        {
            for (int i = 0; i < 6; ++i)
            {
                Vector2Int v = new Vector2Int();
                v = real_tile + offsets[i];
                neighbours[i] = v;
            }
        }

        public int region_count {
            get { return regions.Count; }
        }

        public int link_count {
            get { return links.Count; }
        }

        public HexTile tile(int r, int c)
        {
            if (r >= 0 && r < rows && c >= 0 && c < cols)
                return tiles[r, c];

            return null;
        }

        public HexRegion region(int index)
        {
            if (index < 0 || index >= regions.Count)
                return null;

            return regions[index];
        }

        public HexTileLink link(int index)
        {
            if (index < 0 || index >= links.Count)
                return null;

            return links[index];
        }

        public void add_region(HexRegion region)
        {
            if (region != null)
                regions.Add(region);
        }

        public void remove_region(int index)
        {
            if (index < 0 || index >= regions.Count)
                return;

            regions.RemoveAt(index);
        }

        public void add_link(HexTileLink link)
        {
            if (link != null)
            {
                link.set_parent(map_obj.transform);
                links.Add(link);
            }
        }

        public void remove_link(int index)
        {
            if (index < 0 || index >= links.Count)
                return;

            links.RemoveAt(index);
        }

        public void add_link(Vector2Int start_tile_rc, Vector2Int middle_tile_rc, Vector2Int end_tile_rc)
        {
            HexTile start_tile = get_tile_from_real_offset(start_tile_rc);
            HexTile middle_tile = get_tile_from_real_offset(middle_tile_rc);
            HexTile end_tile = get_tile_from_real_offset(end_tile_rc);

            HexTileLink link = new HexTileLink();
            link.set_tiles(start_tile, middle_tile, end_tile);
            links.Add(link);
        }

        // -----------------------------------------------------------
        // -- Input.
        // -----------------------------------------------------------

        Vector2Int _real_coordinates = new Vector2Int(99999, 99999);

        HexTile _over_tile = null;
        HexTile _prev_over = null;

        HexRegion _over_region = null;
        HexRegion _prev_over_region = null;

        public Vector2Int over_tile_rc {
            get { return _real_coordinates; }
        }

        public HexTile over_tile {
            get { return _over_tile; }
        }

        public HexRegion over_region {
            get { return _over_region; }
        }

        public uint selection_filter_flags = 0;

        void input_update()
        {
            if (!GameSystems.input.enable)
                return;

            float dist = 0;
            Ray ray = camera.ScreenPointToRay(Input.mousePosition);
            plane.Raycast(ray, out dist);
            Vector3 intersection = ray.GetPoint(dist);

            _real_coordinates = world_point_to_real_offset(intersection);
            _over_tile = get_tile_from_real_offset(_real_coordinates);

            if (_over_tile != null)
            {
                _over_region = _over_tile.region;
                bool has_filter = (selection_filter_flags > 0);

                // Only allow the tiles specified in the filter to be selected.
                if (!has_filter || (has_filter && (selection_filter_flags & (uint) _over_tile.tile_state) > 0))
                {
                    if (_prev_over != null)
                    {
                        _prev_over.base_state();
                        _prev_over.set_tile_state(_prev_over.tile_state);
                    }

                    if (_prev_over_region != null)
                        _prev_over_region.set_region_state(HexRegion.STATE_LIGHT, true);

                    _prev_over = _over_tile;
                    _prev_over.base_state(true);
                    _prev_over.set_tile_state(_prev_over.tile_state, true);

                    if (_over_region != null)
                        _over_region.set_region_state(HexRegion.STATE_LIGHT);

                    _prev_over_region = _over_region;
                }
                else
                {
                    _over_tile = null;
                    _over_region = null;
                }
            }

            else
            {
                if (_prev_over != null)
                {
                    _prev_over.base_state();
                    _prev_over.set_tile_state(_prev_over.tile_state);

                    if (_prev_over_region != null)
                        _prev_over_region.set_region_state(HexRegion.STATE_LIGHT, true);
                }

                _prev_over = null;
                _prev_over_region = null;
            }

            // Input mappings.
            // if (Input.GetKeyDown("f1"))
                // toggle_labels();

            // if (Input.GetKeyDown("f2") && EDITOR_MODE)
                // toggle_region_colors();
        }

        // -----------------------------------------------------------

        bool labels_visible = true;

        void toggle_labels()
        {
            if (camera_movement != null && camera_movement.zoom > label_hide_zoom)
                return;

            for (int i = 0; i < regions.Count; ++i)
            {
                HexRegion region = regions[i];

                if (region.tile_count == 0)
                    continue;

                if (labels_visible)
                    region.region_label.hide();
                else
                    region.region_label.show();

            }

            labels_visible = !labels_visible;
        }

        void toggle_region_colors()
        {
            for (int i = 0; i < regions.Count; ++i)
            {
                HexRegion region = regions[i];
                region.toggle_color();
            }
        }

        // Returns the neighbouring tile for the specified edge.
        HexTile get_edge_tile(HexTile tile, int edge)
        {
            HexTile edge_tile = null;

            if (edge >= 0 && edge < 6)
            {
                Vector2Int edge_tile_rc = tile.real_coordinates + HexTile.edge_offsets[edge];
                edge_tile = get_tile_from_real_offset(edge_tile_rc);
            }

            return edge_tile;
        }

        public void generate_borders()
        {
            for (int i = 0; i < regions.Count; ++i)
            {
                HexRegion region = regions[i];
                region.generate_border();

                // if (DEBUG_REGIONS && (debug_region == -1 || (debug_region == region.id)))
                    // region.debug_region_border(transform);
            }
        }

        public void generate_region_meshes()
        {
            for (int i = 0; i < regions.Count; ++i)
            {
                HexRegion region = regions[i];
                region.generate_region_mesh();

                if (DEBUG_REGIONS && (debug_region == -1 || (debug_region == region.id)))
                    region.debug_region_mesh(transform);
            }
        }

        public void update()
        {
            input_update();

            for (int i = 0; i < regions.Count; ++i)
            {
                HexRegion region = regions[i];
                region.region_label.update_ui_position();

                if (region.tile_count == 0)
                    continue;

                if (camera_movement != null)
                {
                    if (camera_movement.zoom > label_hide_zoom)
                        region.region_label.hide();
                    else if (labels_visible)
                        region.region_label.show();
                }

                // if (DEBUG_REGIONS && (debug_region == -1 || (debug_region == region.id)))
                    // region.debug_region_mesh_connections(transform);
            }
        }

        // Aici poate ar fi o idee mai buna sa comasez flag-ul pentru o resetare
        // mai rapida ?.
        public void reset_tile_state()
        {
            for (int row = 0; row < rows; ++row)
            {
                for (int col = 0; col < cols; ++col)
                {
                    HexTile tile = get_tile_from_virtual(row, col);

                    tile.locked_state = false;
                    tile.set_tile_state(HexTileState.STATE_NORMAL);
                }
            }
        }

        public void set_tile_info_state(HexTileInfoState state)
        {
            for (int row = 0; row < rows; ++row)
            {
                for (int col = 0; col < cols; ++col)
                {
                    HexTile tile = get_tile_from_virtual(row, col);
                    tile.set_tile_info_state(state);
                }
            }
        }

        public void set_default_tile_info_state() {
            set_tile_info_state(GameSystems.game.current_player.default_tile_info_state);
        }
    }
}
