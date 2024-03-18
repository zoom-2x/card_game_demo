using UnityEngine;
using UnityEngine.UIElements;

namespace CardGame.MapEditor.Panels
{
    public class NewMapPanel : BasePanel
    {
        public static event System.Action<int, int> generate_event;

        IntegerField rows = null; 
        IntegerField cols = null; 
        Button generate_button = null;

        public NewMapPanel(VisualElement root) : base(root, "new-map-panel")
        {
            rows = panel?.Q<IntegerField>("map-rows");
            cols = panel?.Q<IntegerField>("map-cols");

            generate_button = panel?.Q<Button>("generate-map-button");
            generate_button.RegisterCallback<ClickEvent>(generate_callback);
        }

        void generate_callback(ClickEvent e) 
        {
            int map_rows = rows.value;
            int map_cols = cols.value;

            if (map_rows <= 0)
            {
                map_rows = 10;
                rows.value = 10;
            }

            if (map_cols <= 0)
            {
                map_cols = 10;
                cols.value = 10;
            }

            generate_event?.Invoke(map_rows, map_cols);
        }
    }
}
