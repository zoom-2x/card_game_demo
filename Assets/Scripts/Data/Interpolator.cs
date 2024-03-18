using UnityEngine;

namespace CardGame.Data
{
    public class Interpolator
    {
        public bool enabled = false;
        public float time = 0;
        public float t = 0;
        public float[] start = new float[10];
        public float[] end = new float[10];
    }
}
