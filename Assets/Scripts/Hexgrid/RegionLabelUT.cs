using System.Collections;
using System.Collections.Generic;
using System;

using UnityEngine;
using UnityEngine.UIElements;

using CardGame.Hexgrid;
using CardGame.Managers;

namespace CardGame.Hexgrid
{
    public class RegionLabelUT : RegionLabel
    {
        Transform map = null;
        Camera camera = null;

        VisualElement labels_ui_container = null;
        VisualTreeAsset label_template = null;

        VisualElement template_container = null;
        VisualElement root = null;
        VisualElement labels_ui_root = null;

        Label name = null;
        Label extra_info = null;

        VisualElement[] resources = new VisualElement[HexRegion.RES_COUNT];
        Label[] values = new Label[HexRegion.RES_COUNT];

        public GameObject label_obj = null;

        public RegionLabelUT()
        {
            map = GameObject.Find("MAP").transform;
            camera = GameObject.Find("MAIN_CAMERA").GetComponent<Camera>(); UIDocument labels_ui_document = GameObject.Find("UI_LABELS").GetComponent<UIDocument>();

            labels_ui_container = labels_ui_document.rootVisualElement;
            label_template = GameSystems.asset_manager.aquire_ut_tree_asset(BundleEnum.UI, "region_label");

            label_obj = new GameObject();
            label_obj.transform.parent = map;

            labels_ui_root = labels_ui_container?.Q<VisualElement>("root");

            template_container= label_template.Instantiate();
            root = template_container.Q<VisualElement>("root");
            labels_ui_root.Add(template_container);

            name = root?.Q<Label>("name");
            extra_info = root?.Q<Label>("extra_info");

            VisualElement res_base = root?.Q<VisualElement>("resources");

            for (int i = 0; i < HexRegion.RES_COUNT; ++i)
            {
                resources[i] = res_base.ElementAt(i);
                values[i] = resources[i].Q<Label>("value");

                set_value(i, 0);
            }

            set_extra_info();
        }

        public void set_object_position(Vector3 position) {
            label_obj.transform.position = position;
        }

        public void set_name(string s)
        {
            if (!String.IsNullOrWhiteSpace(s))
                name.text = s.ToUpper();
        }

        public void set_extra_info(string s = null)
        {
            if (!String.IsNullOrWhiteSpace(s))
            {
                extra_info.text = s.ToUpper();
                extra_info.style.display = DisplayStyle.Flex;
            }
            else
                extra_info.style.display = DisplayStyle.None;
        }

        public void set_value(int resource, uint v)
        {
            if (resource < 0 || resource >= HexRegion.RES_COUNT || v < 0 || v > 8)
                return;

            if (v == 0)
                resources[resource].style.display = DisplayStyle.None;
            else
                resources[resource].style.display = DisplayStyle.Flex;

            values[resource].text = $"{v}";
        }

        void remove() {
            labels_ui_root.Remove(template_container);
        }

        public void update_ui_position()
        {
            Vector3 tp = RuntimePanelUtils.CameraTransformWorldToPanel(
                                            root.panel,
                                            label_obj.transform.position,
                                            camera);

            tp += new Vector3(-root.contentRect.width * 0.5f, -root.contentRect.height * 0.5f, 0);
            root.transform.position = tp;
        }

        public void hide() {
            root.style.display = DisplayStyle.None;
        }

        public void show() {
            root.style.display = DisplayStyle.Flex;
        }

        public void destroy()
        {
            Debug.Log("> destroying label...");

            // Remove the label from the ui.
            labels_ui_root.Remove(template_container);
            // Destroy the GameObject used to represent the label's world position.
            GameObject.Destroy(label_obj);
        }
    }
}
