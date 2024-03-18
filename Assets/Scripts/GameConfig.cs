using UnityEngine;
using CardGame.IO;

namespace CardGame
{
    public class GameSettings
    {
        public bool debug_mode = false;

        public float play_threshold = 0.6f;
        public int frame_rate = 200;

        public float inspect_card_forward_duration = 0.5f;
        public float inspect_card_backward_duration = 0.35f;

        public float label_hide_zoom = 5;
        public float card_zoom = 1.5f;

        public int resistance_base_strength = 5;
        public int resistance_base_empty_strength = 0;
        public int expansion_base_strength = 7;
        public int expansion_base_empty_strength = 10;
        public int influence_base_strength = 2;
    }

    public class CardSettings
    {
        public bool use_unlit = true;
        public Color glow_color = Color.white;
        public float glow_brightness = 1.2f;
    }

    public class ContainerSettings
    {
        public float play_preview_duration_sec = 1.0f;
        public Vector3 play_preview_offset = Vector3.zero;
        
        public float discard_preview_duration_sec = 1.0f;
        public Vector3 discard_preview_offset = Vector3.zero; 
        public float discard_preview_delay_sec = 0;
    }

    public class PlaceholderSettings
    {
        public Color line_color = Color.white;
        public Color line_color_invalid = Color.red;
    }

    public class GameConfig
    {
        public static GameSettings game = new GameSettings();
        public static CardSettings card = new CardSettings();
        public static ContainerSettings container = new ContainerSettings();
        public static PlaceholderSettings placeholder = new PlaceholderSettings();

        public static void load()
        {
            IniReader reader = new IniReader(Utils.fullpath("game/game.ini")); 

            game.debug_mode = reader.get_bool("game", "debug_mode", false);
            game.play_threshold = reader.get_float("game", "play_threshold", 0.6f);

            container.play_preview_duration_sec = reader.get_float("container", "play_preview_duration_sec", 1.0f);
            container.play_preview_offset = reader.get_vector3("container", "play_preview_offset", 0, -1, 0);
            container.discard_preview_duration_sec = reader.get_float("container", "discard_preview_duration_sec", 1.0f);
            container.discard_preview_offset = reader.get_vector3("container", "discard_preview_offset", 0, 0, 2);
            container.discard_preview_delay_sec = reader.get_float("container", "discard_preview_delay_sec", 0.3f);

            card.glow_color = reader.get_color("card", "glow_color");
            card.glow_brightness = reader.get_float("card", "glow_brightness", 1.2f);

            placeholder.line_color = reader.get_color("placeholder", "line_color", 1, 1, 1, 1);
            placeholder.line_color_invalid = reader.get_color("placeholder", "line_color_invalid", 1, 0, 0, 1);
        }
    }
}
