using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CardGame.Data
{
    // ap = action points
    // ip = influence points
    // cr = credits
    // vp = victory points
    // it = influence tokens
    // rit = remove influence tokens
    // tp = token protections
    // actx2 = double action effect

    public enum Resource
    {
        CREDITS = 0,
        EXPANSION = 1,
        RESISTANCE = 2,
        INFLUENCE = 3,
        // COUNT = 4
    }

    public class PlayerResources
    {
        public const int COUNT = 4;

        public int vp = 0;
        public int[] values = new int[COUNT];

        public int get(Resource r) {
            return values[(int) r];
        }

        public void set(Resource r, int val) {
            values[(int) r] = val;
        }

        public void add(Resource r, int v)
        {
            int val = get(r) + v;
            set(r, val);
        }

        public void subtract(Resource r, int v)
        {
            int val = get(r) - v;
            if (val < 0) val = 0;
            set(r, val);
        }

        public void incr(Resource r) {
            add(r, 1);
        }

        public void decr(Resource r) {
            subtract(r, 1);
        }
    }
}
