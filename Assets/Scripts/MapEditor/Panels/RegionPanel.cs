using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UIElements;

using CardGame.Hexgrid;

namespace CardGame.MapEditor.Panels
{
    public struct RegionData
    {
        public string name;
        public Color color;
        public uint resource_mask;
        public uint resource_value_mask;
    }

    public class RegionPanel : BasePanel
    {
        public static VisualTreeAsset list_item_template = null;

        public static event System.Action new_region_event;

        Button new_region_button = null;
        // Button edit_region_button = null;

        ListView region_list = null;
        List<RegionData> data = null;

        RegionEditPanel region_edit_panel = null;

        public RegionPanel(VisualElement root) : base(root, "region-list-panel")
        {
            RegionEditPanel.close_event += close_region_edit_panel;

            region_edit_panel = new RegionEditPanel(root);

            new_region_button = panel?.Q<Button>("new-region-button");
            // edit_region_button= panel?.Q<Button>("edit-region-button");
            region_list = panel?.Q<ListView>("region-list");
            
            new_region_button.RegisterCallback<ClickEvent>(new_region_callback);
            // edit_region_button.RegisterCallback<ClickEvent>(edit_region_callback);

            region_list.selectionChanged += on_changed;
            list_setup();
        }

        public override void register_enter_leave_callback(System.Action<bool> callback)
        {
            enter_leave_event += callback;
            region_edit_panel.enter_leave_event += callback;
        }

        void list_setup()
        {
            region_list.makeItem = () =>
            {
                if (RegionPanel.list_item_template == null)
                    Debug.LogWarning("RegionPanel: Missing item template !");

                return RegionPanel.list_item_template.Instantiate();
            };

            region_list.bindItem = (item, index) =>
            {
                if (data != null)
                {
                    RegionData region = data[index];
                    Label name = item.Q<Label>("region-name");
                    VisualElement color = item.Q<VisualElement>("color");
                    VisualElement list = item.Q<VisualElement>("resource-view-list");

                    name.text = region.name;
                    color.style.backgroundColor = region.color;

                    for (int i = 0; i < 5; ++i)
                    {
                        VisualElement res = list.ElementAt(i);
                        res.style.display = DisplayStyle.None;

                        Label res_value = res.Q<Label>("resource-value");
                        uint count = HexRegion.extract_resource_value((uint) i, region.resource_value_mask);
                        res_value.text = $"{count}";

                        if ((region.resource_mask & HexRegion.RESOURCE_MASK[i]) > 0)
                            res.style.display = DisplayStyle.Flex;
                    }
                }
            };
        }

        void new_region_callback(ClickEvent e) {
            new_region_event?.Invoke();
        }

        void edit_region_callback(ClickEvent e)
        {
            if (region_list.selectedIndex > -1 && data != null)
            {
                region_edit_panel.set_region(data[region_list.selectedIndex], region_list.selectedIndex);
                region_edit_panel.open();
            }
        }

        void close_region_edit_panel() {
            region_edit_panel.close();
        }

        public void update_list(List<RegionData> data)
        {
            this.data = data;

            region_list.itemsSource = data;
            region_list.RefreshItems();
        }

        void on_changed(IEnumerable s)
        {
            if (region_list.selectedIndex > -1)
            {
                region_edit_panel.set_region(data[region_list.selectedIndex], region_list.selectedIndex);
                region_edit_panel.open();
            }
        }

        public override void open()
        {
            base.open();
            region_list.selectedIndex = -1;
        }

        public override void close()
        {
            base.close();
            region_edit_panel.close();
        }
    }
}
