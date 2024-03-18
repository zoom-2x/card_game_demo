using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using CardGame.Data;
using CardGame.Animation;

namespace CardGame.Data
{
    public struct QueueUnit
    {
        public AnimationAbstract animation;
        public float delay;
    }

    public class OrderedAnimationQueue
    {
        public const int QUEUE_LEVELS = 5;

        List<QueueUnit>[] queues = new List<QueueUnit>[QUEUE_LEVELS];
        CommandEvent[] on_finished_event = new CommandEvent[QUEUE_LEVELS];
        float[] forced_delay = new float[QUEUE_LEVELS];

        public OrderedAnimationQueue()
        {
            for (int i = 0; i < QUEUE_LEVELS; ++i)
            {
                queues[i] = new List<QueueUnit>();
                on_finished_event[i] = new CommandEvent();
            }
        }

        public void register(int level, System.Action callback)
        {
            if (level < 0 || level >= QUEUE_LEVELS)
                return;

            on_finished_event[level].register(callback);
        }

        public void clear(int level)
        {
            if (level < 0 || level >= QUEUE_LEVELS)
                return;

            List<QueueUnit> queue = queues[level];

            foreach (QueueUnit unit in queue) {
                release_animation(unit.animation);
            }

            queue.Clear();
        }

        public void clear_all()
        {
            for (int i = 0; i < QUEUE_LEVELS; ++i) {
                clear(i);
            }
        }

        public void push_animation(AnimationAbstract a, int level, float delay = 0)
        {
            if (level >= 0 && level < QUEUE_LEVELS)
            {
                QueueUnit unit = new QueueUnit();

                unit.animation = a;
                unit.delay = forced_delay[level] > 0 ? forced_delay[level] : delay;
                forced_delay[level] = 0;

                queues[level].Add(unit);
            }
        }

        public void push_delay(float delay, int level)
        {
            if (level >= 0 && level < QUEUE_LEVELS)
                forced_delay[level] = delay;
                // push_animation(null, level, delay);
        }

        public bool is_done(int level = QUEUE_LEVELS - 1)
        {
            if (level < 0 || level >= QUEUE_LEVELS)
            {
                Debug.LogWarning($"OrderedAnimationQueue: Invalid queue level {level} !");
                return true;
            }

            return queues[level].Count == 0;
        }

        public bool has_running()
        {
            for (int i = 0; i < QUEUE_LEVELS; ++i) 
            {
                if (queues[i].Count > 0)
                    return true;
            }

            return false;
        }

        void release_animation(AnimationAbstract animation)
        {
            if (animation is TransferAnimation)
                GameSystems.memory_manager.release_transfer_animation(animation as TransferAnimation);
            else if (animation is StackTransferAnimation)
                GameSystems.memory_manager.release_stack_transfer_animation(animation as StackTransferAnimation);
        }

        public bool run()
        {
            if (!has_running())
                return true;

            for (int i = 0; i < QUEUE_LEVELS; ++i)
            {
                List<QueueUnit> queue = queues[i];
                int qi = 0;

                while (qi < queue.Count)
                {
                    bool next = true;
                    QueueUnit unit = queue[qi];

                    if (unit.delay > 0)
                    {
                        unit.delay -= Time.deltaTime;
                        queue[qi] = unit;
                    }
                    else
                    {
                        bool finished = true;

                        if (unit.animation != null)
                            finished = unit.animation.frame_update();

                        if (finished)
                        {
                            next = false;
                            queue.RemoveAt(qi);
                            release_animation(unit.animation);
                        }
                    }

                    if (next)
                        qi++;
                }

                // The queue has no animations running.
                if (queue.Count == 0)
                {
                    CommandEvent e = on_finished_event[i];

                    if (!e.empty)
                    {
                        e.trigger();
                        e.clear();
                    }
                }
            }

            return false;
        }
    }
}
