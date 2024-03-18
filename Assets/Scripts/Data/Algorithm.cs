using System.Text;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using CardGame.Hexgrid;
using CardGame.IO;

namespace CardGame.Data
{
    public class Polygon
    {
        public List<Vector3> points = new List<Vector3>();
        public List<Vector2> uvs = new List<Vector2>();
        public List<int> indices = new List<int>();

        public Vector2 min = new Vector2(float.MaxValue, float.MaxValue);
        public Vector2 max = new Vector2(float.MinValue, float.MinValue);

        public Vector3 origin = Vector3.zero;
    }

    public class Algorithm
    {
        public static Vector2Int closest_polygon_point(Polygon b, Polygon p)
        {
            float distance = float.MaxValue;
            Vector2Int res = Vector2Int.zero;

            // for (int i = 0; i < b.base_count; ++i)
            for (int i = 0; i < b.points.Count; ++i)
            {
                Vector3 p0 = b.points[i];

                for (int j = 0; j < p.points.Count; ++j)
                {
                    Vector3 p1 = p.points[j];
                    Vector3 v = p1 - p0;

                    if (v.magnitude < distance)
                    {
                        res.x = i;
                        res.y = j;

                        distance = v.magnitude;
                    }
                }
            }

            Debug.Log($"distance: {distance} / {b.points[res.x]} / {p.points[res.y]}");

            return res;
        }

        static void check_min(ref Vector2 min, Vector3 p)
        {
            if (p.x < min.x)
                min.x = p.x;

            if (p.z < min.y)
                min.y = p.z;
        }

        static void check_max(ref Vector2 max, Vector3 p)
        {
            if (p.x > max.x)
                max.x = p.x;

            if (p.z > max.y)
                max.y = p.z;
        }

        public static Polygon create_polygon(List<HexTileEdge> chain)
        {
            Polygon p = new Polygon();
            int index = 0;

            for (int i = 0; i < chain.Count; ++i)
            {
                HexTileEdge edge = chain[i];

                if (i == 0)
                {
                    p.points.Add(edge.p0);
                    p.points.Add(edge.p1);

                    p.indices.Add(index++);
                    p.indices.Add(index++);
                }

                else if (i < chain.Count - 1)
                {
                    p.points.Add(edge.p1);
                    p.indices.Add(index++);

                    check_min(ref p.min, edge.p1);
                    check_max(ref p.max, edge.p1);
                }
            }

            return p;
        }

        // Points are considered to be on the XoZ plane (y = 0).
        public static bool is_convex(Vector3 p0, Vector3 p1, Vector3 p2)
        {
            bool res = false;
            Vector3 up = Vector3.up;

            // Left hand rule.
            Vector3 cross = Vector3.Cross(p2 - p1, p0 - p1);
            float dot = Vector3.Dot(cross, up);
            res = dot >= 0;

            // Debug.Log($"cross: {cross} / dot: {dot}");

            return res;
        }

        public static bool is_coliniear(Vector3 p0, Vector3 p1, Vector3 p2)
        {
            float angle = Vector3.Angle(p2 - p1, p0 - p1);
            return angle > 179 && angle <= 180;
        }

        // Points are considered to be on the XoZ plane (y = 0).
        // For clockwise points.
        public static bool is_inside_triangle(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 point, bool debug = false)
        {
            bool res = false;

            float a1 = p2.z - p3.z;
            float b1 = p3.x - p2.x;
            float c1 = p2.x * p3.z - p2.z * p3.x;

            float a2 = p3.z - p1.z;
            float b2 = p1.x - p3.x;
            float c2 = p3.x * p1.z - p3.z * p1.x;

            float a3 = p1.z - p2.z;
            float b3 = p2.x - p1.x;
            float c3 = p1.x * p2.z - p1.z * p2.x;

            float l1 = -1 * (a1 * point.x + b1 * point.z + c1);
            float l2 = -1 * (a2 * point.x + b2 * point.z + c2);
            float l3 = -1 * (a3 * point.x + b3 * point.z + c3);

            res = (l1 >= 0) && (l2 >= 0) && (l3 >= 0);

            return res;
        }

