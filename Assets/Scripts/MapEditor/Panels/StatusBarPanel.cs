using System.Collections;
using System.Collections.Generic;
using System.Text;

using UnityEngine;
using UnityEngine.UIElements;

using CardGame.Hexgrid;

namespace CardGame.MapEditor.Panels
{
    public class StatusBarPanel: BasePanel
    {
        float message_duration = 5;
        float message_time = 0;

        Label map_size = null;
        Label coordinates = null;
        Label tile_region = null;
        Label message = null;

        StringBuilder buffer = new StringBuilder(25);

        public StatusBarPanel(VisualElement root) : base(root, "status-bar")
        {
            map_size = panel?.Q<Label>("map-size");
            coordinates = panel?.Q<Label>("coordinates");
            tile_region = panel?.Q<Label>("tile-region");
            message = panel?.Q<Label>("message");
            set_message("");
        }

        public void set_map_size(int rows, int cols) 
        {
            buffer.Clear();
            buffer.Append("Map size: ");

            if (rows == HexMap.INVALID)
                buffer.Append("INV");
            else
                buffer.Append(rows);

            buffer.Append('x');

            if (cols == HexMap.INVALID)
                buffer.Append("INV");
            else
                buffer.Append(cols);

            map_size.text = buffer.ToString();
        }

        public void set_tile_region(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                tile_region.style.display = DisplayStyle.None;
                return;
            }
            else
                tile_region.style.display = DisplayStyle.Flex;

            tile_region.text = name;
        }

        public void set_coordinates(Vector2Int c)
        {  
            buffer.Clear();
            buffer.Append("Coordinates: ");

            if (c.x == HexMap.INVALID)
                buffer.Append("INV");
            else
                buffer.Append(c.x);

            buffer.Append(':');

            if (c.y == HexMap.INVALID)
                buffer.Append("INV");
            else
                buffer.Append(c.y);

            coordinates.text = buffer.ToString();
        }

        public void set_message(string s)
        {
            if (string.IsNullOrEmpty(s))
            {
                message.style.display = DisplayStyle.None;
                return;
            }
            else
                message.style.display = DisplayStyle.Flex;

            message.text = s;
            message_time = 0;
        }

        public void update()
        {
            if (message_time == -1)
                return;

            message_time += Time.deltaTime;

            if (message_time >= message_duration)
            {
                message_time = -1;
                set_message(null);
            }
        }
    }
}
