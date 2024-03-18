using System.IO;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.SceneManagement;

using CardGame.Managers;

namespace CardGame.Mono
{
    public class Entry : MonoBehaviour
    {
        AssetBundle shaders;
        AssetBundle materials;
        AssetBundle african_head;
        AssetBundle test_level;

        void Start()
        {
            QualitySettings.antiAliasing = 4;

            // Bundles to load.
            GameSystems.bundle_manager.enqueue(BundleEnum.SHADERS);
            GameSystems.bundle_manager.enqueue(BundleEnum.MATERIALS);
            GameSystems.bundle_manager.enqueue(BundleEnum.PREFABS);
            GameSystems.bundle_manager.enqueue(BundleEnum.COMMON_TEXTURES);
            GameSystems.bundle_manager.enqueue(BundleEnum.CARD_TEXTURES);
            GameSystems.bundle_manager.enqueue(BundleEnum.UI_TEXTURES);
            GameSystems.bundle_manager.enqueue(BundleEnum.UI);
            GameSystems.bundle_manager.enqueue(BundleEnum.FONTS);
            GameSystems.bundle_manager.enqueue(BundleEnum.CARD_TEMPLATES);
            GameSystems.bundle_manager.enqueue(BundleEnum.LEVELS);
            GameSystems.bundle_manager.set_level("main_menu");

            SceneManager.LoadScene("loader");
        }

        void Update()
        {}
    }
}
