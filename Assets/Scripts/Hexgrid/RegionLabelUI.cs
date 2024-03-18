using System.Collections;
using System.Collections.Generic;
using System;

using UnityEngine;
using UnityEngine.UIElements;
using TMPro;

using CardGame.Hexgrid;

namespace CardGame.Hexgrid
{
    public class RegionLabelUI : RegionLabel
    {
        Transform map = null;
        Camera camera = null;
        GameObject label_obj = null;

        bool disabled = true;

        Transform main_ui_canvas;
        Transform ui_region_labels;
        Transform ui_label;

        Transform resources;

        TextMeshProUGUI name;
        Transform extra_info;
        TextMeshProUGUI extra_info_tmp;
        TextMeshProUGUI[] values = new TextMeshProUGUI[HexRegion.RES_COUNT];

        public RegionLabelUI()
        {
            map = GameObject.Find("MAP").transform;
            camera = GameObject.Find("MAIN_CAMERA").GetComponent<Camera>();

            label_obj = new GameObject();
            label_obj.transform.parent = map;
            label_obj.name = "region_label_object";

            if (!disabled)
            {
                GameObject ui = GameObject.Find("UI");
                main_ui_canvas = ui.transform.Find("main_ui_canvas");
                ui_region_labels = ui.transform.Find("region_labels_canvas").Find("region_labels");

                ui_label = GameSystems.asset_manager.aquire_region_label();
                ui_label.SetParent(ui_region_labels);
                ui_label.gameObject.SetActive(true);

                Transform info = ui_label.Find("info");

                name = ui_label.Find("header").GetChild(0).GetComponent<TextMeshProUGUI>();
                extra_info = info.Find("extra_info");
                extra_info_tmp = extra_info.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>();

                resources = info.Find("resources");

                for (int i = 0; i < resources.childCount; ++i)
                {
                    values[i] = resources.GetChild(i).GetChild(1).GetComponent<TextMeshProUGUI>();
                    set_value(i, 0);
                }

                set_extra_info();
            }
        }

        public void set_object_position(Vector3 position)
        {
            if (disabled)
                return;

            label_obj.transform.position = position;
        }

        public void set_name(string s)
        {
            if (disabled)
                return;

            if (!String.IsNullOrWhiteSpace(s))
                name.text = s.ToUpper();
        }

        public void set_extra_info(string s = null)
        {
            if (disabled)
                return;

            if (!String.IsNullOrWhiteSpace(s))
            {
                extra_info_tmp.text = s.ToUpper();
                extra_info.gameObject.SetActive(true);
            }
            else
                extra_info.gameObject.SetActive(false);
        }

        public void set_value(int resource, uint v)
        {
            if (disabled)
                return;

            if (resource < 0 || resource >= HexRegion.RES_COUNT || v < 0 || v > 8)
                return;

            if (v == 0)
                resources.GetChild(resource).gameObject.SetActive(false);
            else
                resources.GetChild(resource).gameObject.SetActive(true);

            values[resource].text = $"{v}";
        }

        void remove()
        {
            if (disabled)
                return;

            ui_label.parent = null;
            ui_label.gameObject.SetActive(false);
            GameSystems.asset_manager.return_region_label(ui_label.gameObject);
        }

        public void update_ui_position()
        {
            if (disabled)
                return;

            Vector3 screen_position = camera.WorldToScreenPoint(label_obj.transform.position);
            RectTransform rt = ui_label as RectTransform;
            // NOTE(gabic): Problema de performanta cand se schimba pozitia,
            // canvas-ul este "recreat".
            rt.anchoredPosition = new Vector2(screen_position.x, screen_position.y);
        }

        public void hide()
        {
            if (disabled)
                return;

            ui_label.gameObject.SetActive(false);
        }

        public void show()
        {
            if (disabled)
                return;

            ui_label.gameObject.SetActive(true);
        }

        public void destroy()
        {
            if (disabled)
                return;

            Debug.Log("> destroying label...");

            remove();
            GameObject.Destroy(label_obj);
        }
    }
}
