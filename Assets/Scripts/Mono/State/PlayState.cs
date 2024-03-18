using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using CardGame.Data;
using CardGame.Animation;
using CardGame.Mono.State;
using CardGame.Mono.UI;

namespace CardGame.Mono.State
{
    public class PlayState : IState
    {
        const int INACTIVE = 0;
        const int BASE = 1;
        const int SELECT_CARD = 2;
        const int INSPECT_CARD = 3;
        const int END_TURN = 4;

        int state = INACTIVE;

        Camera camera;
        CardPlayStateManager sm = null;

        int played = 0;
        int discard_count = 0;

        public bool wait_for_queue = false;
        float exit_delay = 0;

        System.Action switch_to_draw_callback;
        System.Action switch_to_discard_callback;

        GameUI gameui;

        CardToCursorAnimation cursor_animation = new CardToCursorAnimation();

        Vector3 inspected_position = Vector3.zero;
        Vector3 inspect_start_position = Vector3.zero;

        public PlayState(CardPlayStateManager sm)
        {
            this.sm = sm;

            camera = GameObject.Find("MAIN_CAMERA").GetComponent<Camera>();
            gameui = GameObject.Find("UI").GetComponent<GameUI>();
            gameui.end_turn_callback += end_turn;

            switch_to_discard_callback = switch_to_discard;
            switch_to_draw_callback = switch_to_draw;
        }

        public void on_enter()
        {
            Debug.Log("CardPlayState: enter PLAY_CARD");

            state = BASE;

            played = 0;
            GameSystems.input.enable = true;
            GameSystems.input.card_play_event_filter[(int) CardLocation.LOCATION_HAND] = (uint) MouseEventMask.DRAG_AND_DROP;

            if (sm.play_preview_animation.state == TogglePlayPreviewAnimation.CLOSED)
            {
                wait_for_queue = true;
                sm.queue.push_animation(sm.play_preview_animation, 0);
            }
        }

        public void on_exit()
        {
            Debug.Log("CardPlayState: exit PLAY_CARD");
            state = INACTIVE;

            if (sm.play_preview_animation.state == TogglePlayPreviewAnimation.OPEN)
            {
                sm.queue.push_animation(sm.play_preview_animation, 0);
                Debug.Log($"exit_delay: {exit_delay}");

                if (exit_delay > 0)
                {
                    sm.queue.push_delay(exit_delay, 0);
                    exit_delay = 0;
                }
            }
        }

        void end_turn()
        {
            if (state == INACTIVE || state == END_TURN)
                return;

            if (sm.queue.is_done(1))
                state = END_TURN;
        }

        void base_state()
        {
            Player p = GameSystems.game.current_player;

            CardPlayEvent ev = GameSystems.input.card_play_event;
            CardSelection selection = p.card_selection;

            if (GameSystems.input.key_down(KeyboardEnum.KEY_ENTER)) {
                end_turn();
            }

            else if (ev.card != null && ev.card.location == CardLocation.LOCATION_HAND)
            {
                if (Utils.has_flag(ev.events, GameInput.LEFT_BUTTON_DOWN))
                {
                    state = SELECT_CARD;
                    selection.card = ev.card;
                }

                else if (Utils.has_flag(ev.events, GameInput.RIGHT_BUTTON_DOWN))
                {
                    state = INSPECT_CARD;
                    GameSystems.game.enable_elastic = false;
                    selection.card = ev.card;
                    inspect_start_position = selection.card.transform.position;
                }
            }

            p.card_selection = selection;
        }

        void play_card_animation(CardMono played_card)
        {
            Player p = GameSystems.game.current_player;
            played++;

            Link link = p.get_link(LinkType.HAND_PLAY_PREVIEW);
            TransferAnimation a = link.transfer_float(played_card, 0);
            sm.queue.push_animation(a, 1);
        }

        void select_card_state()
        {
            Player p = GameSystems.game.current_player;

            CardPlayEvent ev = GameSystems.input.card_play_event;
            CardSelection selection = p.card_selection;

            if (played == 3)
            {
                Debug.Log($"selection: {selection}");
                Debug.Log($"card: {selection.card}");
            }

            Utils.attach_object_to_mouse(GameSystems.game.scene_camera, selection.card.transform);
            selection.local_position = selection.card.transform.localPosition;

            if (Utils.has_flag(ev.events, GameInput.RIGHT_BUTTON_DOWN))
            {
                Debug.Log("base");
                state = BASE;

                sm.queue.clear(0);
                selection.card = null;
                selection.local_position = Vector3.zero;
            }

            else if (Utils.has_flag(ev.events, GameInput.LEFT_BUTTON_UP) ||
                     Utils.has_flag(ev.events, GameInput.LEFT_BUTTON_DOWN))
            {
                bool valid_play = GameRules.beyond_play_threshold(selection.local_position);

                if (valid_play)
                {
                    play_card_animation(selection.card);
                    selection.card = null;

                    // Temporar.
                    discard_count = 3;
                    bool card_has_discard = (played % discard_count) == 0 ? true : false;

                    // If a card has a discard effect then block any card interaction
                    // and switch states when the animations finish.
                    if (card_has_discard)
                    {
                        GameSystems.input.enable = false;
                        sm.queue.register(1, switch_to_discard_callback);
                    }

                    state = BASE;
                }
            }

            p.card_selection = selection;
        }

