using UnityEngine;

using CardGame;
using CardGame.Data;
using CardGame.Mono;
using CardGame.Animation;
using CardGame.Managers;
using CardGame.Hexgrid;
using CardGame.IO;

namespace CardGame.CGDebug
{
    public class DebugScenes
    {
        public static void load_bundles_sync()
        {
            GameSystems.bundle_manager.enqueue(BundleEnum.SHADERS);
            GameSystems.bundle_manager.enqueue(BundleEnum.MATERIALS);
            GameSystems.bundle_manager.enqueue(BundleEnum.PREFABS);
            GameSystems.bundle_manager.enqueue(BundleEnum.COMMON_TEXTURES);
            GameSystems.bundle_manager.enqueue(BundleEnum.CARD_TEXTURES);
            GameSystems.bundle_manager.enqueue(BundleEnum.UI_TEXTURES);
            GameSystems.bundle_manager.enqueue(BundleEnum.UI);
            GameSystems.bundle_manager.enqueue(BundleEnum.FONTS);
            GameSystems.bundle_manager.enqueue(BundleEnum.CARD_TEMPLATES);
            GameSystems.bundle_manager.enqueue(BundleEnum.LEVELS);
            GameSystems.bundle_manager.start_sync();
        }

        public static int debug_scene = 4;

        public static void load()
        {
            switch (DebugScenes.debug_scene)
            {
                case 1:
                    DebugScenes.basic_scene();
                    break;

                case 2:
                    DebugScenes.test_draw();
                    break;

                case 3:
                    DebugScenes.test_discard();
                    break;

                case 4:
                    DebugScenes.test_play();
                    break;
            }
        }

        public static void basic_scene()
        {
            Player p = GameSystems.game.current_player;
            CardContainerMono hand_container = p.get_container(CardLocation.LOCATION_HAND);
            CardLibrary cardlib = GameSystems.game.card_library;

            CardMono card_0 = cardlib.create_card(CardTemplateEnum.INFLUENCE, p.id);
            CardMono card_1 = cardlib.create_card(CardTemplateEnum.CREDITS, p.id);
            CardMono card_2 = cardlib.create_card(CardTemplateEnum.PROPAGANDA, p.id);
            CardMono card_3 = cardlib.create_card(CardTemplateEnum.INFLUENCE, p.id);
            CardMono card_4 = cardlib.create_card(CardTemplateEnum.CREDITS, p.id);
            CardMono card_5 = cardlib.create_card(CardTemplateEnum.INFLUENCE, p.id);
            CardMono card_6 = cardlib.create_card(CardTemplateEnum.CREDITS, p.id);
            CardMono card_7 = cardlib.create_card(CardTemplateEnum.INFLUENCE, p.id);
            CardMono card_8 = cardlib.create_card(CardTemplateEnum.PROPAGANDA, p.id);
            CardMono card_9 = cardlib.create_card(CardTemplateEnum.INFLUENCE, p.id);

            hand_container.push_card(card_0);
            hand_container.push_card(card_1);
            hand_container.push_card(card_2);
            hand_container.push_card(card_3);
            hand_container.push_card(card_4);
            hand_container.push_card(card_5);
            hand_container.push_card(card_6);
            hand_container.push_card(card_7);
            hand_container.push_card(card_8);
            hand_container.push_card(card_9);
            hand_container.update_transforms();

            Debug.Log(hand_container.cards.Count);
        }

