using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using CardGame.Data;
using CardGame.Mono;
using CardGame.Hexgrid;

namespace CardGame
{
    public class GameRules
    {
        public static bool beyond_play_threshold(Vector3 pos) {
            return pos.z >= GameConfig.game.play_threshold;
        }

        public static void find_border_tiles()
        {}

        public static int get_expansion_strength(Player p, HexTile t) {
            return t.owner != PlayerID.NONE ? GameConfig.game.expansion_base_strength : GameConfig.game.expansion_base_empty_strength;
        }

        public static float random() {
            return Random.value;
        }

        // Input values are evaluated in the range 0 to 10.
        public static bool roll(int attack_strength, int defense_strength)
        {
             int final_strength = (attack_strength - defense_strength) * 10;

             if (final_strength < 0)
                 final_strength = 0;
             else if (final_strength > 100)
                 final_strength = 100;

            int check = (int) (GameRules.random() * 100);

            return check <= final_strength;
        }

        public static void capture_tile(HexTile target, Player player)
        {
            if (target == null || player == null)
                return;

            if (target.owner != PlayerID.NONE)
            {
                Player target_player =  GameSystems.game.get_player(target.owner);
                target_player.remove_tile(target);
            }

            player.add_tile(target);
        }
    }
}
