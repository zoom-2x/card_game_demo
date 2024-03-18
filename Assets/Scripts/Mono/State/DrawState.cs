using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using CardGame.Data;
using CardGame.Animation;
using CardGame.Mono.State;

namespace CardGame.Mono.State
{
    public class DrawState : IState
    {
        CardPlayStateManager sm = null;
        bool activated = false;

        public int count = 10;
        public bool wait_for_queue = false;

        public DrawState(CardPlayStateManager sm)
        {
            this.sm = sm;
        }

        public void on_enter() 
        {
            activated = false;
            GameSystems.input.enable = false;
        }

        public void on_exit()
        {}

        public void frame_update()
        {
            if (wait_for_queue)
            {
                if (sm.queue.has_running())
                    return;

                wait_for_queue = false;
            }

            Player p = GameSystems.game.current_player;

            // Run the draw animation.
            if (!activated && sm.queue.is_done(0))
            {
                Link link = p.get_link(LinkType.DRAW_PILE_HAND);
                    
                if (link.container_0.cards.Count > 0)
                {
                    activated = true;

                    if (count > link.container_0.cards.Count)
                        count = link.container_0.cards.Count;

                    StackTransferAnimation draw_animation = GameSystems.memory_manager.aquire_stack_transfer_animation();
                    draw_animation.setup(LinkType.DRAW_PILE_HAND, count, Link.FLIP);
                    sm.queue.push_animation(draw_animation, 0);
                }

                else
                {
                    StackTransferAnimation draw_animation = GameSystems.memory_manager.aquire_stack_transfer_animation();
                    draw_animation.setup(LinkType.DRAW_PILE_DISCARD_PILE, -1, Link.REVERSED);
                    sm.queue.push_animation(draw_animation, 0);
                }
            }

            // After the animation is finished switch to the play state.
            else if (sm.queue.is_done(0))
                sm.set_state(CardPlayState.PLAY_CARD);
        }
    }
}
