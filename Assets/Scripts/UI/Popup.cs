using System.Collections;
using System.Collections.Generic;
using System.Text;

using UnityEngine;
using UnityEngine.UIElements;

namespace CardGame.UI
{
    public struct PopupData
    {
        public float val;
        public string description;
    }

    public class Popup
    {
        public static VisualTreeAsset entry_template = null;
        VisualElement popup = null;
        VisualElement bkg = null;
        ListView list = null;
        List<PopupData> data = null;

        public Popup(VisualElement root)
        {
            popup = root?.Q<VisualElement>("popup-info");
            bkg = popup?.Q<VisualElement>("bkg");
            // list = popup?.Q<ListView>("info-list");
            // setup();
        }

        void setup()
        {
            list.makeItem = () =>
            {
                if (entry_template == null)
                    Debug.LogWarning("Popup: Missing item template !");

                return entry_template.Instantiate();
            };

            list.bindItem = (item, index) =>
            {
                if (data != null)
                {
                    PopupData info = data[index];

                    item.Q<Label>("value").text = $"{info.val}";
                    item.Q<Label>("description").text = info.description;
                }
            };
        }

        StringBuilder buffer = new StringBuilder();

        public void update_list(List<PopupData> data)
        {
            this.data = data;

            bkg.Clear();

            for (int i = 0; i < this.data.Count; ++i)
            {
                PopupData info = this.data[i];
                buffer.Clear();

                if (info.val < 0)
                {
                    buffer.Append("-");
                    buffer.Append(-info.val);
                }
                else
                {
                    buffer.Append("+");
                    buffer.Append(info.val);
                }

                buffer.Append(": ");
                buffer.Append(info.description);

                Label item = new Label(buffer.ToString());

                item.AddToClassList("popup-info-entry");
                item.RemoveFromClassList("popup-info-entry__plus");
                item.RemoveFromClassList("popup-info-entry__minus");

                if (info.val < 0)
                    item.AddToClassList("popup-info-entry__minus");
                else
                    item.AddToClassList("popup-info-entry__plus");

                bkg.Add(item);
            }
        }

        public void open(int x, int y)
        {
            popup.style.display = DisplayStyle.Flex;
            popup.style.left = x;
            popup.style.top = y;
        }

        public void close()
        {
            popup.style.display = DisplayStyle.None;
        }
    }
}
