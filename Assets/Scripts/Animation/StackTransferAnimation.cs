using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using CardGame.Mono;
using CardGame.Data;

namespace CardGame.Animation
{
    internal struct Timer
    {
        public bool created;
        public float t;
        public int index;
        public bool random;
    }

    // Used to transfer a group of cards.
    public class StackTransferAnimation : AnimationAbstract
    {
        List<Timer> timers = new List<Timer>();
        List<AnimationAbstract> queue = new List<AnimationAbstract>();
        Link link = null;

        uint transfer_mask = 0;
        CardContainerMono transfer_source = null;

        public void setup(LinkType type, int transfer_count = -1, uint mask = 0, bool random = false)
        {
            timers.Clear();
            queue.Clear();

            transfer_mask = mask;

            Player p = GameSystems.game.current_player;
            link = p.get_link(type);
            // Debug.Log($"link: {type}");

            transfer_source = link.container_0;

            if ((mask & Link.REVERSED) > 0)
                transfer_source = link.container_1;

            if (transfer_count == -1 ||
                transfer_source.cards.Count == 0 ||
                transfer_count > transfer_source.cards.Count)
            {
                transfer_count = transfer_source.cards.Count;
            }

            // Setup the timers.
            for (int i = 0; i < transfer_count; ++i)
            {
                Timer t = new Timer() {created = false, t = -i * link.offset, random = random};
                timers.Add(t);
            }
        }

        public void setup(LinkType type, List<int> selection, uint mask = 0)
        {
            timers.Clear();
            queue.Clear();

            if (selection == null || selection.Count == 0)
                return;

            transfer_mask = mask;

            Player p = GameSystems.game.current_player;
            link = p.get_link(type);

            transfer_source = link.container_0;

            if ((mask & Link.REVERSED) > 0)
                transfer_source = link.container_1;

            // Setup the timers.
            for (int i = 0; i < selection.Count; ++i)
            {
                if (selection[i] >= 0 && selection[i] < transfer_source.cards.Count)
                {
                    Timer t = new Timer() {created = false, t = -i * link.offset, index = selection[i],random = false};
                    timers.Add(t);
                }
            }
        }

        public override bool frame_update()
        {
            // Delayed TransferAnimation creation.
            for (int i = 0; i < timers.Count; ++i)
            {
                Timer timer = timers[i];
                timer.t += Time.deltaTime;

                if (!timer.created && timer.t >= 0)
                {
                    if (transfer_source.cards.Count > 0)
                    {
                        int last = transfer_source.cards.Count - 1;

                        if (timer.random)
                            last = (int) (Random.value * last);

                        if (last >= 0)
                        {
                            CardMono card = transfer_source.cards[last];
                            queue.Add(link.get_transfer_animation(card, transfer_mask));
                        }
                    }

                    timer.created =  true;
                }

                timers[i] = timer;
            }

            int j = 0;

            // TransferAnimation execution.
            if (queue.Count == 0)
            {
                on_finished_event.trigger();
                on_finished_event.clear();

                return true;
            }

            while (true)
            {
                AnimationAbstract a = queue[j];
                bool finished = a.frame_update();

                if (finished)
                {
                    queue.RemoveAt(j);

                    if (a is TransferAnimation)
                        GameSystems.memory_manager.release_transfer_animation(a as TransferAnimation);
                }
                else
                    j++;

                if (j == queue.Count)
                    break;
            }

            return false;
        }
    }
}
