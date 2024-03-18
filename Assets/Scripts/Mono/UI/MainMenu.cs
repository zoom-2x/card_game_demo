using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;
// using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

using CardGame.CGDebug;

namespace CardGame.Mono.UI
{
    public class MainMenu : MonoBehaviour
    {
        public bool debug_load = false;

        string[] levels = new string[]
        {
            "card_play",
            "map_play",
            "map_editor"
        };
        
        LevelFade fade;

        bool input_enabled = false;
        bool loading_request = false;
        string selected_level = "";

        void Start()
        {
            if (debug_load)
                DebugScenes.load_bundles_sync();

            SceneManager.LoadScene("room", LoadSceneMode.Additive);

            fade = MonoUtils.get_level_fade();

            if (fade != null)
            {
                fade.fade_in_finished_event += fade_in_callback;
                fade.fade_out_finished_event += fade_out_callback;
                fade.set_state(LevelFade.STATE_FADE_IN);
            }
        }

        void fade_in_callback() {
            input_enabled = true;
        }

        void fade_out_callback() {
            SceneManager.LoadScene(selected_level); // Async ?
        }

        public void click_callback(int i)
        {
            if (i == 3)
            {
                Application.Quit();
                return;
            }

            if (fade == null || fade.running || 
                loading_request || !input_enabled || i < 0 || i >= levels.Length)
                return;

            loading_request = true;
            selected_level = levels[i];

            if (fade != null)
            {
                fade.set_state(LevelFade.STATE_FADE_OUT);
                fade.begin();
            }
        }

        void Update()
        {
            if (fade != null)
                fade.begin();
        }
    }
}
