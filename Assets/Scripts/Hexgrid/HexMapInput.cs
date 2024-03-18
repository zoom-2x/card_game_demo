using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using CardGame.Mono;

namespace CardGame.Hexgrid
{
    public class HexMapInput
    {
        // public static HexTile over_tile = null;
        // public static HexMap map = null;
        // public static Camera camera = null;
        // public static Vector2Int real_coordinates = new Vector2Int(99999, 99999);
        // public static bool disable = false;

        // static HexTile prev_over = null;

        // public static void update()
        // {
        //     if (map == null || camera == null)
        //         return;

        //     float dist = 0;
        //     Ray ray = camera.ScreenPointToRay(Input.mousePosition);
        //     map.plane.Raycast(ray, out dist);
        //     Vector3 intersection = ray.GetPoint(dist);

        //     real_coordinates = map.world_point_to_real_offset(intersection);
        //     over_tile = map.get_tile_from_real_offset(real_coordinates);

        //     if (over_tile != null)
        //     {
        //         if (prev_over != null)
        //         {
        //             prev_over.base_state();
        //             prev_over.tile_state();
        //         }

        //         prev_over = over_tile;
        //         prev_over.base_state(true);
        //         prev_over.tile_state(true);
        //     }

        //     else
        //     {
        //         if (prev_over != null)
        //         {
        //             prev_over.base_state();
        //             prev_over.tile_state();
        //         }

        //         prev_over = null;
        //     }
        // }
    }
}
