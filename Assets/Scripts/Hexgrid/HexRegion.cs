using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using TMPro;
using Random = UnityEngine.Random;

using CardGame.IO;
using CardGame.Hexgrid;
using CardGame.Managers;
using CardGame.Mono;
using CardGame.Data;
using gc_components;

namespace CardGame.Hexgrid
{
    public enum HexRegionInfoState
    {
        STATE_HIDDEN = 1,
        STATE_ID = 2,
        STATE_RESISTANCE = 3,
    }

    public class HexRegion
    {
        static uint global_flags = 0;
        public static uint global {set {global_flags |= value;}}

        bool flag(uint flag) {
            return (global_flags & flag) > 0;
        }

        public const uint FLAG_EDITOR_MODE = 1;
        public const uint FLAG_CURVED_BORDER = 2;

        static int counter = 0;

        public const uint STATE_NORMAL = 1;
        public const uint STATE_DASHED = 2;
        public const uint STATE_LIGHT = 4;

        public const uint RES_BLUE = 0;
        public const uint RES_RED = 1;
        public const uint RES_GREEN = 2;
        public const uint RES_PURPLE = 3;
        public const uint RES_GOLD = 4;
        public const uint RES_COUNT = 5;

        public const ushort RES_VALUE_BITS = 3;
        public static uint[] RESOURCE_MASK = new uint[5] {1, 2, 4, 8, 16};

        public int id = 0;

        // -----------------------------------------------------------
        // -- Packed data.
        // -----------------------------------------------------------

        // public int tile_count;
        public string name = "";
        public Color color = Color.white;
        public uint resource_mask = 0;
        public uint resource_value_mask = 0;

        public List<HexTile> tile_list = new List<HexTile>();

        // -----------------------------------------------------------

        GameObject map_obj = null;

        public RegionLabel region_label = null;

        public StraightLine border = null;
        public ProceduralLine border_curved = null;
        public List<HexTileEdge> border_edges = new List<HexTileEdge>();

        Polygon _mesh_polygon;
        Transform region_mesh_prefab;
        MeshFilter region_mesh_filter;
        Renderer region_mesh_renderer;
        RegionMeshInfo region_mesh_info;

        public PlayerID owner = PlayerID.NONE;
        int owner_index {
            get { return (int) owner + 1; }
        }

        public bool locked_state = false;
        uint _region_state = STATE_NORMAL;

        string[] region_mat_over_names = new string[] {
            "region_over",
            "region_over_blue",
            "region_over_red"
        };

        string[] region_mat_dashed_names = new string[] {
            "region_dashed",
            "region_dashed_blue",
            "region_dashed_red"
        };

        string[] region_mat_dashed_light_names = new string[] {
            "region_dashed_light",
            "region_dashed_light_blue",
            "region_dashed_light_red"
        };

        Material[] region_mat_over = new Material[(int) PlayerID.PLAYER_COUNT + 1];
        Material[] region_mat_dashed = new Material[(int) PlayerID.PLAYER_COUNT + 1];
        Material[] region_mat_dashed_light = new Material[(int) PlayerID.PLAYER_COUNT + 1];
        
        Transform mesh;

        public Polygon mesh_polygon {get {return _mesh_polygon;}}

        bool color_enabled = false;

