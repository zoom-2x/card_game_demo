using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UIElements;

using CardGame.Hexgrid;

namespace CardGame.MapEditor.Panels
{
    public struct TileRegionData
    {
        public int index; 
        public string name;
        public Color color;
    }

    public class TilePanel : BasePanel
    {
        public static VisualTreeAsset list_item_template = null;
        public static event System.Action<int> select_tile_region_event;

        ListView region_list = null;
        List<TileRegionData> data = null;

        public TilePanel(VisualElement root) : base(root, "tile-panel")
        {
            region_list = panel?.Q<ListView>("region-list");
            region_list.selectionChanged += select_region;
            list_setup();
        }

        void select_region(IEnumerable<object> e) {
            select_tile_region_event?.Invoke(region_list.selectedIndex);
        }

        public void update_list(List<TileRegionData> data)
        {
            this.data = data;

            region_list.itemsSource = data;
            region_list.RefreshItems();
        }

        void list_setup()
        {
            region_list.makeItem = () =>
            {
                if (RegionPanel.list_item_template == null)
                    Debug.LogWarning("TilePanel: Missing item template !");

                return TilePanel.list_item_template.Instantiate();
            };

            region_list.bindItem = (item, index) =>
            {
                if (data != null)
                {
                    TileRegionData region = data[index];
                    Label name = item.Q<Label>("region-name");
                    VisualElement color = item.Q<VisualElement>("color");
                    VisualElement list = item.Q<VisualElement>("resource-view-list");

                    name.text = region.name;
                    color.style.backgroundColor = region.color;
                }
            };
        }
    }
}
