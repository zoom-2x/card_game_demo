using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using CardGame.Data;

namespace CardGame.Mono.State
{
    public class PlayPreviewState : MonoBehaviour 
    {
        public const uint OPEN = 1;
        public const uint CLOSED = 2;
        public const uint TOGGLE = 3;

        uint _state = CLOSED;
        bool initialized = false;

        public float duration_sec = 0.5f;
        public Vector3 offset = Vector3.zero;

        float one_over_duration_sec = 1.0f;
        AnimationUnit unit = new AnimationUnit();

        public uint state {
            get {return _state;}
            set {_state = value;}
        }

        void initialize()
        {
            initialized = true;

            unit.t = 0;
            unit.time = 0;

            if (duration_sec <= 0)
                duration_sec = 1;

            one_over_duration_sec = 1.0f / duration_sec;
            Vector3 view_position = GameSystems.game.scene_camera.WorldToViewportPoint(transform.position);

            unit.start = view_position + offset;
            unit.end = view_position;

            transform.gameObject.SetActive(true);
        }

        void play()
        {}

        void toggle()
        {
            unit.time += Time.deltaTime;
            unit.t = unit.time * one_over_duration_sec;

            if (unit.t > 1.0f)
                unit.t = 1.0f;

            Vector3 vp = Vector3.zero;

            if (state == CLOSED)
                vp = (Utils.ease_out_cubic(unit.start, unit.end, unit.t));
            else if (state == OPEN)
                vp = Utils.ease_out_cubic(unit.end, unit.start, unit.t);

            transform.position = GameSystems.game.scene_camera.ViewportToWorldPoint(vp);

            if (unit.t >= 1.0f)
            {
                unit.t = 0;
                unit.time = 0;
                state = state == OPEN ? CLOSED : OPEN;
            }
        }

        public void update()
        {
            if (!initialized)
                initialize();

            if (state == OPEN)
                play();
            else if (state == TOGGLE)
                toggle();
        }
    }
}