        public HexRegion(Color color, string name = "Default Region Name")
        {
            id = ++counter;

            map_obj = GameObject.Find("MAP");

            // -----------------------------------------------------------
            // -- Region mesh.
            // -----------------------------------------------------------

            region_mesh_prefab = GameSystems.asset_manager.aquire_region_mesh();
            region_mesh_prefab.parent = map_obj.transform;
            region_mesh_prefab.localPosition += new Vector3(0, 0.03f, 0);

            region_mesh_info = new RegionMeshInfo(region_mesh_prefab);

            for (int i = 0; i < 3; ++i) {
                region_mesh_info.set_value(i, (int) (Random.value * 10));
            }

            mesh = region_mesh_prefab.Find("mesh"); 

            region_mesh_filter = mesh.GetComponent<MeshFilter>();
            region_mesh_renderer = mesh.GetComponent<Renderer>();

            for (int i = 0; i < 3; ++i)
            {
                region_mat_over[i] = GameSystems.asset_manager.aquire_material(BundleEnum.MATERIALS, region_mat_over_names[i]);
                region_mat_dashed[i] = GameSystems.asset_manager.aquire_material(BundleEnum.MATERIALS, region_mat_dashed_names[i]);
                region_mat_dashed_light[i] = GameSystems.asset_manager.aquire_material(BundleEnum.MATERIALS, region_mat_dashed_light_names[i]);
            }

            set_region_state(STATE_NORMAL);

            if (flag(FLAG_EDITOR_MODE))
            {
                Debug.Log("RegionLabelUT init...");
                region_label = new RegionLabelUT();
                region_mesh_prefab.gameObject.SetActive(false);
            }
            else
            {
                Debug.Log("RegionLabelUI init...");
                region_label = new RegionLabelUI();
            }

            // -----------------------------------------------------------
            // -- Curved border config.
            // -----------------------------------------------------------

            border_curved = GameSystems.asset_manager.aquire_hex_region_border_v2();
            border_curved.config_tile_size = 0.12f;
            border_curved.config_thickness = 0.04f;
            border_curved.config_circle_radius = 0.03f;
            border_curved.config_tilt_multiplier = 0.4f;
            border_curved.config_tilt_value = 0.3f;

            border_curved.transform.parent = map_obj.transform;

            Vector3 border_position = border_curved.transform.position;
            border_position.y = 0.028f;
            border_curved.transform.position = border_position;

            // -----------------------------------------------------------
            // -- Straight border config.
            // -----------------------------------------------------------

            border = GameSystems.asset_manager.aquire_hex_region_border();

            border.closed = true;
            border.cw = true;
            border.inner_border = true;
            border.width = 0.03f;
            border.inner_border_offset = 0.01f;
            border.transform.parent = map_obj.transform;

            border_position = border.transform.position;
            border_position.y = 0.028f;
            border.transform.position = border_position;

            border_curved.gameObject.SetActive(flag(FLAG_CURVED_BORDER));
            border.gameObject.SetActive(!flag(FLAG_CURVED_BORDER));

            // -----------------------------------------------------------

            change_name(name);
            update_label();
        }

        public void set_owner(PlayerID owner)
        {
            this.owner = owner;
            set_region_state(STATE_NORMAL);
        }

        public void set_region_state(uint s, bool disable = false)
        {
            uint mask = STATE_NORMAL | STATE_DASHED;

            if (disable)
                _region_state &= ~s;
            else
            {
                if ((s & mask) > 0)
                    _region_state &= ~mask;

                _region_state |= s;
            }

            update_region_state();
        }

        void update_region_state()
        {
            // if (locked_state)
                // return;

            if (region_mesh_renderer)
            {
                region_mesh_renderer.gameObject.SetActive(true);
                bool is_light = (_region_state & STATE_LIGHT) > 0;

                if ((_region_state & STATE_NORMAL) > 0)
                {
                    if (is_light)
                        region_mesh_renderer.sharedMaterial = region_mat_over[owner_index];
                    else
                        region_mesh_renderer.gameObject.SetActive(false);
                }

                if ((_region_state & STATE_DASHED) > 0)
                {
                    if (is_light)
                        region_mesh_renderer.sharedMaterial = region_mat_dashed_light[owner_index];
                    else
                        region_mesh_renderer.sharedMaterial = region_mat_dashed[owner_index];
                }
            }
        }

        HexTile find_border_tile()
        {
            for (int i = 0; i < tile_count; ++i)
            {
                HexTile t = tile(i);

                if (t.is_region_border())
                    return t;
            }

            return null;
        }

        void debug_tile_region()
        {
            for (int i = 0; i < tile_count; ++i)
            {
                HexTile t = tile(i);
                Debug.Log($"region id: {t.region.id}");
            }
        }

