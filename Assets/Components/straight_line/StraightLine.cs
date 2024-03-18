using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using CardGame;
using CardGame.IO;

namespace gc_components
{
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    public class StraightLine : MonoBehaviour
    {
        protected struct Line
        {
            public Vector3 p0;
            public Vector3 p1;
            public Vector3 direction;
            public Vector3 normal;
            public float offset;
        }

        [System.NonSerialized] public bool closed = false;
        [System.NonSerialized] public float width = 1.0f;
        [System.NonSerialized] public bool cw = false;
        [System.NonSerialized] public bool inner_border = false;
        [System.NonSerialized] public float inner_border_offset = 0.01f;

        bool DEBUG_MODE = false;

        Mesh mesh;
        List<Vector3> vertices = new List<Vector3>();
        List<int> indices = new List<int>();

        List<Line> lines = new List<Line>();
        List<Vector3> points = new List<Vector3>();

        void OnEnable()
        {
            mesh = new Mesh();
            mesh.name = "Procedural line (straight)";
            GetComponent<MeshFilter>().sharedMesh = mesh;
            // generate();
        }

        void OnDisable()
        {
            Debug.Log("StraightLine: disabled");
            clear_points();

            #if UNITY_EDITOR
                Object.DestroyImmediate(mesh);
            #else
                Object.Destroy(mesh);
            #endif
        }

        public void clear_points() {
            points.Clear();
        }

        public void add_point(Vector3 point) {
            points.Add(point);
        }

        public int point_count {
            get { return points.Count; }
        }

        Line get_line(Vector3 p0, Vector3 p1)
        {
            Line line = new Line();

            line.p0 = p0;
            line.p1 = p1;
            line.direction = (p1 - p0).normalized;
            line.normal = new Vector3(line.direction.normalized.z, 0, -line.direction.normalized.x);
            line.offset = 0;

            return line;
        }

        float join_offset(int line_index, bool front = true)
        {
            int si = (line_index + 1) % lines.Count;

            if (!front)
                si = (line_index + (lines.Count - 1)) % lines.Count;

            Line first_line = lines[line_index];
            Line second_line = lines[si];

            float angle = Mathf.Acos(Vector3.Dot(-first_line.direction, second_line.direction));
            float half_angle = 0.5f * angle;

            if (line_index == lines.Count - 1)
            {
                // Debug.Log(width / Mathf.Tan(half_angle));
                // Debug.Log(half_angle * Mathf.Rad2Deg);
            }

            return width / Mathf.Tan(half_angle);
        }

        int rotate_front(int index, int count) {
            return (index + 1) % count;
        }

        int rotate_back(int index, int count) {
            return (index + (count - 1)) % count;
        }

        int pscale = 256;
        float one_over_pscale = 1.0f / 256;

        float fp2float(int fp) {
            return fp * one_over_pscale;
        }

        int float2fp(float f) {
            return Mathf.RoundToInt(f * pscale); 
        }

        Vector3 snap_point(Vector3 p)
        {
            p.x = fp2float(float2fp(p.x));
            p.y = fp2float(float2fp(p.y));
            p.z = fp2float(float2fp(p.z));

            return p;
        }

        bool check_vector_turn(Vector3 v0, Vector3 v1)
        {
            v0.y = 0;
            v1.y = 0;

            return Vector3.SignedAngle(v0, v1, Vector3.up) >= 0;
        }

        void generate_full(int i)
        {
            int front_index = rotate_front(i, lines.Count);
            int back_index = rotate_back(i, lines.Count);

            Line base_line = lines[i];
            Line front_line = lines[front_index];
            Line back_line = lines[back_index];

            float front_offset = join_offset(i);
            float back_offset = 0;

            if (i == 0 && closed || i > 0)
                back_offset = join_offset(i, false);

            Vector3 p0p = base_line.p0 + base_line.direction * back_offset;
            Vector3 p1p = base_line.p1 - base_line.direction * front_offset;

            int index_base = vertices.Count;

            vertices.Add(snap_point(p0p - base_line.normal * width));
            vertices.Add(snap_point(p0p + base_line.normal * width));

            vertices.Add(snap_point(p1p - base_line.normal * width));
            vertices.Add(snap_point(p1p + base_line.normal * width));

            indices.Add(index_base + 0);
            indices.Add(index_base + 3);
            indices.Add(index_base + 1);

            indices.Add(index_base + 0);
            indices.Add(index_base + 2);
            indices.Add(index_base + 3);

            if (closed || (!closed && i < lines.Count - 1))
            {
                bool turn_right = check_vector_turn(base_line.direction, front_line.direction);

                if (turn_right)
                {
                    vertices.Add(snap_point(base_line.p1 +
                                            base_line.direction * front_offset -
                                            base_line.normal * width));

                    vertices.Add(snap_point(front_line.p0 +
                                            front_line.direction * front_offset -
                                            front_line.normal * width));

                    indices.Add(index_base + 3);
                    indices.Add(index_base + 2);
                    indices.Add(index_base + 4);

                    indices.Add(index_base + 3);
                    indices.Add(index_base + 4);
                    indices.Add(index_base + 5);
                }

                else
                {
                    vertices.Add(snap_point(base_line.p1 +
                                            base_line.direction * front_offset +
                                            base_line.normal * width));

                    vertices.Add(snap_point(front_line.p0 +
                                            front_line.direction * front_offset +
                                            front_line.normal * width));

                    if (i == lines.Count - 1)
                    {
                        Debug.Log(base_line.p1 + base_line.direction * front_offset + base_line.normal * width);
                        Debug.Log(base_line.p0 + front_line.direction * front_offset + front_line.normal * width);
                    }

                    indices.Add(index_base + 2);
                    indices.Add(index_base + 5);
                    indices.Add(index_base + 4);

                    indices.Add(index_base + 2);
                    indices.Add(index_base + 4);
                    indices.Add(index_base + 3);
                }
            }
        }

        void generate_inner_cw(int i)
        {
            int front_index = rotate_front(i, lines.Count);
            int back_index = rotate_back(i, lines.Count);

            Line base_line = lines[i];
            Line front_line = lines[front_index];
            Line back_line = lines[back_index];

            bool front_turn_right = check_vector_turn(base_line.direction, front_line.direction);
            bool back_turn_right = check_vector_turn(back_line.direction, base_line.direction);

            float front_offset = join_offset(i);
            float back_offset = 0;
            float front_offset_backup = front_offset;

            if (((i == 0 && closed) || i > 0) && back_turn_right)
                back_offset = join_offset(i, false);

            if ((i == lines.Count && !closed) || !front_turn_right)
                front_offset = 0;
            
            Vector3 base_p0 = base_line.p0 + (base_line.normal + back_line.normal).normalized * inner_border_offset;
            Vector3 base_p1 = base_line.p1 + (base_line.normal + front_line.normal).normalized * inner_border_offset;

            Vector3 p0p = base_p0 + base_line.direction * back_offset;
            Vector3 p1p = base_p1 - base_line.direction * front_offset;

            int index_base = vertices.Count;
            
            // Segment.
            vertices.Add(snap_point(p0p));
            vertices.Add(snap_point(p0p + base_line.normal * width));

            vertices.Add(snap_point(p1p));
            vertices.Add(snap_point(p1p + base_line.normal * width));

            indices.Add(index_base + 0);
            indices.Add(index_base + 3);
            indices.Add(index_base + 1);

            indices.Add(index_base + 0);
            indices.Add(index_base + 2);
            indices.Add(index_base + 3);

            // Join.
            if (closed || (!closed && i < lines.Count - 1))
            {
                if (front_turn_right)
                {
                    vertices.Add(snap_point(base_p1));
                    vertices.Add(snap_point(base_p1 + front_line.direction * front_offset));

                    indices.Add(index_base + 3);
                    indices.Add(index_base + 2);
                    indices.Add(index_base + 4);

                    indices.Add(index_base + 3);
                    indices.Add(index_base + 4);
                    indices.Add(index_base + 5);
                }
                else
                {
                    vertices.Add(snap_point(base_p1 + base_line.normal * width + base_line.direction * front_offset_backup));
                    vertices.Add(snap_point(base_p1 + front_line.normal * width));

                    indices.Add(index_base + 2);
                    indices.Add(index_base + 4);
                    indices.Add(index_base + 3);

                    indices.Add(index_base + 2);
                    indices.Add(index_base + 5);
                    indices.Add(index_base + 4);
                }
            }
        }

        void generate_inner_ccw(int i)
        {
            int front_index = rotate_front(i, lines.Count);
            int back_index = rotate_back(i, lines.Count);

            Line base_line = lines[i];
            Line front_line = lines[front_index];
            Line back_line = lines[back_index];

            bool front_turn_right = check_vector_turn(base_line.direction, front_line.direction);
            bool back_turn_right = check_vector_turn(back_line.direction, base_line.direction);

            float front_offset = join_offset(i);
            float back_offset = 0;
            float front_offset_backup = front_offset;

            if (((i == 0 && closed) || i > 0) && !back_turn_right)
                back_offset = join_offset(i, false);

            if ((i == lines.Count && !closed) || front_turn_right)
                front_offset = 0;

            Vector3 base_p0 = base_line.p0 - (base_line.normal + back_line.normal).normalized * inner_border_offset;
            Vector3 base_p1 = base_line.p1 - (base_line.normal + front_line.normal).normalized * inner_border_offset;
            
            Vector3 p0p = base_p0 + base_line.direction * back_offset;
            Vector3 p1p = base_p1 - base_line.direction * front_offset;

            int index_base = vertices.Count;
            
            // Segment.
            vertices.Add(snap_point(p0p));
            vertices.Add(snap_point(p0p - base_line.normal * width));

            vertices.Add(snap_point(p1p));
            vertices.Add(snap_point(p1p - base_line.normal * width));

            indices.Add(index_base + 0);
            indices.Add(index_base + 1);
            indices.Add(index_base + 3);

            indices.Add(index_base + 0);
            indices.Add(index_base + 3);
            indices.Add(index_base + 2);

            // Join.
            if (closed || (!closed && i < lines.Count - 1))
            {
                if (!front_turn_right)
                {
                    vertices.Add(snap_point(base_p1));
                    vertices.Add(snap_point(base_p1 + front_line.direction * front_offset));

                    indices.Add(index_base + 3);
                    indices.Add(index_base + 4);
                    indices.Add(index_base + 2);

                    indices.Add(index_base + 3);
                    indices.Add(index_base + 5);
                    indices.Add(index_base + 4);
                }
                else
                {
                    vertices.Add(snap_point(base_p1 - base_line.normal * width + base_line.direction * front_offset_backup));
                    vertices.Add(snap_point(base_p1 - front_line.normal * width));

                    indices.Add(index_base + 2);
                    indices.Add(index_base + 3);
                    indices.Add(index_base + 4);

                    indices.Add(index_base + 2);
                    indices.Add(index_base + 4);
                    indices.Add(index_base + 5);
                }
            }
        }

        public void generate()
        {
            lines.Clear();
            int count = closed ? points.Count : points.Count - 1;

            for (int i = 0; i < count; ++i) {
                lines.Add(get_line(points[i], points[(i + 1) % points.Count]));
            }

            // For now the vertices are not unique, duplicates can occur.
            Vector3[] segment_points = new Vector3[6];

            // Generate segments.
            for (int i = 0; i < lines.Count; ++i)
            {
                if (inner_border && cw)
                    generate_inner_cw(i);
                else if (inner_border && !cw)
                    generate_inner_ccw(i);
                else
                    generate_full(i);    
            }

            mesh.Clear();
            mesh.SetVertices(vertices);
            mesh.SetTriangles(indices, 0);

            if (DEBUG_MODE)
                debug_vertices();
        }

        void debug_vertices()
        {
            float scale = 0.1f;

            for (int i = 0; i < vertices.Count; ++i)
            {
                Transform point = GameSystems.asset_manager.aquire_debug_point();
                point.position = vertices[i];
                point.localScale = new Vector3(scale, scale, scale);
            }
        }
    }
}