        public static void test_draw()
        {
            Player p = GameSystems.game.current_player;
            CardContainerMono draw_pile = p.containers[(int) CardLocation.LOCATION_DRAW];
            CardLibrary cardlib = GameSystems.game.card_library;

            p.set_resource_base(Resource.CREDITS, 5);

            CardMono card_0 = cardlib.create_card(CardTemplateEnum.INFLUENCE, p.id);
            CardMono card_1 = cardlib.create_card(CardTemplateEnum.CREDITS, p.id);
            CardMono card_2 = cardlib.create_card(CardTemplateEnum.PROPAGANDA, p.id);
            CardMono card_3 = cardlib.create_card(CardTemplateEnum.INFLUENCE, p.id);
            CardMono card_4 = cardlib.create_card(CardTemplateEnum.CREDITS, p.id);
            CardMono card_5 = cardlib.create_card(CardTemplateEnum.INFLUENCE, p.id);
            CardMono card_6 = cardlib.create_card(CardTemplateEnum.CREDITS, p.id);
            CardMono card_7 = cardlib.create_card(CardTemplateEnum.INFLUENCE, p.id);
            CardMono card_8 = cardlib.create_card(CardTemplateEnum.PROPAGANDA, p.id);
            CardMono card_9 = cardlib.create_card(CardTemplateEnum.INFLUENCE, p.id);

            draw_pile.push_card(card_0);
            draw_pile.push_card(card_1);
            draw_pile.push_card(card_2);
            draw_pile.push_card(card_3);
            draw_pile.push_card(card_4);
            draw_pile.push_card(card_5);
            draw_pile.push_card(card_6);
            draw_pile.push_card(card_7);
            draw_pile.push_card(card_8);
            draw_pile.push_card(card_9);
            draw_pile.update_transforms();

            card_0.flip();
            card_1.flip();
            card_2.flip();
            card_3.flip();
            card_4.flip();
            card_5.flip();
            card_6.flip();
            card_7.flip();
            card_8.flip();
            card_9.flip();

            GameSystems.game.enter_card_play_mode();
        }

        public static void test_discard()
        {
            Player p = GameSystems.game.current_player;
            CardContainerMono hand_container = p.get_container(CardLocation.LOCATION_HAND);
            CardContainerMono draw_pile = p.containers[(int) CardLocation.LOCATION_DRAW];
            CardLibrary cardlib = GameSystems.game.card_library;

            p.set_resource_base(Resource.CREDITS, 5);

            CardMono card_0 = cardlib.create_card(CardTemplateEnum.INFLUENCE, p.id);
            CardMono card_1 = cardlib.create_card(CardTemplateEnum.CREDITS, p.id);
            CardMono card_2 = cardlib.create_card(CardTemplateEnum.PROPAGANDA, p.id);
            CardMono card_3 = cardlib.create_card(CardTemplateEnum.INFLUENCE, p.id);
            CardMono card_4 = cardlib.create_card(CardTemplateEnum.CREDITS, p.id);
            CardMono card_5 = cardlib.create_card(CardTemplateEnum.INFLUENCE, p.id);
            CardMono card_6 = cardlib.create_card(CardTemplateEnum.CREDITS, p.id);
            CardMono card_7 = cardlib.create_card(CardTemplateEnum.INFLUENCE, p.id);
            CardMono card_8 = cardlib.create_card(CardTemplateEnum.PROPAGANDA, p.id);
            CardMono card_9 = cardlib.create_card(CardTemplateEnum.INFLUENCE, p.id);

            card_0.flip();
            card_1.flip();
            card_2.flip();
            card_3.flip();
            card_4.flip();

            hand_container.push_card(card_0);
            hand_container.push_card(card_1);
            hand_container.push_card(card_2);
            hand_container.push_card(card_3);
            hand_container.push_card(card_4);
            hand_container.push_card(card_5);
            hand_container.push_card(card_6);
            hand_container.push_card(card_7);
            hand_container.push_card(card_8);
            hand_container.push_card(card_9);
            hand_container.update_transforms();

            GameSystems.game.card_play_sm.discard_state.count = 2;
            GameSystems.game.enter_card_play_mode(CardPlayState.DISCARD);
        }

        public static void test_play()
        {
            Player p = GameSystems.game.current_player;
            CardContainerMono hand_container = p.get_container(CardLocation.LOCATION_HAND);
            CardLibrary cardlib = GameSystems.game.card_library;

            p.set_resource_base(Resource.CREDITS, 5);

            CardMono card_0 = cardlib.create_card(CardTemplateEnum.INFLUENCE, p.id);
            CardMono card_1 = cardlib.create_card(CardTemplateEnum.CREDITS, p.id);
            CardMono card_2 = cardlib.create_card(CardTemplateEnum.PROPAGANDA, p.id);
            CardMono card_3 = cardlib.create_card(CardTemplateEnum.INFLUENCE, p.id);
            CardMono card_4 = cardlib.create_card(CardTemplateEnum.CREDITS, p.id);
            CardMono card_5 = cardlib.create_card(CardTemplateEnum.INFLUENCE, p.id);
            CardMono card_6 = cardlib.create_card(CardTemplateEnum.CREDITS, p.id);
            CardMono card_7 = cardlib.create_card(CardTemplateEnum.INFLUENCE, p.id);
            CardMono card_8 = cardlib.create_card(CardTemplateEnum.PROPAGANDA, p.id);
            CardMono card_9 = cardlib.create_card(CardTemplateEnum.INFLUENCE, p.id);

            hand_container.push_card(card_0);
            hand_container.push_card(card_1);
            hand_container.push_card(card_2);
            hand_container.push_card(card_3);
            hand_container.push_card(card_4);
            hand_container.push_card(card_5);
            hand_container.push_card(card_6);
            hand_container.push_card(card_7);
            hand_container.push_card(card_8);
            hand_container.push_card(card_9);
            hand_container.update_transforms();

            GameSystems.game.enter_card_play_mode(CardPlayState.PLAY_CARD);
        }