        public void generate_border()
        {
            border_edges.Clear();

            // Add the points in the border component.
            if (flag(FLAG_CURVED_BORDER))
                border_curved.clear_points();
            else
                border.clear_points();

            // -----------------------------------------------------------
            // -- Compute the border points.
            // -----------------------------------------------------------

            if (tile_count > 0)
            {
                HexTile border_tile = find_border_tile();
                HexTile starting_tile = border_tile;

                if (border_tile != null)
                {
                    // Clockwise traversal.
                    int start_edge = HexTile.EDGE_NE;
                    int transition_edge = -1;
                    int count = 0;

                    // Checks if the border edge has reach the starting point (tile_id, edge).
                    Vector2Int start = new Vector2Int(starting_tile.id, -1);

                    while (true)
                    {
                        bool added = false;
                        int added_count = 0;
                        transition_edge = -1;

                        // Cycle through the tile's edges and determine which one is a border.
                        while (true)
                        {
                            int edge = (start_edge++) % 6;

                            if (border_tile.id == start.x && edge == start.y)
                            {
                                transition_edge = -1;
                                break;
                            }

                            bool is_edge = border_tile.is_border_tile(edge);

                            // Add the edge to the border edge list.
                            if (is_edge)
                            {
                                added = true;
                                added_count++;

                                HexTileEdge e = border_tile.edges[edge];
                                border_edges.Add(e);

                                if (start.y == -1)
                                    start.y = edge;
                            }

                            // A transition edge was reached, time to switch to a new border tile.
                            else if (added)
                            {
                                transition_edge = edge;
                                break;
                            }

                            // The region contains a single hex.
                            if (added_count == 6)
                                break;
                        }

                        // Switch to the next border tile.
                        if (transition_edge != -1)
                        {
                            border_tile = border_tile.get_neighbour(transition_edge);
                            start_edge = (transition_edge + 4) % 6;
                        }
                        else
                            break;

                        count++;

                        if (count > 1000)
                        {
                            Debug.Log("infinite");
                            break;
                        }
                    }
                }
            }

            // -----------------------------------------------------------
            // -- Generate the border mesh.
            // -----------------------------------------------------------

            for (int i = 0; i < border_edges.Count; ++i)
            {
                HexTileEdge edge = border_edges[i];

                if (flag(FLAG_CURVED_BORDER))
                    border_curved.add_point(edge.p0);
                else
                    border.add_point(edge.p0);
            }

            if (flag(FLAG_CURVED_BORDER))
                border_curved.generate();
            else
                border.generate();
        }

        // -----------------------------------------------------------
        // -- Region mesh generator.
        // -----------------------------------------------------------

        List<HexTileEdge> edge_buffer = new List<HexTileEdge>();
        List<List<HexTileEdge>> chains = new List<List<HexTileEdge>>();
        Mesh region_mesh;

