using UnityEngine;
using UnityEngine.UIElements;

namespace CardGame.MapEditor.Panels
{
    public class SaveLoadMapPanel : BasePanel
    {
        public static event System.Action<string, string> save_map_event;
        public static event System.Action<string> load_map_event;

        TextField map_name = null;
        TextField map_filepath = null;

        Button save = null;
        Button load = null;

        public SaveLoadMapPanel(VisualElement root) : base(root, "save-load-map-panel")
        {
            map_name = panel?.Q<TextField>("map-name");
            map_filepath = panel?.Q<TextField>("map-filepath");

            save = panel?.Q<Button>("save-map-button");
            load = panel?.Q<Button>("load-map-button");

            save.RegisterCallback<ClickEvent>(save_callback);
            load.RegisterCallback<ClickEvent>(load_callback);
        }

        void save_callback(ClickEvent e) {
            save_map_event?.Invoke(map_name.text, map_filepath.text);
        }

        void load_callback(ClickEvent e) {
            load_map_event?.Invoke(map_filepath.text);
        }

        public void set_map_name(string name) {
            map_name.value = name;
        }

        public void set_map_filepath(string filepath) {
            map_filepath.value = filepath;
        }
    }
}
