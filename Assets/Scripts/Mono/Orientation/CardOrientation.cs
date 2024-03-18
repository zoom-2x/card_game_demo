using UnityEngine;

using CardGame.Data;

namespace CardGame.Mono.Orientation
{
    public class CardOrientation : MonoBehaviour
    {
        [HideInInspector] public bool absolute = true;
        [HideInInspector] public int virtual_count = 0;

        public int recommended_count = 10;
        private int old_count = 0;

        public virtual Vector3 position(int index) {return Vector3.zero;}
        public virtual Vector3 position_t(float t) {return Vector3.zero;}
        public virtual float rotation(int index) {return 0;}
        public virtual float rotation_t(float t) {return 0;}
        public virtual float spacing() {return 0;}
        public virtual float spacing_t() {return 0;}
        public virtual float relative_position(int index) {return 0;}

        public void push_count(int count = 0)
        {
            old_count = virtual_count;

            if (count > 0)
                virtual_count = count;
        }

        public void pop_count() {
            virtual_count = old_count;
        }
    }
}