        public void generate_region_mesh()
        {
            edge_buffer.Clear();
            DebugFile debugger = DebugFile.get_instance();

            List<Polygon> polygons = new List<Polygon>();
            int outside_polygon = -1;
            int outside_count = -1;

            // -----------------------------------------------------------
            // -- Collect the edges.
            // -----------------------------------------------------------

            for (int i = 0; i < tile_list.Count; ++i)
            {
                HexTile tile = tile_list[i];

                for (int edge = 0; edge < 6; ++edge)
                {
                    if (tile.is_border_tile(edge))
                        edge_buffer.Add(tile.edges[edge]);
                }
            }

            // -----------------------------------------------------------
            // -- Order the edges and find out if the region has a hole.
            // -----------------------------------------------------------

            List<HexTileEdge> current_chain = null;

            while (true)
            {
                if (current_chain == null)
                {
                    List<HexTileEdge> tl = new List<HexTileEdge>();
                    current_chain = tl;
                    chains.Add(tl);
                }

                if (current_chain.Count == 0)
                {
                    HexTileEdge edge = edge_buffer[0];
                    edge_buffer.RemoveAt(0);
                    current_chain.Add(edge);
                }

                else
                {
                    bool added = false;

                    for (int i = 0; i < edge_buffer.Count; ++i)
                    {
                        HexTileEdge check_edge = edge_buffer[i];

                        if (is_edge_chain(current_chain, check_edge))
                        {
                            current_chain.Add(check_edge);
                            edge_buffer.RemoveAt(i);
                            added = true;

                            break;
                        }
                    }

                    if (!added)
                    {
                        if (current_chain.Count < 3)
                        {
                            Debug.Log($"Invalid chain: {current_chain.Count} !");
                            break;
                        }

                        // Start a new chain for the rest of the edges.
                        if (edge_buffer.Count > 0)
                            current_chain = null;
                        
                        else 
                        {
                            if (!is_closed_chain(current_chain))
                                Debug.Log("Chain was not closed !");

                            break;
                        }
                    }
                }
            }

            // Generate the region's polygons.
            for (int i = 0; i < chains.Count; ++i)
            {
                Polygon p = Algorithm.create_polygon(chains[i]);
                polygons.Add(p);

                if (p.points.Count > outside_count)
                {
                    outside_polygon = i;
                    outside_count = p.points.Count;
                }
            }

            if (outside_polygon > -1)
            {
                Polygon outer = polygons[outside_polygon];
                _mesh_polygon = outer;
                polygons.RemoveAt(outside_polygon);

                // -----------------------------------------------------------
                // -- Attach the inner polygons to the outer one.
                // -----------------------------------------------------------

                for (int i = 0; i < polygons.Count; ++i)
                {
                    Polygon inner = polygons[i];
                    // x: outer connection point, y: inside connection point
                    Vector2Int indices = Algorithm.closest_polygon_point(outer, inner);
                    int insertion_point = -1;

                    // Brute search for the insertion index.
                    for (int k = 0; k < outer.indices.Count; ++k)
                    {
                        if (indices.x == outer.indices[k])
                        {
                            insertion_point = k + 1;
                            break;
                        }
                    }

                    int in_first_index = -1;

                    // -----------------------------------------------------------
                    // -- Add the inner polygon's points to the outer polygon
                    // -----------------------------------------------------------

                    for (int j = 0, k = indices.y; j < inner.points.Count; ++j, ++k)
                    {
                        int ti = k % inner.points.Count;

                        if (insertion_point >= outer.indices.Count)
                        {
                            outer.points.Add(inner.points[ti]);
                            outer.indices.Add(outer.points.Count - 1);

                            if (in_first_index == -1)
                                in_first_index = outer.points.Count - 1;

                            if (j == inner.points.Count - 1)
                            {
                                outer.indices.Add(in_first_index);
                                outer.indices.Add(indices.x);
                            }
                        }

                        else
                        {
                            outer.points.Add(inner.points[ti]);
                            outer.indices.Insert(insertion_point + j, outer.points.Count - 1);

                            if (in_first_index == -1)
                                in_first_index = outer.points.Count - 1;

                            if (j == inner.points.Count - 1)
                            {
                                outer.indices.Insert(insertion_point + j + 1, in_first_index);
                                outer.indices.Insert(insertion_point + j + 2, indices.x);
                            }
                        }
                    }
                }

                // -----------------------------------------------------------
                // -- Mesh generation.
                // -----------------------------------------------------------

                if (region_mesh == null)
                {
                    region_mesh = new Mesh();
                    region_mesh.name = "Region overlay mesh";
                }

                if (_mesh_polygon != null)
                {
                    Algorithm.offset(_mesh_polygon);

                    Algorithm.triangulate_ear_clipping(_mesh_polygon);
                    Algorithm.generate_uvs(_mesh_polygon);

                    region_mesh.Clear();
                    region_mesh.SetVertices(_mesh_polygon.points.ToArray());
                    region_mesh.SetUVs(0, _mesh_polygon.uvs.ToArray());
                    region_mesh.SetTriangles(_mesh_polygon.indices.ToArray(), 0);
                    region_mesh_filter.sharedMesh = region_mesh;

                    Vector3 pos = region_mesh_prefab.localPosition;

                    pos.x = _mesh_polygon.origin.x;
                    pos.y = 0.03f;
                    pos.z = _mesh_polygon.origin.z;

                    region_mesh_prefab.localPosition = pos;
                }

                debugger.flush();
            }
        }

