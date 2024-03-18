using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using CardGame.Data;

namespace CardGame.Data
{
    public class CardStyle
    {
        public string tex_art_mask;
        public string tex_outline_mask;
        public string tex_front;
        public string tex_back;
        public string tex_metallic_front;
        public string tex_metallic_back;
        public string tex_smoothness_front;
        public string tex_smoothness_back;
        public string tex_normal_front;
        public string tex_normal_back;
    }

    public class Card
    {
        public ushort id;
        public string name;
        public string description;
        public string art_name;
        public float art_offset;
        public int cost;
        // 5 x 1 effect (4 bytes) = 20 bytes
        public byte[] effects;
    }
}
