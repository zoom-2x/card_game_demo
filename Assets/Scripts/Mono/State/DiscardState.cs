using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using CardGame.Data;
using CardGame.Animation;
using CardGame.Mono.UI;

namespace CardGame.Mono.State
{
    public class DiscardState : IState
    {
        const int INACTIVE = 0;
        const int ACTIVE = 1;

        int state = INACTIVE;

        public int count = 0;
        int played = 0;
        public bool wait_for_queue = false;

        CardPlayStateManager sm = null;
        GameUI gameui;
        ToggleDiscardPreviewAnimation discard_preview_animation = null;

        public DiscardState(CardPlayStateManager sm)
        {
            this.sm = sm;

            gameui = GameObject.Find("UI").GetComponent<GameUI>();
            gameui.end_turn_callback += end_turn;
            discard_preview_animation = new ToggleDiscardPreviewAnimation();
        }

        void init_placeholders()
        {}

        void clean_placeholders()
        {}

        void end_turn()
        {
            Player p = GameSystems.game.current_player; 

            if (state == INACTIVE || finished)
                return;

            if ((p.get_container_count(CardLocation.LOCATION_HAND) == 0 || played == count) && 
                 sm.queue.is_done(0))
            {
                finished = true;

                StackTransferAnimation discard_animation = GameSystems.memory_manager.aquire_stack_transfer_animation();
                discard_animation.setup(LinkType.DISCARD_PREVIEW_DISCARD_PILE, -1, Link.FLIP);
                sm.queue.push_animation(discard_animation, 0);
            }
        }

        public void on_enter()
        {
            Debug.Log("CardPlayState: enter DISCARD");
            GameSystems.input.card_play_event_filter[(int) CardLocation.LOCATION_HAND] = (uint) MouseEventMask.CLICK;
            GameSystems.input.enable = true;

            state = ACTIVE;
            finished = false;

            if (discard_preview_animation.state == ToggleDiscardPreviewAnimation.CLOSED)
            {
                played = 0;
                wait_for_queue = true;
                discard_preview_animation.count = count;
                sm.queue.push_animation(discard_preview_animation, 0);
            }
        }

        public void on_exit()
        {
            state = INACTIVE;

            if (discard_preview_animation.state == ToggleDiscardPreviewAnimation.OPEN)
            {
                sm.queue.push_animation(discard_preview_animation, 0);
                sm.queue.push_delay(0.5f, 0);
            }
        }

        bool forward = false;
        bool backward = false;
        bool finished = false;

        public void frame_update()
        {
            if (wait_for_queue)
            {
                if (sm.queue.has_running())
                    return;

                wait_for_queue = false;
            }

            if (!finished)
            {
                if (sm.queue.is_done(0))
                {
                    forward = false;
                    backward = false;
                }

                Player p = GameSystems.game.current_player; 
                CardPlayEvent ev = GameSystems.input.card_play_event;
                
                // Hand to preview.
                if (!backward && played < count && ev.card != null && 
                    (ev.events & GameInput.LEFT_BUTTON_DOWN) > 0 &&
                    ev.card.location == CardLocation.LOCATION_HAND)
                {
                    forward = true;
                    played++;

                    CardMono card = ev.card;

                    Link link = p.get_link(LinkType.HAND_DISCARD_PREVIEW);
                    TransferAnimation a = link.get_transfer_animation(card, 0);

                    sm.queue.push_animation(a, 0);

                    discard_preview_animation.placeholder_status(played);
                }

                // Preview to hand.
                else if (!forward && played > 0 && ev.card != null && 
                         (ev.events & GameInput.LEFT_BUTTON_DOWN) > 0 &&
                         ev.card.location == CardLocation.LOCATION_DISCARD_PREVIEW)
                {
                    backward = true;
                    played--;

                    CardMono card = ev.card;

                    Link link = p.get_link(LinkType.HAND_DISCARD_PREVIEW);
                    TransferAnimation a = link.get_transfer_animation(card, Link.REVERSED);
                    // a.use_cache = true;
                    
                    CardContainerMono discard_preview = link.container_1;
                    discard_preview.update_transforms();

                    sm.queue.push_animation(a, 0);

                    discard_preview_animation.placeholder_status(played);
                }

                // Start the discard animation.
                if (GameSystems.input.key_down(KeyboardEnum.KEY_ENTER))
                    end_turn(); 
            }

            // Wait for the discard animation to finish.
            else if (sm.queue.is_done(0))
                sm.set_state(CardPlayState.PLAY_CARD);
        }
    }
}
