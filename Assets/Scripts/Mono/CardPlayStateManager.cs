using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using CardGame.Animation;
using CardGame.Data;
using CardGame.Mono.State;

namespace CardGame.Mono
{
    public enum CardPlayState
    {
        NONE = -1,
        PLAY_CARD = 0,
        DISCARD = 1,
        DRAW = 2,
        COUNT = 3
    }

    public class CardPlayStateManager : IStateManager 
    {
        public CardPlayState state = CardPlayState.NONE;
        public int played = 0;

        bool _active = false;
        bool[] enter_state_flag = new bool[(int) CardPlayState.COUNT];

        public OrderedAnimationQueue queue = new OrderedAnimationQueue();
        public TogglePlayPreviewAnimation play_preview_animation = null;

        public DrawState draw_state = null;
        public PlayState card_play_state = null;
        public DiscardState discard_state = null;

        IState current_state = null;
        CardMono last_card = null;

        public CardPlayStateManager() 
        {
            draw_state = new DrawState(this);
            card_play_state = new PlayState(this);
            discard_state = new DiscardState(this);

            play_preview_animation = new TogglePlayPreviewAnimation();
        }

        public void set_active(bool active) {
            _active = active;
        }

        public void set_state(CardPlayState state, bool wait_for_queue = false)
        {
            this.state = state;

            if (current_state != null)
                current_state.on_exit();

            if (state == CardPlayState.DRAW)
            {
                draw_state.wait_for_queue = wait_for_queue;
                current_state = draw_state;
            }
            else if (state == CardPlayState.PLAY_CARD)
            {
                card_play_state.wait_for_queue = wait_for_queue;
                current_state = card_play_state;
            }
            else if (state == CardPlayState.DISCARD)
            {
                discard_state.wait_for_queue = wait_for_queue;
                current_state = discard_state; 
            }

            if (current_state != null)
                current_state.on_enter();
        }
        
        void persistent_state()
        {
            Player p = GameSystems.game.current_player;

            if (p.card_selection.card != null)
            {
                last_card = p.card_selection.card;
                return;
            }

            CardPlayEvent ev = GameSystems.input.card_play_event;

            if (ev.card != null)
            {
                CardMono card = ev.card;

                if (last_card != null && p.card_selection.card == null)
                    last_card.set_normal();

                // Mouse over effect.
                if (Utils.has_flag(ev.events, GameInput.OVER))
                {
                    if (last_card != null && card.id != last_card.id)
                        last_card.set_normal();

                    card.set_over();
                    last_card = card;
                }
            }

            else if (last_card != null)
            {
                last_card.set_normal();
                last_card = null;
            }
        }

        public void frame_update()
        {
            // -----------------------------------------------------------
            // -- Animations update (current player).
            // -----------------------------------------------------------

            if (GameSystems.game.enable_reposition)
            {
                for (int i = 0; i < GameSystems.reposition_list.Count; ++i) {
                    GameSystems.reposition_list[i].reposition_animation.frame_update();
                }
            }

            if (GameSystems.game.enable_elastic)
            {
                for (int i = 0; i < GameSystems.elastic_list.Count; ++i) {
                    GameSystems.elastic_list[i].elastic_animation.frame_update();
                }
            }

            persistent_state();
            
            // Execute the current state.
            if (current_state != null)
                current_state.frame_update();

            queue.run();
        }
    }
}
