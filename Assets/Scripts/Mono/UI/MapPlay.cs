using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

using CardGame.IO;
using CardGame.Hexgrid;
using CardGame.Data;

using CardGame.CGDebug;

namespace CardGame.Mono.UI
{
    public class MapPlay : MonoBehaviour
    {
        LevelFade fade;

        HexMap map = null;

        void Start()
        {
            QualitySettings.antiAliasing = 0;
            UniversalRenderPipelineAsset data = GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;

            SceneManager.LoadScene("room", LoadSceneMode.Additive);
            MonoUtils.cache_init();

            fade = MonoUtils.get_level_fade();

            if (fade != null)
            {
                fade.fade_in_finished_event += fade_in_callback;
                fade.fade_out_finished_event += fade_out_callback;
                fade.set_state(LevelFade.STATE_FADE_IN);
            }
        }

        void fade_in_callback() {}
        void fade_out_callback() {}

        void Update()
        {
            if (fade != null)
                fade.begin();

            if (map != null) 
                map.update();

            if (Input.GetMouseButtonDown(0))
            {
                if (GameSystems.game.map.over_tile != null)
                {}
            }
        }
    }
}
