using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using CardGame.Data;
using CardGame.Mono.Orientation;

namespace CardGame.Mono.Orientation
{
    public class Vertical : CardOrientation
    {
        public override Vector3 position(int i) {
            return new Vector3(0, Constants.CARD_DEPTH * 0.5f + i * Constants.CARD_DEPTH, 0);
        }

        public override float spacing() {
            return Constants.CARD_DEPTH;
        }
    }
}
