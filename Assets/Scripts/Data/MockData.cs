using System.Collections;
using System.Collections.Generic;
using System.Text;

using UnityEngine;

using CardGame.UI;

namespace CardGame.Data
{
    public class MockData
    {
        static StringBuilder builder = new StringBuilder();

        public static void generate_popup_sample_data(List<InfoData> list)
        {
            int count = Mathf.FloorToInt(Random.value * 10) + 1;
            list.Clear();

            for (int i = 0; i < count; ++i)
            {
                int type = Mathf.FloorToInt(Random.value * 1 + 0.5f);
                float val = (float) System.Math.Round(Random.value * 9 + 1, 1);
                builder.Clear();

                if (type == 1)
                {
                    builder.Append("-");
                    builder.Append(val);
                }
                else
                {
                    builder.Append("+");
                    builder.Append(val);
                }

                list.Add(new InfoData() {type = type, text = $"{builder.ToString()}: Region {i + 1}"});
            }
        }
    }
}
