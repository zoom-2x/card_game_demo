using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro; 

namespace CardGame.Mono
{
    public class LevelLoader : MonoBehaviour
    {
        public bool show_text = true;
        public string loader_title = "Level loader text";

        Image bar;
        TextMeshProUGUI loader_text; 
        TextMeshProUGUI bar_text; 

        void Start()
        {
            Transform loader = transform.GetChild(0);
            Transform progress_bar = transform.GetChild(1);

            loader_text = loader.GetChild(1).GetComponent<TextMeshProUGUI>();
            loader_text.text = loader_title;

            bar = progress_bar.GetChild(2).GetComponent<Image>();
            bar.fillAmount = 0.0f;
            bar_text = progress_bar.GetChild(3).GetComponent<TextMeshProUGUI>();

            GameSystems.bundle_manager.start();
        }

        float prev = 0;

        void Update()
        {
            if (GameSystems.bundle_manager != null && GameSystems.bundle_manager.running)
            {
                GameSystems.bundle_manager.update();
                bar.fillAmount = GameSystems.bundle_manager.loading_progress;

                if (bar.fillAmount < prev)
                    Debug.Log($"current: {GameSystems.bundle_manager.loading_progress} / prev: {prev}");

                prev = GameSystems.bundle_manager.loading_progress;

                if (!show_text)
                    bar_text.text = $"{Mathf.RoundToInt(GameSystems.bundle_manager.loading_progress * 100)}%";
                else
                    bar_text.text = $"Loading {GameSystems.bundle_manager.loading_name}";
            }
        }
    }
}