        CardInspectAnimation inspect_animation = new CardInspectAnimation(); 
        
        void inspect_card_state()
        {
            Player p = GameSystems.game.current_player;

            CardSelection selection = p.card_selection;
            CardPlayEvent ev = GameSystems.input.card_play_event;

            if (inspect_animation.state == 0)
            {
                Debug.Log("inspect");

                inspect_animation.state = 1;
                inspect_animation.t = 0;
                inspect_animation.time = 0;

                inspect_animation.card = selection.card;
                inspect_animation.start = selection.card.transform.position;
                inspect_animation.end = camera.transform.position + camera.transform.forward * GameConfig.game.card_zoom;
                inspect_animation.set_duration(GameConfig.game.inspect_card_forward_duration);

                sm.queue.push_animation(inspect_animation, 1);
            }

            // selection.card.transform.position = camera.transform.position + camera.transform.forward * 1.2f;
            // selection.local_position = selection.card.transform.localPosition;
            
            if (sm.queue.is_done(1) && inspect_animation.state == 1 && Utils.has_flag(ev.events, GameInput.LEFT_BUTTON_DOWN | GameInput.RIGHT_BUTTON_DOWN))
            {
                inspect_animation.state = 2;

                inspect_animation.t = 0;
                inspect_animation.time = 0;

                inspect_animation.card = selection.card;
                inspect_animation.start = selection.card.transform.position;
                inspect_animation.end = inspect_start_position;
                inspect_animation.set_duration(GameConfig.game.inspect_card_backward_duration);

                sm.queue.push_animation(inspect_animation, 1);
            }

            else if (inspect_animation.state == 2 && sm.queue.is_done(1))
            {
                Debug.Log("return from inspect");

                inspect_animation.state = 0;

                GameSystems.game.enable_elastic = true;
                state = BASE;

                selection.card.transform.position = inspect_start_position;

                sm.queue.clear(0);
                selection.card = null;
                selection.local_position = Vector3.zero;
            }

            p.card_selection = selection;
        }

        void end_turn_state()
        {
            Player p = GameSystems.game.current_player;

            StackTransferAnimation preview_discard_animation = GameSystems.memory_manager.aquire_stack_transfer_animation();
            StackTransferAnimation hand_discard_animation = GameSystems.memory_manager.aquire_stack_transfer_animation();

            preview_discard_animation.setup(LinkType.PLAY_PREVIEW_DISCARD_PILE, -1, Link.FLIP);
            hand_discard_animation.setup(LinkType.HAND_DISCARD_PILE, -1, Link.FLIP);

            float delay = 0;

            if (p.get_container_count(CardLocation.LOCATION_PLAY_PREVIEW) > 0)
                delay = 0.3f;

            sm.queue.push_animation(preview_discard_animation, 0);
            sm.queue.push_animation(hand_discard_animation, 0, delay);

            sm.queue.register(0, switch_to_draw_callback);

            GameSystems.input.enable = false;
            state = BASE;
        }

        public void frame_update()
        {
            if (!GameSystems.input.enable)
                return;

            if (wait_for_queue)
            {
                if (sm.queue.has_running())
                    return;

                wait_for_queue = false;
            }

            Player p = GameSystems.game.current_player;

            // No credits.
            if (p.get_resource_total(Resource.CREDITS) == 0)
                return;

            // State switches.
            switch (state)
            {
                case BASE:
                    base_state();
                    break;

                case SELECT_CARD:
                    select_card_state();
                    break;

                case INSPECT_CARD:
                    inspect_card_state();
                    break;

                case END_TURN:
                    end_turn_state();
                    break;
            }
        }

        // -----------------------------------------------------------
        // -- Callbacks.
        // -----------------------------------------------------------

        void switch_to_draw()
        {
            Player p = GameSystems.game.current_player;
            Link link = p.get_link(LinkType.DRAW_PILE_HAND);

            exit_delay = 0.0f;
            sm.draw_state.count = 5;
            sm.set_state(CardPlayState.DRAW);
        }
        
        void switch_to_discard()
        {
            sm.discard_state.count = discard_count;
            exit_delay = 0.8f;
            sm.set_state(CardPlayState.DISCARD);
        }
    }
}
