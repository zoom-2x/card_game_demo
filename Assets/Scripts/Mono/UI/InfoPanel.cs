using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace CardGame.Mono.UI
{
    public class InfoPanel : MonoBehaviour
    {
        public bool start_open = false;
        public bool has_background = true;
        public bool has_scroll = true;
        [Range(0, 1)] public float background_opacity = 1.0f;
        // public bool fade_in = true;
        // public float fade_duration = 1.0f;
        [TextAreaAttribute(10, 5)] public string panel_text = "";

        Transform content;

        Transform panel0;
        Transform panel1;

        TextMeshProUGUI t0;
        TextMeshProUGUI t1;

        Image bkg;

        void Start()
        {
            content = transform.GetChild(0);

            bkg = content.GetComponent<Image>();
            bkg.color = new Color(0, 0, 0, background_opacity);

            panel0 = content.GetChild(0); 
            panel1 = content.GetChild(1); 

            t0 = panel0.GetChild(0).GetComponent<TextMeshProUGUI>();
            t1 = panel1.GetChild(0).GetChild(0).GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>();

            t0.text = panel_text;
            t1.text = panel_text;

            if (has_scroll)
            {
                panel0.gameObject.SetActive(false);
                panel1.gameObject.SetActive(true);
            }
            else
            {
                panel0.gameObject.SetActive(true);
                panel1.gameObject.SetActive(false);
            }

            if (start_open)
                open();
        }

        public void open()
        {
            content.gameObject.SetActive(true);
        }

        public void close()
        {
            content.gameObject.SetActive(false);
        }

        public void set_text(string text)
        {
            if (!String.IsNullOrEmpty(text))
            {
                panel_text = text;

                if (has_scroll)
                    t1.text = panel_text;
                else
                    t0.text = panel_text;
            }
        }
    }
}