        public static void offset(Polygon p)
        {
            p.origin = new Vector3(
                    p.min.x + 0.5f * (p.max.x - p.min.x),
                    0,
                    p.min.y + 0.5f * (p.max.y - p.min.y));

            for (int i = 0; i < p.points.Count; ++i)
            {
                Vector3 point = p.points[i];

                point.x -= p.origin.x;
                point.z -= p.origin.z;

                p.points[i] = point;
            }
        }

        public static void triangulate_ear_clipping(Polygon p)
        {
            List<int> triangles = new List<int>();
            int current = 1;
            int cycles = 0;

            DebugFile debugger = DebugFile.get_instance();
            StringBuilder b = new StringBuilder();

            for (int i = 0; i < p.indices.Count; ++i) 
            {
                b.Append($"{p.indices[i]}");

                if (i < p.indices.Count - 1)
                    b.Append(" ");
            }

            debugger.write(b.ToString());

            while (p.indices.Count > 3)
            {
                int idx0 = (p.indices.Count + current - 1) % p.indices.Count;
                int idx1 = (p.indices.Count + current + 0) % p.indices.Count;
                int idx2 = (p.indices.Count + current + 1) % p.indices.Count;

                int index0 = p.indices[idx0];
                int index1 = p.indices[idx1];
                int index2 = p.indices[idx2];

                Vector3 p0 = p.points[index0];
                Vector3 p1 = p.points[index1];
                Vector3 p2 = p.points[index2];

                bool is_coliniear = Algorithm.is_coliniear(p0, p1, p2);
                bool is_convex = Algorithm.is_convex(p0, p1, p2);

                bool removed = false;
                debugger.write($"current: {current} / {idx0} / {idx1} / {idx2}");
                debugger.write($"set: {index0} / {index1} / {index2} / convex: {is_convex}");
                // Debug.Log($"convex: {is_convex} / {p0} / {p1} / {p2}");

                if (!is_coliniear && is_convex)
                {
                    bool overlaps = false;

                    // Check if current triangle overlaps any point.
                    for (int i = 0; i < p.points.Count; ++i)
                    {
                        Vector3 point = p.points[i];
                        bool debug = false;

                        if (i == index0 || i == index1 || i == index2)
                            continue;

                        if (index0 == 1 && index1 == 2 && index2 == 3)
                            debug = true;

                        bool check = Algorithm.is_inside_triangle(p0, p1, p2, point, debug);

                        if (index0 == 1 && index1 == 2 && index2 == 3)
                        {
                            if (check)
                                debugger.write($"overlap: {i} / {point}");
                        }
                        
                        if (check)
                        {
                            overlaps = true;
                            break;
                        }
                    }

                    // Add the triangle indices.
                    if (!overlaps)
                    {
                        debugger.write($"removing: {index1}");
                        removed = true;

                        if (idx1 == 0 || idx1 == p.indices.Count - 1)
                            current = 0;

                        p.indices.RemoveAt(idx1);

                        triangles.Add(index0);
                        triangles.Add(index1);
                        triangles.Add(index2);
                    }
                }

                if (cycles > 5000)
                {
                    Debug.Log($"infinte: {p.indices.Count}");
                    break;
                }

                if (!removed)
                    current++;

                cycles++;
            }

            // Add the last triangle.
            if (p.indices.Count == 3)
            {
                triangles.Add(p.indices[0]);
                triangles.Add(p.indices[1]);
                triangles.Add(p.indices[2]);
            }

            p.indices = triangles;

            debugger.flush();
        }

        public static void generate_uvs(Polygon p)
        {
            if (p == null)
                return;

            p.uvs.Clear();

            float oodx = 1.0f / (p.max.x - p.min.x);
            float oody = 1.0f / (p.max.y - p.min.y);

            for (int i = 0; i < p.points.Count; ++i)
            {
                Vector3 vertex = p.points[i];

                // Vertices are of type Vector3 but the polygon is on the plane xoz.
                float u = (vertex.x - p.min.x) * oodx;
                float v = (vertex.z - p.min.y) * oody;

                p.uvs.Add(new Vector2(u, v));
            }
        }
    }
}
