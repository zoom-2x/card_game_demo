using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UIElements;

using gc_components;
using CardGame.Data;

namespace CardGame.Managers
{
    public class AssetManager
    {
        Dictionary<string, Texture> texture_cache = new Dictionary<string, Texture>();
        Dictionary<string, GameObject> prefab_cache = new Dictionary<string, GameObject>();
        Dictionary<string, Material> material_cache = new Dictionary<string, Material>();
        // Dictionary<string, CardTemplate> card_templates_cache = new Dictionary<string, CardTemplate>();

        Dictionary<string, Dictionary<string, Object>> asset_cache = new Dictionary<string, Dictionary<string, Object>>();
        Dictionary<string, Queue<GameObject>> prefab_pool = new Dictionary<string, Queue<GameObject>>();

        BundleManager manager;

        public AssetManager(BundleManager manager)
        {
            this.manager = manager;
        }

        public Object load_asset(BundleEnum bundle, string name)
        {
            Object asset = null;
            BundleInfo bi = BundleManager.bundles[(int) bundle];

            if (!asset_cache.ContainsKey(bi.name))
                asset_cache[bi.name] = new Dictionary<string, Object>();

            Dictionary<string, Object> selected_cache = asset_cache[bi.name];
            AssetBundle loaded_bundle = manager.bundle(bundle);

            if (loaded_bundle == null)
                Debug.LogWarning($"AssetManager: Bundle \"{bundle}\" was not loaded !");
            else
            {
                if (selected_cache.ContainsKey(name))
                    asset = selected_cache[name];
                else
                {
                    asset = loaded_bundle.LoadAsset(name);

                    if (asset != null)
                        selected_cache[name] = asset;
                    else
                        Debug.LogWarning($"AssetManager: Asset {bundle}:{name} not found !");
                }
            }

            return asset;
        }

        public Material aquire_material(BundleEnum bundle, string name, bool copy = false)
        {
            Material mat = null;
            Material asset = load_asset(bundle, name) as Material;

            if (asset != null)
                mat = copy ? Material.Instantiate(asset) : asset;

            return mat;
        }

        public VisualTreeAsset aquire_ut_tree_asset(BundleEnum bundle, string name)
        {
            VisualTreeAsset asset = load_asset(bundle, name) as VisualTreeAsset;

            return asset;
        }

        GameObject _aquire_prefab(BundleEnum bundle, string name, bool use_pool = true)
        {
            GameObject obj = null;
            GameObject prefab = load_asset(bundle, name) as GameObject;

            if (prefab != null)
            {
                if (use_pool)
                {
                    if (!prefab_pool.ContainsKey(name))
                        prefab_pool[name] = new Queue<GameObject>();

                    Queue<GameObject> selected_queue = prefab_pool[name];

                    if (selected_queue.Count == 0)
                        selected_queue.Enqueue(Object.Instantiate(prefab));

                    obj = selected_queue.Dequeue();
                }
                else
                    obj = Object.Instantiate(prefab);
            }

            return obj;
        }

        public GameObject aquire_prefab(BundleEnum bundle, string name, bool use_pool = true)
        {
            GameObject obj = _aquire_prefab(bundle, name, use_pool);

            // Reset the object.
            if (obj != null)
            {
                // obj.SetActive(false);
                obj.transform.parent = GameSystems.cache.transform;
                obj.transform.localPosition = Vector3.zero;
            }

            return obj;
        }

        public GameObject aquire_prefab_ui(BundleEnum bundle, string name, bool use_pool = true) {
            return _aquire_prefab(bundle, name, use_pool);
        }

        public void return_prefab(string name, GameObject prefab, bool back_to_cache = true)
        {
            if (prefab != null)
            {
                if (prefab_pool.ContainsKey(name))
                {
                    Queue<GameObject> selected_queue = prefab_pool[name];

                    prefab.SetActive(false);

                    if (back_to_cache)
                        prefab.transform.parent = GameSystems.cache.transform;

                    selected_queue.Enqueue(prefab);
                }
            }
        }

        // -----------------------------------------------------------
        // -- Utility functions.
        // -----------------------------------------------------------

        public Transform aquire_placeholder(PlayerID id)
        {
            GameObject res = aquire_prefab(BundleEnum.PREFABS, "placeholder");
    
            if (res != null)
            {
                res.layer = Constants.LAYER_PLAYER[(int) id];
                res.SetActive(true);
                res.GetComponent<Placeholder>().set_invalid();
                return res.transform;
            }

            return null;
        }