        // Assuming clockwise edges.
        bool is_edge_chain(List<HexTileEdge> chain, HexTileEdge edge)
        {
            HexTileEdge last = chain[chain.Count - 1];
            return last.p1 == edge.p0;
        }

        bool is_closed_chain(List<HexTileEdge> chain)
        {
            if (chain.Count == 0)
                return false;

            return chain[0].p0 == chain[chain.Count - 1].p1;
        }

        void check_min(ref Vector2 min, Vector3 p)
        {
            if (p.x < min.x)
                min.x = p.x;

            if (p.z < min.y)
                min.y = p.z;
        }

        void check_max(ref Vector2 max, Vector3 p)
        {
            if (p.x > max.x)
                max.x = p.x;

            if (p.z > max.y)
                max.y = p.z;
        }

        // -----------------------------------------------------------

        public void toggle_color() {
            color_enabled = !color_enabled;
        }

        public void change_name(string name)
        {
            this.name = name;
            region_mesh_info.set_name(name);
            region_label.set_name(name);
        }

        void update_label()
        {
            if (tile_list.Count == 0)
            {
                region_label.hide();
                return;
            }

            region_label.show();
            Vector3 median_local_position = Vector3.zero;

            for (int i = 0; i < tile_list.Count; ++i)
            {
                HexTile tile = tile_list[i];
                median_local_position += tile.get_local_position();
            }

            median_local_position /= tile_list.Count;
            median_local_position.y = 0.2f;

            region_label.set_object_position(median_local_position);
        }

        public bool contains(HexTile tile) {
            return tile_list.Contains(tile);
        }

        public void add_tile(HexTile tile, bool regenerate = false)
        {
            if (tile == null || tile_list.Contains(tile))
                return;

            // tile.enable_tile(tile_mat_on, tile_mat_off);
            tile.set_region(this);
            tile_list.Add(tile);

            tile.region = this;
            update_label();

            if (regenerate)
                generate_border();
        }

        public void remove_tile(HexTile tile, bool regenerate = false)
        {
            if (tile == null)
                return;

            tile.clear_region();

            if (!tile_list.Contains(tile))
                return;

            tile_list.Remove(tile);
            update_label();

            Debug.Log($"remove count: {tile_count}");

            if (regenerate)
                generate_border();
        }

        public int tile_count {
            get { return tile_list.Count; }
        }

        public HexTile tile(int index)
        {
            if (index < 0 || index >= tile_count)
                return null;

            return tile_list[index];
        }

        public void destroy()
        {
            Debug.Log("> destroying region...");

            // Material.Destroy(tile_mat_on);
            // Material.Destroy(tile_mat_off);

            border_edges.Clear();
            tile_list.Clear();
            region_label.destroy();

            GameSystems.asset_manager.return_hex_region_border(border.gameObject);
            GameSystems.asset_manager.return_hex_region_border_v2(border_curved.gameObject);
        }

        public bool has_resource(uint resource) {
            return (RESOURCE_MASK[resource] & resource_mask) > 0;
        }

        public void set_resource(uint resource, uint value)
        {
            ushort shift = (ushort) (RES_VALUE_BITS * resource);

            if (value == 0)
            {
                resource_mask |= ~RESOURCE_MASK[resource];
                resource_value_mask &= (uint) ~(7 << shift);
            }

            else
            {
                resource_mask |= RESOURCE_MASK[resource];
                resource_value_mask |= (uint) ((value & 7) << shift);
            }
        }

        public uint get_resource(uint resource) {
            return (resource_value_mask >> (ushort) (resource * RES_VALUE_BITS)) & 7;
        }

        public void set_resources(uint resource_mask, uint resource_value_mask)
        {
            this.resource_mask = resource_mask;
            this.resource_value_mask = resource_value_mask;

            for (int i = 0; i < RES_COUNT; ++i) {
                region_label.set_value(i, get_resource((uint) i));
            }
        }

        // -----------------------------------------------------------
        // -- Statics (used in the map editor).
        // -----------------------------------------------------------

        public static bool has_resource(uint resource, uint mask) {
            return (RESOURCE_MASK[resource] & mask) > 0;
        }

