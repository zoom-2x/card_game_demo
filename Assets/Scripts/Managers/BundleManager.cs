using System.Collections;
using System.Collections.Generic;
using System.IO;

using UnityEngine;
using UnityEngine.SceneManagement;

namespace CardGame.Managers
{
    public struct BundleInfo
    {
        public string file;
        public string name;
        public bool loaded;
        public AssetBundle data;
    }

    public enum BundleEnum
    {
        SHADERS = 0,
        MATERIALS = 1,
        PREFABS = 2,
        COMMON_TEXTURES = 3,
        CARD_TEXTURES = 4,
        UI_TEXTURES = 5,
        UI = 6,
        FONTS = 7,
        CARD_TEMPLATES = 8,
        LEVELS = 9
    }

    public class BundleManager 
    {
        public static BundleInfo[] bundles = new BundleInfo[]
        {
            new BundleInfo() {file = "shaders", name = "Shaders"},
            new BundleInfo() {file = "materials", name = "Materials"},
            new BundleInfo() {file = "prefabs", name = "Prefabs"},
            new BundleInfo() {file = "common_textures", name = "Common textures"},
            new BundleInfo() {file = "card_textures", name = "Card textures"},
            new BundleInfo() {file = "ui_textures", name = "UI textures"},
            new BundleInfo() {file = "ui", name = "UI"},
            new BundleInfo() {file = "fonts", name = "Fonts"},
            new BundleInfo() {file = "card_templates", name = "Card templates"},
            new BundleInfo() {file = "levels", name = "Levels"},
        };

        Queue<int> bundles_to_load = new Queue<int>();

        string level_to_load = "";
        int bundles_to_load_count = 0;
        int finished_count = 0;
        int processed_bundle = -1;

        AsyncOperation bundle_request;
        AsyncOperation level_request;

        public bool running = false;
        float _progress = 0;
        public float loading_progress = 0;
        public string loading_name = "";

        bool is_valid_bundle(int bi) {
            return bi >= 0 && bi < bundles.Length;
        }

        public void set_level(string level) {
            level_to_load = level;
        }

        public bool is_loaded(int bundle) {
            return bundles[bundle].loaded;
        }

        public AssetBundle bundle(BundleEnum bundle)
        {
            BundleInfo bi = bundles[(int) bundle];

            if (bi.loaded)
                return bi.data;

            return null;
        }

        public void enqueue(BundleEnum bundle) {
            bundles_to_load.Enqueue((int) bundle);
        }

        public void start()
        {
            if (bundles_to_load.Count == 0)
                return;

            running = true;
            _progress = 0;
            loading_progress = 0;
            finished_count = 0;
            loading_name = "";
            bundles_to_load_count = bundles_to_load.Count;
        }

        public void start_sync()
        {
            if (bundles_to_load.Count == 0)
                return;

            while (bundles_to_load.Count > 0)
            {
                int bi = (int) bundles_to_load.Dequeue();

                if (!is_valid_bundle(bi) || is_loaded(bi))
                    continue;

                BundleInfo bundle = bundles[bi];

                #if UNITY_EDITOR
                string bundle_filepath = Path.Combine("Assets/assetbundles", bundle.file);
                #else
                string bundle_filepath = Path.Combine(Application.dataPath, "assetbundles", bundle.file);
                #endif

                bundle.loaded = true;
                bundle.data = AssetBundle.LoadFromFile(bundle_filepath);
                bundles[bi] = bundle;
            }
        }

        void _make_bundle_request(int bi)
        {
            if (!is_valid_bundle(bi) || is_loaded(bi))
                return;

            BundleInfo bundle = bundles[bi];
            processed_bundle = bi;

            #if UNITY_EDITOR
            string bundle_filepath = Path.Combine("Assets/assetbundles", bundle.file);
            #else
            string bundle_filepath = Path.Combine(Application.dataPath, "assetbundles", bundle.file);
            #endif

            // TODO(gabic): Verificarea fisierului.

            loading_name = bundle.file;
            bundle_request = AssetBundle.LoadFromFileAsync(bundle_filepath);
        }

        public void unload(string bundle)
        {}

        public void unload_all()
        {}

        public void update()
        {
            if (!running)
                return;

            int total = bundles_to_load_count;

            // Load the next bundle.
            if (finished_count < bundles_to_load_count && bundle_request == null) {
                _make_bundle_request(bundles_to_load.Dequeue());
            }

            else if (bundle_request != null)
            {
                // Bundle has finished loading.
                if (bundle_request.isDone)
                {
                    // Update the loaded bundle info.
                    BundleInfo bundle = bundles[processed_bundle];
                    bundle.loaded = true;
                    bundle.data = ((AssetBundleCreateRequest) bundle_request).assetBundle;
                    bundles[processed_bundle] = bundle;

                    finished_count++;
                    _progress = finished_count;
                    bundle_request = null;
                }
                else
                    _progress = finished_count + bundle_request.progress;

                // All the bundles are loaded.
                if (finished_count == bundles_to_load_count)
                {
                    if (!string.IsNullOrEmpty(level_to_load))
                        level_request = SceneManager.LoadSceneAsync(level_to_load);
                }
            }

            // Level load (sa scot partea asta de aici ?).
            else if (level_request != null)
            {
                if (level_request.isDone)
                {
                    finished_count++;
                    _progress = finished_count;
                    level_request = null;
                }
                else
                    _progress = finished_count + level_request.progress;
            }

            if (_progress < finished_count)
            {
                Debug.Log($"progress: {_progress} / finished_count: {finished_count} / total: {total}");
            }

            loading_progress = Mathf.Clamp01(_progress / total);

            // Everything was loaded.
            if (loading_progress == 1.0f && bundle_request == null && level_request == null)
            {
                running = false;
                processed_bundle = -1;
            }
        }
    }
}