        public void return_placeholder(GameObject obj)
        {
            obj.SetActive(false);
            return_prefab("placeholder", obj);
        }

        // ----------------------------------------------------------------------------------
        // -- Map objects.
        // ----------------------------------------------------------------------------------

        public Transform aquire_hex_base()
        {
            GameObject obj = aquire_prefab(BundleEnum.PREFABS, "hex_base");

            if (obj != null)
                return obj.transform;
            else
                Debug.LogWarning("HexTile: Missing prefab \"hex_base\"");

            return null;
        }

        public Transform aquire_hex_tile()
        {
            GameObject obj = aquire_prefab(BundleEnum.PREFABS, "hex_tile");

            if (obj != null)
                return obj.transform;
            else
                Debug.LogWarning("HexTile: Missing prefab \"hex_tile\"");

            return null;
        }

        public ProceduralLine aquire_hex_tile_link()
        {
            GameObject obj = aquire_prefab(BundleEnum.PREFABS, "tile_link");

            if (obj != null)
                return obj.GetComponent<ProceduralLine>();
            else
                Debug.LogWarning("HexTile: Missing prefab \"tile_link\"");

            return null;
        }

        public StraightLine aquire_hex_region_border()
        {
            GameObject obj = aquire_prefab(BundleEnum.PREFABS, "region_border");

            if (obj != null)
                return obj.GetComponent<StraightLine>();
            else
                Debug.LogWarning("HexTile: Missing prefab \"region_border\"");

            return null;
        }

        int order = 0;

        public ProceduralLine aquire_hex_region_border_v2()
        {
            GameObject obj = aquire_prefab(BundleEnum.PREFABS, "region_border_curved");
            Renderer r = obj.GetComponent<Renderer>();
            r.material.renderQueue += 1 * (++order);

            if (obj != null)
                return obj.GetComponent<ProceduralLine>();
            else
                Debug.LogWarning("HexTile: Missing prefab \"region_border_curved\"");

            return null;
        }

        public void return_hex_base(GameObject obj) {
            return_prefab("hex_base", obj);
        }

        public void return_hex_tile(GameObject obj) {
            return_prefab("hex_tile", obj);
        }

        public void return_hex_tile_link(GameObject obj) {
            return_prefab("tile_link", obj);
        }

        public void return_hex_region_border(GameObject obj) {
            return_prefab("region_border", obj);
        }

        public void return_hex_region_border_v2(GameObject obj) {
            return_prefab("region_border_curved", obj);
        }

        // -----------------------------------------------------------
        // -- UI prefabs.
        // -----------------------------------------------------------
        
        public Transform aquire_popup_item_plus()
        {
            GameObject obj = aquire_prefab_ui(BundleEnum.PREFABS, "popup_item_plus");
            
            if (obj != null)
                return  obj.transform;

            return null;
        }

        public Transform aquire_popup_item_minus()
        {
            GameObject obj = aquire_prefab_ui(BundleEnum.PREFABS, "popup_item_minus");
            
            if (obj != null)
                return  obj.transform;

            return null;
        }

        public void return_popup_item(GameObject obj)
        {
            if (obj == null)
                return;

            PopupItem item = obj.GetComponent<PopupItem>();

            if (item.type == 0)
                return_prefab("popup_item_plus", obj, false);
            else if (item.type == 1)
                return_prefab("popup_item_minus", obj, false);
        }

        public Transform aquire_region_label()
        {
            GameObject obj = aquire_prefab_ui(BundleEnum.PREFABS, "region_label_gui");
            
            if (obj != null)
                return  obj.transform;

            return null;
        }

        public void return_region_label(GameObject obj)
        {
            if (obj == null)
                return;

            return_prefab("region_label_gui", obj, false);
        }

        public Transform aquire_region_mesh()
        {
            GameObject obj = aquire_prefab_ui(BundleEnum.PREFABS, "region_mesh");
            
            if (obj != null)
                return  obj.transform;

            return null;
        }

        public void return_region_mesh(GameObject obj)
        {
            if (obj == null)
                return;

            return_prefab("region_mesh", obj, false);
        }

        // -----------------------------------------------------------
        // -- Debug prefabs.
        // -----------------------------------------------------------

        public Transform aquire_debug_point()
        {
            GameObject obj = aquire_prefab(BundleEnum.PREFABS, "debug_point");

            if (obj != null)
                return obj.transform;

            return null;
        }

        public void return_debug_point(GameObject obj) {
            return_prefab("debug_point", obj);
        }
    }
}