        // No resource index bounds check.
        public static uint get_resource_mask(uint resource) {
            return RESOURCE_MASK[resource];
        }

        // No resource index bounds check.
        public static uint get_resource_value_mask(uint resource, ushort value)
        {
            ushort shift = (ushort) (RES_VALUE_BITS * resource);
            return (uint) ((value & 7) << shift);
        }

        public static uint extract_resource_value(uint resource, uint mask) {
            return (mask >> (ushort) (resource * RES_VALUE_BITS)) & 7;
        }

        // -----------------------------------------------------------
        // -- Debugging routines.
        // -----------------------------------------------------------

        Vector3 debug_offset_y = new Vector3(0, 0.03f, 0);
        Vector3 debug_point_scale = new Vector3(0.1f, 0.1f, 0.1f);
        List<Transform> debug_points = new List<Transform>();

        public void debug_region_border(Transform map)
        {
            for (int i = 0; i < this.border_edges.Count; ++i)
            {
                HexTileEdge edge = this.border_edges[i];

                if (i == 0)
                {
                    Transform p0_t = GameSystems.asset_manager.aquire_debug_point();
                    Transform p1_t = GameSystems.asset_manager.aquire_debug_point();

                    p0_t.GetChild(0).gameObject.SetActive(false);
                    p1_t.GetChild(0).gameObject.SetActive(false);

                    debug_points.Add(p0_t);
                    debug_points.Add(p1_t);

                    p0_t.localScale = debug_point_scale;
                    p1_t.localScale = debug_point_scale;

                    p0_t.position = map.TransformPoint(edge.p0);
                    p1_t.position = map.TransformPoint(edge.p1);
                }

                else
                {
                    Transform p1_t = GameSystems.asset_manager.aquire_debug_point();
                    p1_t.GetChild(0).gameObject.SetActive(false);

                    p1_t.localScale = debug_point_scale;
                    p1_t.position = map.TransformPoint(edge.p1);

                    debug_points.Add(p1_t);
                }
            }
        }

        public void debug_region_mesh(Transform map)
        {
            if (this.mesh_polygon != null)
            {
                for (int i = 0; i < this.mesh_polygon.points.Count; ++i)
                {
                    Vector3 point = this.mesh_polygon.points[i] + debug_offset_y;
                    Transform pt = GameSystems.asset_manager.aquire_debug_point();

                    Transform debug_canvas = pt.GetChild(0);
                    debug_canvas.gameObject.SetActive(true);
                    TextMeshProUGUI t = debug_canvas.GetChild(0).GetComponent<TextMeshProUGUI>();

                    Vector2 uv = this.mesh_polygon.uvs[i];

                    // t.text = $"{i}";
                    t.text = String.Format("{0:F2} / {1:F2}", uv.x, uv.y);

                    pt.localScale = debug_point_scale;
                    pt.position = map.TransformPoint(point);

                    debug_points.Add(pt);
                }
            }
        }

        public void debug_region_mesh_connections(Transform map)
        {
            Polygon p = this.mesh_polygon;
            float duration = 0;

            if (p != null)
            {
                for (int j = 0; j < p.indices.Count; j += 3)
                {
                    int i0 = j;
                    int i1 = (j + 1) % p.indices.Count;
                    int i2 = (j + 2) % p.indices.Count;

                    Vector3 p0 = map.TransformPoint(p.points[p.indices[i0]] + debug_offset_y);
                    Vector3 p1 = map.TransformPoint(p.points[p.indices[i1]] + debug_offset_y);
                    Vector3 p2 = map.TransformPoint(p.points[p.indices[i2]] + debug_offset_y);

                    Debug.DrawLine(p0, p1, Color.green, duration, false);
                    Debug.DrawLine(p1, p2, Color.green, duration, false);
                    Debug.DrawLine(p2, p0, Color.green, duration, false);
                }
            }
        }

        public void debug_cleanup()
        {
            for (int i = 0; i < debug_points.Count; ++i) {
                GameSystems.asset_manager.return_debug_point(debug_points[i].gameObject);
            }
        }
    }
}
