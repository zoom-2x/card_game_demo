using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UIElements;

namespace CardGame.MapEditor.Panels
{
    public struct LinkData
    {
        public Vector2Int start_tile;
        public Vector2Int middle_tile;
        public Vector2Int end_tile;
    }

    public class LinkListPanel: BasePanel
    {
        public static VisualTreeAsset list_item_template = null;
        public static event System.Action new_link_event;

        Button new_link = null;
        ListView link_list = null;
        LinkEditPanel edit_panel = null;
        List<LinkData> data = null;

        public LinkListPanel(VisualElement root) : base(root, "link-list-panel")
        {
            link_list = panel?.Q<ListView>("link-list");
            edit_panel = new LinkEditPanel(root);
            new_link = panel?.Q<Button>("new-link-button");

            new_link.RegisterCallback<ClickEvent>(new_link_callback);
            list_setup();

            link_list.selectionChanged += on_changed;
        }

        void new_link_callback(ClickEvent e) {
            new_link_event?.Invoke();
        }

        void list_setup()
        {
            link_list.makeItem = () =>
            {
                if (list_item_template == null)
                    Debug.LogWarning("LinkListPanel: Missing item template !");

                return list_item_template.Instantiate();
            };

            link_list.bindItem = (item, index) =>
            {
                if (data != null)
                {
                    LinkData link = data[index];
                    Label name = item.Q<Label>("link-name");

                    name.text = $"Link: ({link.start_tile.x}, {link.start_tile.y}) / ({link.middle_tile.x}, {link.middle_tile.y}) / ({link.end_tile.x}, {link.end_tile.y})";
                }
            };
        }

        public void update_list(List<LinkData> data)
        {
            this.data = data;

            link_list.itemsSource = data;
            link_list.RefreshItems();
        }

        void on_changed(IEnumerable s)
        {
            if (link_list.selectedIndex > -1)
            {
                edit_panel.set_data(data[link_list.selectedIndex], link_list.selectedIndex);
                edit_panel.open();
            }
        }

        public override void open()
        {
            base.open();
            link_list.selectedIndex = -1;
        }

        public override void close()
        {
            base.close();
            edit_panel.close();
        }
    }
}
