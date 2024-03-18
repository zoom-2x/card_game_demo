using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

namespace CardGame.Mono
{
    public class LevelFade : MonoBehaviour
    {
        public static int STATE_IDLE = 0;
        public static int STATE_FADE_IN = 1;
        public static int STATE_FADE_OUT = 2;

        public event System.Action fade_in_finished_event;
        public event System.Action fade_out_finished_event;

        // seconds
        public float duration = 1;
        public float delay = 0;
        bool _running = false;
        float one_over_duration = 1;

        int _state = STATE_IDLE; 
        int _frame_count = 0;
        float time = 0;
        Image fade_bkg;

        void Awake()
        {
            Canvas fade_canvas = transform.GetChild(0).GetComponent<Canvas>();
            fade_bkg = fade_canvas.transform.GetChild(0).GetComponent<Image>();

            set_alpha(1);

            if (duration <= 0)
                duration = 1;

            one_over_duration = 1.0f / duration;
        }

        public bool visible {
            get { return fade_bkg.color.a == 1; }
        }

        void set_alpha(float a)
        {
            Color c = fade_bkg.color;
            c.a = a;
            fade_bkg.color = c;
        }

        public void set_state(int s)
        {
            if (_running)
                return;

            _state = s;
        }

        public bool running {
            get { return _running; }
        }

        public void begin() 
        {
            if (_running || _state == 0)
                return;

            _running = true;
            _frame_count = 0;
            time = 0;
            GameSystems.input.enable = false;
            // _state = next_state;
        }

        void Update()
        {
            if (_state == 0 || !_running)
                return;

            _frame_count++;

            // Delay the fade for the specified frame count.
            if (_frame_count < delay)
                return;

            time += Time.deltaTime;
            float t = Mathf.Clamp01(time * one_over_duration);
            t = 1 - (1 - t) * (1 - t);

            if (_state == STATE_FADE_IN)
                set_alpha(1 - t);
            else if (_state == STATE_FADE_OUT)
                set_alpha(t);

            // Finished.
            if (t == 1.0f)
            {
                if (_state == STATE_FADE_IN)
                {
                    fade_in_finished_event?.Invoke();
                    GameSystems.input.enable = true;
                }
                else if (_state == STATE_FADE_OUT)
                {
                    fade_out_finished_event?.Invoke();
                    GameSystems.input.enable = true;
                }

                _state = STATE_IDLE;
                _running = false;
            }
        }
    }
}
