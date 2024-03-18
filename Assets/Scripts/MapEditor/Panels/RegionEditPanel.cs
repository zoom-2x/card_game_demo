using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UIElements;

using CardGame.Hexgrid;

namespace CardGame.MapEditor.Panels
{
    public class RegionEditPanel : BasePanel
    {
        public static event System.Action<RegionData, int> update_region_event;
        public static event System.Action<int> delete_region_event;
        public static event System.Action close_event;

        RegionData region = new RegionData();
        int region_index = -1;

        Button update_button = null;
        // Button close_button = null;
        Button delete_button = null;

        TextField color = null;
        TextField name = null;

        VisualElement resource_list = null;

        bool delete_confirmation = false;
        int active_resources = 0;

        public RegionEditPanel(VisualElement root) : base(root, "region-edit-panel")
        {
            update_button = panel?.Q<Button>("update-button");
            // close_button = panel?.Q<Button>("close-button");
            delete_button = panel?.Q<Button>("delete-button");

            color = panel?.Q<TextField>("color");
            name = panel?.Q<TextField>("name");

            resource_list = panel?.Q<VisualElement>("resource-list");

            for (int i = 0; i < resource_list.childCount; ++i)
            {
                VisualElement resource = resource_list.ElementAt(i);
                Toggle toggle = resource.Q<Toggle>("toggle");
                toggle.RegisterCallback<ChangeEvent<bool>>(toggle_changed_callback);
            }

            update_button.RegisterCallback<ClickEvent>(update_region_callback);
            // close_button.RegisterCallback<ClickEvent>(close_panel_callback);
            delete_button.RegisterCallback<ClickEvent>(delete_panel_callback);
        }

        public void set_region(RegionData region, int index)
        {
            this.region = region;
            region_index = index;

            color.value = $"#{ColorUtility.ToHtmlStringRGB(region.color)}";
            name.value = region.name;
            active_resources = 0;

            // Setup the resources.
            for (int i = 0; i < resource_list.childCount; ++i)
            {
                VisualElement resource = resource_list.ElementAt(i);
                SliderInt slider = resource.Q<SliderInt>("count");
                Toggle toggle = resource.Q<Toggle>("toggle");

                if (HexRegion.has_resource((uint) i, region.resource_mask))
                {
                    uint value = HexRegion.extract_resource_value((uint) i, region.resource_value_mask);

                    slider.value = (int) value;
                    toggle.SetValueWithoutNotify(true);

                    active_resources++;
                }

                else
                {
                    slider.value = 0;
                    toggle.SetValueWithoutNotify(false);
                }
            }
        }

        void toggle_changed_callback(ChangeEvent<bool> e)
        {
            Toggle toggle = e.target as Toggle;

            if (e.newValue)
            {
                if (active_resources == 3)
                {
                    Debug.Log($"over the limit: {active_resources} / {toggle.value}");
                    toggle.SetValueWithoutNotify(false);
                }
                else
                    active_resources++;
            }
            else
                active_resources--;
        }

        void update_region_callback(ClickEvent e)
        {
            if (color.text[0] != '#')
                color.value = $"#{color.text}";

            ColorUtility.TryParseHtmlString(color.text, out region.color);
            region.name = name.text;

            region.resource_mask = 0;
            region.resource_value_mask = 0;

            for (int i = 0; i < resource_list.childCount; ++i)
            {
                VisualElement resource = resource_list.ElementAt(i);
                SliderInt slider = resource.Q<SliderInt>("count");
                Toggle toggle = resource.Q<Toggle>("toggle");

                if (toggle.value)
                {
                    region.resource_mask |= HexRegion.get_resource_mask((uint) i);
                    region.resource_value_mask |= HexRegion.get_resource_value_mask((uint) i, (ushort) slider.value);
                }

                if (HexRegion.has_resource((uint) i, region.resource_mask))
                {
                    uint value = HexRegion.extract_resource_value((uint) i, region.resource_value_mask);

                    slider.value = (int) value;
                    toggle.SetValueWithoutNotify(true);

                    active_resources++;
                }

                else
                {
                    slider.value = 0;
                    toggle.SetValueWithoutNotify(false);
                }
            }

            update_region_event?.Invoke(region, region_index);
        }

        void close_panel_callback(ClickEvent e) {
            close_event?.Invoke();
        }

        void delete_panel_callback(ClickEvent e)
        {
            if (!delete_confirmation)
            {
                delete_confirmation = true;
                delete_button.text = "Delete (Confirm)";
            }
            else
            {
                delete_confirmation = false;
                delete_button.text = "Delete";

                delete_region_event?.Invoke(region_index);
                close_event?.Invoke();
            }
        }
    }
}
