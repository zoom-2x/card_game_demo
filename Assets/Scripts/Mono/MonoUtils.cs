using UnityEngine;
using CardGame.Mono;

namespace CardGame.Mono
{
    public class MonoUtils
    {
        // To be called in the scene entry behaviour.
        public static void cache_init()
        {
            GameObject cache = GameObject.Find("CACHE");

            if (cache == null)
            {
                Debug.Log("Creating the cache...");

                cache = new GameObject();
                cache.transform.position = Vector3.zero;
                cache.name = "CACHE";
            }

            GameSystems.cache = cache;
        }

        public static GameObject get_map()
        {
            GameObject map_obj = GameObject.Find("MAP");

            // If the "MAP" does not exist then create it.
            if (map_obj == null)
            {
                Debug.Log("Creating the map...");

                map_obj = new GameObject();
                map_obj.transform.position = Vector3.zero;
                map_obj.name = "MAP";
            }

            return map_obj;
        }

        public static LevelFade get_level_fade()
        {
            LevelFade fade = null;
            GameObject fade_obj = GameObject.Find("level_fade");

            if (fade_obj != null)
                fade = fade_obj.GetComponent<LevelFade>();

            return fade;
        }
    }
}
