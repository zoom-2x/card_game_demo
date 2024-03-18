using System;
using System.Collections;
using System.Collections.Generic;

using CardGame.Data;

namespace CardGame.Animation
{
    public abstract class AnimationAbstract
    {
        public float t = 0;
        public float time = 0;

        protected float duration_sec = 1.0f;
        protected float one_over_duration_sec = 1;

        public CommandEvent on_finished_event = new CommandEvent();
        // true = animation finished, false = animation not finished
        public abstract bool frame_update();

        public void set_duration(float val)
        {
            duration_sec = val;
            
            if (duration_sec <= 0)
                duration_sec = 1;

            one_over_duration_sec = 1.0f / duration_sec;
        }
    }
}
