using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

namespace CardGame.Mono.UI
{
    public class MainMenuUT : MonoBehaviour
    {
        string[] button_names = new string[]
        {
            "card-play",
            "map-play",
            "editor"
        };

        string[] levels = new string[]
        {
            "card_play",
            "map_play",
            "map_editor"
        };

        UIDocument ui_document;
        VisualElement root;
        Label title;
        Button[] ui_buttons;

        LevelFade fade;
        bool input_enabled = false;
        bool loading_request = false;
        string selected_level = "";

        void Start()
        {
            ui_document = GetComponent<UIDocument>();
            root = ui_document.rootVisualElement;

            title = root?.Q<Label>("title");
            VisualElement main_menu = root?.Q<VisualElement>("main-menu");
            ui_buttons = new Button[button_names.Length];

            for (int i = 0; i < button_names.Length; ++i)
            {
                ui_buttons[i] = main_menu?.Q<Button>(button_names[i]);
                ui_buttons[i].RegisterCallback<ClickEvent, int>(click_callback, i);
            }

            loading_request = false;

            fade = GameObject.Find("level_fade").GetComponent<LevelFade>();
            fade.fade_in_finished_event += fade_in_callback;
            fade.fade_out_finished_event += fade_out_callback;
            fade.set_state(LevelFade.STATE_FADE_IN);
            fade.begin();
        }

        void fade_in_callback() {
            input_enabled = true;
        }

        void fade_out_callback() {
            SceneManager.LoadScene(selected_level); // Async ?
        }

        void click_callback(ClickEvent e, int i)
        {
            if (!loading_request && input_enabled && i >= 0 && i < levels.Length)
            {
                loading_request = true;
                selected_level = levels[i];

                fade.set_state(LevelFade.STATE_FADE_OUT);
                fade.begin();
            }
        }

        void Update()
        {}
    }
}
