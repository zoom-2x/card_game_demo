using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEditor;

using CardGame.IO;

namespace gc_components
{
    [ExecuteAlways]
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    public class ProceduralLine : MonoBehaviour
    {
        protected struct LineJoinCircle
        {
            public Vector3 origin;
            public float radius;
            public float angle;

            public Vector3 join_point_0;
            public Vector3 join_point_1;

            public bool cw;
        }

        protected struct Line
        {
            public Vector3 p0;
            public Vector3 p1;
            public Vector3 direction;
            public Vector3 normal;
        }

        static int _counter = 0;
        private Mesh mesh;

        List<Vector3> base_points = new List<Vector3>();
        List<Vector3> points = new List<Vector3>();
        List<Line> lines = new List<Line>();
        List<float> distances = new List<float>();

        public int id;
        public bool enable_debug = false;

        // ----------------------------------------------------------------------------------
        // -- Line config fields.
        // ----------------------------------------------------------------------------------

        public bool config_show_handles = true;
        public bool config_closed = false;
        public bool config_reversed = false;
        public float config_tile_size = 1.0f;
        [Range(0.01f, 4.0f)]
        public float config_thickness = 0.5f;
        [Range(0.01f, 4.0f)]
        public float config_circle_radius = 1.0f;
        [Range(2, 64)]
        public int config_join_segments = 4;
        [Range(0.0f, 1.0f)]
        public float config_tilt_multiplier = 0.0f;
        public float config_tilt_value = 0.05f;
        public bool config_tilt_reversed = false;
        public Texture config_texture = null;

        void Awake()
        {
            id = ++_counter;
        }

        void OnEnable()
        {
            mesh = new Mesh();
            mesh.name = "Procedural line (curved)";
            GetComponent<MeshFilter>().sharedMesh = mesh;

            generate();
        }

        void OnDisable()
        {
            // Debug.Log("ProceduralLine: disabled");
            clear_points();

            #if UNITY_EDITOR
                Object.DestroyImmediate(mesh);
            #else
                Object.Destroy(mesh);
            #endif
        }

        void Update()
        {
            #if UNITY_EDITOR
            if (!Application.isPlaying)
                update_line();
            #endif
        }

        // Update a node's position.
        public void set_position(Vector3 position, int index)
        {
            if (index < 0 || index >= transform.childCount)
                return;

            transform.GetChild(index).position = position;
        }

        public void update_line()
        {
            update_positions();
            generate();
        }

        Vector3[] dir_positions = new Vector3[2];

        void update_positions()
        {
            dir_positions[0] = Vector3.zero;
            dir_positions[1] = Vector3.zero;

            if (transform.childCount > 0)
            {
                int index_0 = transform.childCount - 1;
                int index_1 = transform.childCount - 2;

                dir_positions[0] = transform.GetChild(index_0).localPosition;

                if (index_1 >= 0)
                    dir_positions[1] = transform.GetChild(index_1).localPosition;
            }
        }

    #if UNITY_EDITOR
        List<Vector3> tmp_list = new List<Vector3>();

        void show_node_handles()
        {
            foreach (Transform t in transform)
            {
                ProceduralLineNode node = t.gameObject.GetComponent<ProceduralLineNode>();

                if (node != null)
                    node.show_handles = config_show_handles;
            }
        }

        void OnDrawGizmos()
        {
            Gizmos.color = Color.white;
            Gizmos.DrawCube(transform.position, new Vector3(0.1f, 0.1f, 0.1f));

            show_node_handles();

            if (config_show_handles)
            {
                tmp_list.Clear();

                foreach (Transform node in transform) {
                    tmp_list.Add(node.position);
                }

                Gizmos.color = Color.grey;
                for (int i = 0; i < tmp_list.Count; ++i) {
                    // Gizmos.DrawLine(tmp_list[i], tmp_list[i + 1]);
                    Handles.DrawDottedLine(tmp_list[i], tmp_list[(i + 1) % tmp_list.Count], 3.0f);
                }
            }
        }

        void OnDrawGizmosSelected()
        {}

        Vector3 get_next_position(float dist = 1.0f)
        {
            Vector3 dir = dir_positions[0] - dir_positions[1];

            if (dir == Vector3.zero)
                dir = new Vector3(1, 0, 0);

            Vector3 point = dir_positions[0] + dist * dir.normalized;

            return point;
        }

        public void add_node()
        {
            GameObject node = new GameObject("LineNode");
            node.AddComponent(typeof(ProceduralLineNode));
            node.transform.parent = transform;

            if (transform.childCount == 0)
                node.transform.localPosition = Vector3.zero;
            else
                node.transform.localPosition = get_next_position();
        }
    #endif

        Line get_line(Vector3 p0, Vector3 p1)
        {
            Line line = new Line();

            line.p0 = p0;
            line.p1 = p1;
            line.direction = (p1 - p0).normalized;
            line.normal = new Vector3(line.direction.normalized.y, -line.direction.normalized.x, 0);

            return line;
        }

        LineJoinCircle compute_join(Line l0, Line l1, float radius)
        {
            LineJoinCircle join = new LineJoinCircle();

            float dot = Mathf.Clamp(Vector3.Dot(-l0.direction, l1.direction), -1, 1);
            float angle = Mathf.Acos(dot);
            float half_angle = 0.5f * angle;

            join.cw = Vector3.Dot(Vector3.Cross(l0.direction, l1.direction), Vector3.forward) < 0;
            join.radius = radius;
            join.angle = Mathf.PI - angle;

            float s = radius * Mathf.Cos(half_angle) / Mathf.Sin(half_angle);

            join.join_point_0 = l0.p1 - l0.direction * s;
            join.join_point_1 = l1.p0 + l1.direction * s;

            if (join.cw)
                join.origin = join.join_point_0 + l0.normal * radius;
            else
                join.origin = join.join_point_0 - l0.normal * radius;

            return join;
        }

        void node_to_point()
        {
            if (transform.childCount == 0)
                return;

            base_points.Clear();

            foreach (Transform t in transform) {
                base_points.Add(t.localPosition);
            }
        }

        public void clear_points() {
            base_points.Clear();
        }

        public int point_count {
            get { return base_points.Count; }
        }

        // For use when adding points procedurally (not from the editor).
        public void add_point(Vector3 wsp) {
            base_points.Add(transform.InverseTransformVector(wsp));
        }

        public void generate()
        {
            mesh.Clear();
            lines.Clear();
            points.Clear();
            distances.Clear();
            node_to_point();

            if (base_points.Count < 2)
            {
                Debug.Log("ProceduralLine: invalid line");
                return;
            }

            float half_thickness = 0.5f * config_thickness;
            int base_points_count = base_points.Count;

            if (!config_closed)
                base_points_count--;

            for (int i = 0; i < base_points_count; ++i) {
            // for (int i = 0; i < 2; ++i) {
                lines.Add(get_line(base_points[i], base_points[(i + 1) % base_points.Count]));
            }

            if (lines.Count >= 2)
            {
                int lines_count = lines.Count;

                if (!config_closed)
                    lines_count--;

                // ----------------------------------------------------------------------------------
                // -- Generate points based on the input lines.
                // ----------------------------------------------------------------------------------

                Vector3 first_center_point = Vector3.zero;
                Vector3 first_outer_point = Vector3.zero;
                Vector3 first_inner_point = Vector3.zero;

                Vector3 previous = Vector3.zero;
                int segment_count = 0;

                for (int i = 0; i < lines_count; ++i)
                {
                    Line l0 = lines[i];
                    Line l1 = lines[(i + 1) % lines.Count];

                    // NOTE(gabic): Sa injumatatesc numarul de config_join_segments pentru unghiurile mari.

                    LineJoinCircle join = compute_join(l0, l1, config_circle_radius);
                    float angle_incr = join.angle / config_join_segments;

                    if (!config_closed && i == 0)
                    {
                        points.Add(l0.p0 + l0.normal * half_thickness);
                        points.Add(l0.p0 - l0.normal * half_thickness);
                        segment_count++;

                        distances.Add(0.0f);
                        previous = l0.p0;
                    }

                    Vector3 start_outer_point = join.join_point_0 + l0.normal * half_thickness;
                    Vector3 start_inner_point = join.join_point_0 - l0.normal * half_thickness;

                    Vector3 start_outer_vector = start_outer_point - join.origin;
                    Vector3 start_inner_vector = start_inner_point - join.origin;
                    Vector3 radius_vector = join.join_point_0 - join.origin;

                    // float outer_radius = config_circle_radius + half_thickness;
                    // float inner_radius = config_circle_radius - half_thickness;

                    // points.Add(start_outer_point);
                    // points.Add(start_inner_point);
                    // segment_count++;

                    float segment_angle_incr = join.angle * 1.0f / (config_join_segments);
                    // float segment_angle = segment_angle_incr;
                    float segment_angle = 0;

                    // distances.Add(distances[distances.Count - 1] + (join.join_point_0 - previous).magnitude);
                    // previous = join.join_point_0;

                    float sign = join.cw ? 1.0f : -1.0f;
                    float arclen = segment_angle_incr * config_circle_radius;

                    for (int j = 0; j <= config_join_segments; ++j)
                    {
                        float cos = Mathf.Cos(sign * segment_angle);
                        float sin = Mathf.Sin(sign * segment_angle);

                        Vector3 radius_rotated_vector = new Vector3(
                            cos * radius_vector.x + sin * radius_vector.y,
                            -sin * radius_vector.x + cos * radius_vector.y);

                        Vector3 outer_rotated_vector = new Vector3(
                            cos * start_outer_vector.x + sin * start_outer_vector.y,
                            -sin * start_outer_vector.x + cos * start_outer_vector.y);

                        Vector3 inner_rotated_vector = new Vector3(
                            cos * start_inner_vector.x + sin * start_inner_vector.y,
                            -sin * start_inner_vector.x + cos * start_inner_vector.y);

                        points.Add(join.origin + outer_rotated_vector);
                        points.Add(join.origin + inner_rotated_vector);
                        segment_count++;

                        Vector3 center_point = join.origin + radius_rotated_vector;

                        if (j >= 0)
                        {
                            if (i == 0)
                            {
                                first_center_point = join.join_point_0;
                                first_outer_point = start_outer_point;
                                first_inner_point = start_inner_point;
                            }

                            if (distances.Count == 0)
                                distances.Add(0);
                            else
                                distances.Add(distances[distances.Count - 1] + (center_point - previous).magnitude);
                        }
                        // else
                        //     distances.Add(distances[distances.Count - 1] + arclen);

                        previous = center_point;
                        segment_angle += segment_angle_incr;
                    }

                    if (!config_closed && i == lines_count - 1)
                    {
                        points.Add(l1.p1 + l1.normal * half_thickness);
                        points.Add(l1.p1 - l1.normal * half_thickness);
                        segment_count++;

                        distances.Add(distances[distances.Count - 1] + (l1.p1 - previous).magnitude);
                        previous = l1.p1;
                    }

                    if (config_closed && i == lines_count - 1)
                    {
                        points.Add(first_outer_point);
                        points.Add(first_inner_point);
                        segment_count++;

                        distances.Add(distances[distances.Count - 1] + (first_center_point - previous).magnitude);
                        previous = first_center_point;
                    }
                }

                // ----------------------------------------------------------------------------------
                // -- Generate mesh data.
                // ----------------------------------------------------------------------------------

                if (points.Count > 0)
                {
                    // int count = (segment_count - 1) * 6;
                    // Debug.Log(segment_count);
                    float one_over_total_distance = 1.0f / distances[distances.Count - 1];

                    Vector3[] vertices = new Vector3[points.Count];
                    int[] indices = new int[(segment_count - 1) * 6];
                    Vector2[] uvs = new Vector2[points.Count];

                    float final_tilt_value = config_tilt_multiplier * config_tilt_value;

                    if (config_tilt_reversed)
                        final_tilt_value *= -1;

                    int total_tile_count = (int) (distances[distances.Count - 1] / config_tile_size);
                    float tile_size_multiplier = distances[distances.Count - 1] / (total_tile_count * config_tile_size);
                    float final_tile_size = config_tile_size * tile_size_multiplier;

                    if (config_reversed)
                        final_tile_size *= -1;

                    for (int i = 0; i < points.Count; i += 2)
                    {
                        int segment = i >> 1;

                        vertices[i + 0] = points[i + 0];
                        vertices[i + 1] = points[i + 1];

                        float u = distances[segment] / final_tile_size;

                        uvs[i + 0] = new Vector2(u + final_tilt_value, 1.0f);
                        uvs[i + 1] = new Vector2(u, 0.0f);

                        // uvs[i + 0] = new Vector2(distances[segment] * one_over_total_distance + final_tilt_value, 1.0f);
                        // uvs[i + 1] = new Vector2(distances[segment] * one_over_total_distance, 0.0f);
                    }

                    for (int i = 0, j = 0; i < points.Count - 2; i += 2, j += 6)
                    {
                        indices[j + 0] = i + 0;
                        indices[j + 1] = i + 1;
                        indices[j + 2] = i + 3;
                        indices[j + 3] = i + 0;
                        indices[j + 4] = i + 3;
                        indices[j + 5] = i + 2;
                    }

                    if (enable_debug)
                    {
                        DebugFile debugger = DebugFile.get_instance();
                        debugger.write(vertices);
                        debugger.flush();
                    }

                    mesh.SetVertices(vertices);
                    mesh.SetTriangles(indices, 0);
                    mesh.SetUVs(0, uvs);
                }
            }
        }

        void OnInspectorGUI()
        {
        }
    }
}
