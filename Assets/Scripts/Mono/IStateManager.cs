using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace CardGame.Mono
{
    public interface IStateManager
    {
        public void set_active(bool active);
        public void frame_update();
    }
}

