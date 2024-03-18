using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using TMPro;

namespace CardGame.Hexgrid
{
    public class RegionMeshInfo
    {
        TextMeshProUGUI name;
        TextMeshProUGUI[] resources; 

        public RegionMeshInfo(Transform region_mesh)
        {
            Transform ui = region_mesh.Find("ui");

            name = ui.Find("name").GetComponent<TextMeshProUGUI>();
            Transform ui_resources = ui.Find("resources");
            resources = new TextMeshProUGUI[ui_resources.childCount];

            for (int i = 0; i < ui_resources.childCount; ++i)
            {
                resources[i] = ui_resources.GetChild(i).Find("value").GetComponent<TextMeshProUGUI>();
                // resources[i].transform.parent.gameObject.SetActive(false);
            }
        }

        public void set_name(string name) {
            this.name.text = name;
        }

        public void set_value(int i, int value)
        {
            if (i < 0 || i >= resources.Length)
                return;

            resources[i].text = $"{value}";

            if (value == 0)
                hide(i);
            else
                show(i);
        }

        void show(int i)
        {
            if (i < 0 || i >= resources.Length)
                return;

            resources[i].transform.parent.gameObject.SetActive(true);
        }

        void hide(int i)
        {
            if (i < 0 || i >= resources.Length)
                return;

            resources[i].transform.parent.gameObject.SetActive(false);
        }
    }
}
