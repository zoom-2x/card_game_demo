using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using UnityEngine.UIElements;

namespace CardGame.MapEditor.Panels
{
    public class MainMenuPanel : BasePanel
    {
        public event System.Action<int> menu_click;

        string[] button_names = new string[] {
            "new-map-button",
            "save-load-map-button",
            "region-button",
            "tile-button",
            "links-button",
            "main-menu-button"
        };

        bool[] button_state;
        Button[] buttons;

        public MainMenuPanel(VisualElement root) : base(root, "main-menu")
        {
            button_state = new bool[button_names.Length];
            buttons = new Button[button_names.Length];

            for (int i = 0; i < button_names.Length; ++i)
            {
                buttons[i] = panel.Q<Button>(button_names[i]);
                button_state[i] = false;
                buttons[i].RegisterCallback<ClickEvent, int>(click_callback, i);
            }
        }

        public void debug()
        {
            Debug.Log(panel.resolvedStyle.height);
            Debug.Log($"menu: {panel.resolvedStyle.height} / {panel.resolvedStyle.top} / {panel.resolvedStyle.left}");
        }

        public void state_on(int i)
        {
            button_state[i] = true;
            buttons[i].AddToClassList("selected");           
        }

        public void state_off(int i)
        {
            button_state[i] = false;
            buttons[i].RemoveFromClassList("selected");           
        }
        
        void click_callback(ClickEvent a, int i)
        {
            menu_click?.Invoke(i);
        }
    }
}