        public static HexMap test_map_play(string debug_map_name)
        {
            Player p = GameSystems.game.current_player;
            GameSystems.game.enter_map_play_mode();

            int[] player_1_tiles = new int[] {
                -8, -4, -8, -3, -8, -2, -7, -5, -7, -4, -7, -3, -7, -2, -6, -5, -6, -4, -6, -3,
                -5, -5, -5, -4, -5, -3, -4, -5, -4, -4, -4, -3, -3, -6, -3, -5, -2, -6, -2, -5,
                -1, -6, -1, -5, 0, -6, 0, -5, 1, -7, 1, -6, 1, -5, 2, -6,
            };

            int[] player_2_tiles = new int[] {
                7, 0, 7, 1, 7, 2, 8, -1, 8, 0, 8, 1, 8, 2, 9, -1, 9, 0, 9, 1,
                8, 3, 7, 4, 8, 4,
                6, 1, 6, 2, 5, 2, 5, 3, 4, 2, 3, 2,
                7, -1, 8, -2, 7, -2, 8, -3, 6, -2, 7, -3, 6, -3
            };

            HexTile.reset_counter();
            HexMap map = MapLoader.load(debug_map_name);
            map.set_position(new Vector3(0, 8.1f, 0));
            map.generate_borders();
            map.generate_region_meshes();

            // -----------------------------------------------------------
            // -- Debug data.
            // -----------------------------------------------------------

            Player p1 = GameSystems.game.get_player(PlayerID.P1);
            Player p2 = GameSystems.game.get_player(PlayerID.P2);

            p1.res.set(Resource.CREDITS, 10);
            p1.res.set(Resource.EXPANSION, 10);
            p1.res.set(Resource.RESISTANCE, 3);
            p1.res.set(Resource.INFLUENCE, 5);

            p2.res.set(Resource.CREDITS, 10);
            p2.res.set(Resource.EXPANSION, 4);
            p2.res.set(Resource.RESISTANCE, 3);
            p2.res.set(Resource.INFLUENCE, 5);

            p1.add_region(map.region(0));
            add_region_tiles(p1, map.region(0));
            p1.add_region(map.region(1));
            add_region_tiles(p1, map.region(1));

            p2.add_region(map.region(2));
            add_region_tiles(p2, map.region(2));
            p2.add_region(map.region(3));
            add_region_tiles(p2, map.region(3));
            p2.add_region(map.region(4));
            add_region_tiles(p2, map.region(4));
            p2.add_region(map.region(5));
            add_region_tiles(p2, map.region(5));

            map.region(0).set_region_state(HexRegion.STATE_DASHED);
            map.region(1).set_region_state(HexRegion.STATE_DASHED);

            Vector2Int tv = Vector2Int.zero;

            if (false)
            {
                // for (int i = 0; i < player_1_tiles.Length; i += 2)
                // {
                //     tv.x = player_1_tiles[i + 0];
                //     tv.y = player_1_tiles[i + 1];

                //     HexTile t = map.get_tile_from_real_offset(tv);
                //     p1.add_tile(t);
                // }

                // for (int i = 0; i < player_2_tiles.Length; i += 2)
                // {
                //     tv.x = player_2_tiles[i + 0];
                //     tv.y = player_2_tiles[i + 1];

                //     HexTile t = map.get_tile_from_real_offset(tv);
                //     p2.add_tile(t);
                //     t.set_tile_resistance((int) (Random.value * 5));
                // }
            }

            return map;
        }

        static void add_region_tiles(Player p, HexRegion region)
        {
            foreach (HexTile t in region.tile_list)
            {
                p.add_tile(t);
                t.set_tile_resistance((int) (Random.value * 5));
            }
        }
    }
}
