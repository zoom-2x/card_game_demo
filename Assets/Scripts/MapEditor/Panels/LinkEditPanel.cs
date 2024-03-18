using UnityEngine;
using UnityEngine.UIElements;

namespace CardGame.MapEditor.Panels
{
    public class LinkEditPanel: BasePanel
    {
        public static event System.Action<Vector2Int, Vector2Int, Vector2Int, int> save_link_event;

        IntegerField start_tile_row = null;
        IntegerField start_tile_col = null;

        IntegerField middle_tile_row = null;
        IntegerField middle_tile_col = null;

        IntegerField end_tile_row = null;
        IntegerField end_tile_col = null;

        Button save = null;

        LinkData data = new LinkData();
        int index = -1;

        public LinkEditPanel(VisualElement root) : base(root, "links-panel")
        {
            start_tile_row = panel?.Q<IntegerField>("start-tile-row");
            start_tile_col = panel?.Q<IntegerField>("start-tile-col");

            middle_tile_row = panel?.Q<IntegerField>("middle-tile-row");
            middle_tile_col = panel?.Q<IntegerField>("middle-tile-col");

            end_tile_row = panel?.Q<IntegerField>("end-tile-row");
            end_tile_col = panel?.Q<IntegerField>("end-tile-col");

            save = panel?.Q<Button>("save-link-button");

            save.RegisterCallback<ClickEvent>(save_callback);
        }

        void save_callback(ClickEvent e) 
        {
            Vector2Int start_tile = new Vector2Int(start_tile_row.value, start_tile_col.value);
            Vector2Int middle_tile = new Vector2Int(middle_tile_row.value, middle_tile_col.value);
            Vector2Int end_tile = new Vector2Int(end_tile_row.value, end_tile_col.value);

            save_link_event?.Invoke(start_tile, middle_tile, end_tile, index);
        }

        public void set_data(LinkData data, int index) 
        {
            this.data = data;
            this.index = index;
            
            start_tile_row.value = data.start_tile.x;
            start_tile_col.value = data.start_tile.y;

            middle_tile_row.value = data.middle_tile.x;
            middle_tile_col.value = data.middle_tile.y;

            end_tile_row.value = data.end_tile.x;
            end_tile_col.value = data.end_tile.y;

            Debug.Log(data);
            Debug.Log(index);
        }
    }
}
